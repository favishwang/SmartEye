using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartEye.Feature
{
    /// <summary>
    /// ONNX Runtime로 YOLOv5/YOLOv8을 실행하여 이미지에서 차량을 검출합니다.
    /// FP16/FP32 모델 자동 지원, yolov8n.onnx 우선 로드 후 yolov5n.onnx 시도.
    /// </summary>
    public class CarDetector : IDisposable
    {
        private InferenceSession? _session;
        private bool _disposed;
        private bool _useFloat16 = true; // FP16 시도 후 실패 시 FP32로 전환

        /// <summary>로그 메시지 전달용 콜백</summary>
        public Action<string>? OnLog { get; set; }

        /// <summary>최소 신뢰도 (0~1)</summary>
        public float MinConfidence { get; set; } = 0.25f;

        /// <summary>NMS 임계값</summary>
        public float NmsThreshold { get; set; } = 0.5f;

        private const int InputSize = 640;
        private static readonly int[] VehicleClassIds = { 2, 3, 5, 7 }; // car, motorcycle, bus, truck (COCO)

        private static readonly string[] ModelPaths = new[]
        {
            "yolov8n.onnx",
            "yolov5n.onnx",
        };

        private const long MinOnnxSize = 100_000;

        private bool EnsureLoaded()
        {
            if (_session != null) return true;

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var modelName in ModelPaths)
            {
                var paths = new[]
                {
                    Path.Combine(baseDir, "Data", modelName),
                    Path.Combine(baseDir, modelName),
                    Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "CarDetectionTest", "Data", modelName)),
                };

                foreach (var path in paths)
                {
                    if (!File.Exists(path)) continue;

                    var fi = new FileInfo(path);
                    if (fi.Length < MinOnnxSize)
                    {
                        Log($"[CarDetector] 파일 크기 부족 ({fi.Length} bytes): {path}");
                        continue;
                    }

                    try
                    {
                        var bytes = File.ReadAllBytes(path);
                        _session = new InferenceSession(bytes);
                        _useFloat16 = true;
                        Log($"[CarDetector] ONNX 로드 완료: {modelName} ({fi.Length / 1024}KB)");
                        return true;
                    }
                    catch (OnnxRuntimeException ex)
                    {
                        Log($"[CarDetector] 모델 로드 실패 ({path}, {fi.Length} bytes): {ex.Message}");
                        if (ex.Message.Contains("InvalidProtobuf"))
                            Log("[CarDetector] ONNX 파일이 손상되었거나 형식이 맞지 않습니다. 모델을 다시 다운로드하세요.");
                    }
                    catch (Exception ex)
                    {
                        Log($"[CarDetector] 모델 로드 실패 ({path}): {ex.Message}");
                    }
                }
            }

            Log("[CarDetector] yolov8n.onnx 또는 yolov5n.onnx를 Data 폴더에 배치하세요.");
            return false;
        }

        /// <summary>
        /// BGR 이미지에서 차량을 검출하여 ROI 사각형 목록을 반환합니다.
        /// </summary>
        public IReadOnlyList<Rect> Detect(Mat bgrImage)
        {
            if (bgrImage == null || bgrImage.Empty())
                return Array.Empty<Rect>();

            if (!EnsureLoaded())
                return Array.Empty<Rect>();

            try
            {
                var (padded, padX, padY, scale) = Letterbox(bgrImage);
                using (padded)
                {
                    DenseTensor<float>? output = RunInference(padded);
                    if (output == null)
                        return Array.Empty<Rect>();

                    var rects = PostProcess(bgrImage, output, padX, padY, scale);
                    Log($"[CarDetector] YOLO {rects.Count}대 검출");
                    return rects;
                }
            }
            catch (Exception ex)
            {
                Log($"[CarDetector] 검출 오류: {ex.Message}");
                return Array.Empty<Rect>();
            }
        }

        private DenseTensor<float>? RunInference(Mat padded)
        {
            NamedOnnxValue inputValue;
            if (_useFloat16)
            {
                var tensor = MatToInputTensorFloat16(padded);
                inputValue = NamedOnnxValue.CreateFromTensor(_session!.InputNames[0], tensor);
            }
            else
            {
                var tensor = MatToInputTensorFloat32(padded);
                inputValue = NamedOnnxValue.CreateFromTensor(_session!.InputNames[0], tensor);
            }

            try
            {
                var inputs = new List<NamedOnnxValue> { inputValue };
                using var results = _session!.Run(inputs);
                return GetOutputAsFloatTensor(results.First());
            }
            catch (OnnxRuntimeException ex) when (ex.Message.Contains("Float") && ex.Message.Contains("Float16"))
            {
                Log($"[CarDetector] FP16/FP32 전환 후 재시도");
                _useFloat16 = !_useFloat16;
                return RunInference(padded);
            }
        }

        /// <summary>BGR Mat을 NCHW RGB Float16 텐서로 변환 (0~1 정규화)</summary>
        private static DenseTensor<Float16> MatToInputTensorFloat16(Mat bgr)
        {
            var tensor = new DenseTensor<Float16>(new[] { 1, 3, InputSize, InputSize });
            var span = tensor.Buffer.Span;

            for (int y = 0; y < InputSize; y++)
            {
                for (int x = 0; x < InputSize; x++)
                {
                    var idx = bgr.At<OpenCvSharp.Vec3b>(y, x);
                    int baseOff = y * InputSize + x;
                    span[0 * InputSize * InputSize + baseOff] = ToFloat16(idx.Item2 / 255f);
                    span[1 * InputSize * InputSize + baseOff] = ToFloat16(idx.Item1 / 255f);
                    span[2 * InputSize * InputSize + baseOff] = ToFloat16(idx.Item0 / 255f);
                }
            }
            return tensor;
        }

        /// <summary>BGR Mat을 NCHW RGB Float32 텐서로 변환 (0~1 정규화)</summary>
        private static DenseTensor<float> MatToInputTensorFloat32(Mat bgr)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, InputSize, InputSize });
            var span = tensor.Buffer.Span;

            for (int y = 0; y < InputSize; y++)
            {
                for (int x = 0; x < InputSize; x++)
                {
                    var idx = bgr.At<OpenCvSharp.Vec3b>(y, x);
                    int baseOff = y * InputSize + x;
                    span[0 * InputSize * InputSize + baseOff] = idx.Item2 / 255f;
                    span[1 * InputSize * InputSize + baseOff] = idx.Item1 / 255f;
                    span[2 * InputSize * InputSize + baseOff] = idx.Item0 / 255f;
                }
            }
            return tensor;
        }

        private static Float16 ToFloat16(float v)
        {
            return new Float16((ushort)BitConverter.HalfToInt16Bits((Half)v));
        }

        private static DenseTensor<float>? GetOutputAsFloatTensor(NamedOnnxValue? value)
        {
            var v = value?.Value;
            if (v is DenseTensor<float> ft) return ft;
            if (v is DenseTensor<Float16> ht)
            {
                var dims = ht.Dimensions.ToArray();
                var result = new DenseTensor<float>(dims.Select(d => (int)d).ToArray());
                var src = ht.Buffer.Span;
                var dst = result.Buffer.Span;
                for (int i = 0; i < src.Length; i++)
                    dst[i] = (float)(Half)BitConverter.Int16BitsToHalf((short)src[i].value);
                return result;
            }
            return null;
        }

        private static (Mat padded, float padX, float padY, float scale) Letterbox(Mat src)
        {
            float r = Math.Min((float)InputSize / src.Width, (float)InputSize / src.Height);
            var newW = (int)(src.Width * r);
            var newH = (int)(src.Height * r);
            var padX = (InputSize - newW) / 2f;
            var padY = (InputSize - newH) / 2f;

            using var resized = new Mat();
            Cv2.Resize(src, resized, new OpenCvSharp.Size(newW, newH));
            var padded = new Mat(InputSize, InputSize, MatType.CV_8UC3, new Scalar(114, 114, 114));
            var roi = new OpenCvSharp.Rect((int)padX, (int)padY, newW, newH);
            resized.CopyTo(new Mat(padded, roi));
            return (padded, padX, padY, r);
        }

        /// <summary>YOLOv5 [1,N,85] / YOLOv8 [1,84,8400] 출력 형식 자동 판별</summary>
        private List<Rect> PostProcess(Mat image, DenseTensor<float> output, float padX, float padY, float scale)
        {
            var dims = output.Dimensions.ToArray();
            int numProposals, numAttrs;
            if (dims.Length == 3)
            {
                numProposals = (int)dims[1];
                numAttrs = (int)dims[2];
            }
            else if (dims.Length == 2)
            {
                numProposals = (int)dims[0];
                numAttrs = (int)dims[1];
            }
            else
                return new List<Rect>();

            var data = output.Buffer.Span;

            if (numAttrs >= 85)
                return PostProcessYolov5(image, data, numProposals, numAttrs, padX, padY, scale);
            if (numAttrs >= 84)
                return PostProcessYolov8(image, data, numProposals, numAttrs, padX, padY, scale);

            return new List<Rect>();
        }

        /// <summary>YOLOv5: [1, N, 85] (x,y,w,h, obj, class0..79)</summary>
        private List<Rect> PostProcessYolov5(Mat image, ReadOnlySpan<float> data, int numProposals, int numAttrs, float padX, float padY, float scale)
        {
            var rects = new List<Rect>();
            var confidences = new List<float>();

            for (int i = 0; i < numProposals; i++)
            {
                int baseIdx = i * numAttrs;
                float cx = data[baseIdx + 0];
                float cy = data[baseIdx + 1];
                float w = data[baseIdx + 2];
                float h = data[baseIdx + 3];
                float obj = data[baseIdx + 4];

                float bestScore = 0;
                foreach (var classId in VehicleClassIds)
                {
                    if (classId >= 80) continue;
                    float clsScore = data[baseIdx + 5 + classId];
                    float score = obj * clsScore;
                    if (score > bestScore) bestScore = score;
                }

                if (bestScore >= MinConfidence)
                    AddRect(image, rects, confidences, cx, cy, w, h, padX, padY, scale, bestScore);
            }

            return NmsAndSort(rects, confidences);
        }

        /// <summary>YOLOv8: [1, 84, N] (cx,cy,w,h + 80 class scores)</summary>
        private List<Rect> PostProcessYolov8(Mat image, ReadOnlySpan<float> data, int numProposals, int numChannels, float padX, float padY, float scale)
        {
            var rects = new List<Rect>();
            var confidences = new List<float>();

            bool transposed = numProposals > numChannels;

            for (int i = 0; i < numProposals; i++)
            {
                float cx, cy, w, h;
                if (transposed)
                {
                    cx = data[i * numChannels + 0];
                    cy = data[i * numChannels + 1];
                    w = data[i * numChannels + 2];
                    h = data[i * numChannels + 3];
                }
                else
                {
                    cx = data[0 * numProposals + i];
                    cy = data[1 * numProposals + i];
                    w = data[2 * numProposals + i];
                    h = data[3 * numProposals + i];
                }

                float bestScore = 0;
                foreach (var classId in VehicleClassIds)
                {
                    if (classId >= 80) continue;
                    var score = transposed
                        ? data[i * numChannels + 4 + classId]
                        : data[(4 + classId) * numProposals + i];
                    if (score > bestScore) bestScore = score;
                }

                if (bestScore >= MinConfidence)
                    AddRect(image, rects, confidences, cx, cy, w, h, padX, padY, scale, bestScore);
            }

            return NmsAndSort(rects, confidences);
        }

        private static void AddRect(Mat image, List<Rect> rects, List<float> confidences,
            float cx, float cy, float w, float h, float padX, float padY, float scale, float score)
        {
            var cxOrig = (cx - padX) / scale;
            var cyOrig = (cy - padY) / scale;
            var bw = w / scale;
            var bh = h / scale;
            var x1 = (int)(cxOrig - bw / 2);
            var y1 = (int)(cyOrig - bh / 2);
            var width = (int)bw;
            var height = (int)bh;

            x1 = Math.Max(0, Math.Min(x1, image.Width - 1));
            y1 = Math.Max(0, Math.Min(y1, image.Height - 1));
            width = Math.Max(1, Math.Min(width, image.Width - x1));
            height = Math.Max(1, Math.Min(height, image.Height - y1));

            rects.Add(new Rect(x1, y1, width, height));
            confidences.Add(score);
        }

        private List<Rect> NmsAndSort(List<Rect> rects, List<float> confidences)
        {
            if (rects.Count == 0) return rects;
            CvDnn.NMSBoxes(rects, confidences, MinConfidence, NmsThreshold, out int[] indices);
            return indices.Select(i => rects[i]).OrderByDescending(r => r.Width * r.Height).ToList();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _session?.Dispose();
            _session = null;
            _disposed = true;
        }

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
