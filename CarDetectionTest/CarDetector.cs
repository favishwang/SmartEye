using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarDetectionTest
{
    /// <summary>
    /// ONNX Runtime로 YOLOv5를 실행하여 이미지에서 차량을 검출합니다.
    /// </summary>
    public class CarDetector : IDisposable
    {
        private InferenceSession? _session;
        private bool _disposed;

        public Action<string>? OnLog { get; set; }
        public float MinConfidence { get; set; } = 0.25f;
        public float NmsThreshold { get; set; } = 0.5f;

        private const int InputSize = 640;
        /// <summary>COCO: car=2, motorcycle=3, bus=5, truck=7</summary>
        private static readonly int[] VehicleClassIds = { 2, 3, 5, 7 };

        private bool EnsureLoaded()
        {
            if (_session != null) return true;

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var paths = new[]
            {
                Path.Combine(baseDir, "Data", "yolov5n.onnx"),
                Path.Combine(baseDir, "yolov5n.onnx"),
            };

            foreach (var path in paths)
            {
                if (!File.Exists(path)) continue;
                try
                {
                    _session = new InferenceSession(path);
                    OnLog?.Invoke($"[CarDetector] YOLOv5 ONNX Runtime 로드: {path}");
                    return true;
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"[CarDetector] 모델 로드 실패: {ex.Message}");
                }
            }

            OnLog?.Invoke("[CarDetector] yolov5n.onnx를 찾을 수 없습니다. Data 폴더에 배치하세요.");
            return false;
        }

        public IReadOnlyList<OpenCvSharp.Rect> Detect(Mat bgrImage)
        {
            if (bgrImage == null || bgrImage.Empty())
                return Array.Empty<OpenCvSharp.Rect>();

            if (!EnsureLoaded())
                return Array.Empty<OpenCvSharp.Rect>();

            try
            {
                var (padded, padX, padY, scale) = Letterbox(bgrImage);
                using (padded)
                {
                    var inputTensor = MatToInputTensorFloat16(padded);
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor(_session!.InputNames[0], inputTensor)
                    };

                    using var results = _session.Run(inputs);
                    var output = GetOutputAsFloatTensor(results.First());
                    if (output == null)
                        return Array.Empty<OpenCvSharp.Rect>();
                    var rects = PostProcessYolov5(bgrImage, output, padX, padY, scale);
                    OnLog?.Invoke($"[CarDetector] {rects.Count}대 검출");
                    return rects;
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"[CarDetector] 검출 오류: {ex.Message}");
                return Array.Empty<OpenCvSharp.Rect>();
            }
        }

        /// <summary>BGR Mat을 NCHW RGB Float16 텐서로 변환 (0~1 정규화, FP16 모델용)</summary>
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

        private static Float16 ToFloat16(float v)
        {
            return new Float16((ushort)BitConverter.HalfToInt16Bits((Half)v));
        }

        /// <summary>출력 텐서를 float 배열로 변환 (Float16/float 모두 처리)</summary>
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

        /// <summary>YOLOv5 출력 형식: [1, numProposals, 85] (x,y,w,h, obj, class0..79)</summary>
        private List<OpenCvSharp.Rect> PostProcessYolov5(Mat image, DenseTensor<float> output, float padX, float padY, float scale)
        {
            var rects = new List<OpenCvSharp.Rect>();
            var confidences = new List<float>();

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
                return rects;

            if (numAttrs < 85) return rects;

            var data = output.Buffer.Span;

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

                    rects.Add(new OpenCvSharp.Rect(x1, y1, width, height));
                    confidences.Add(bestScore);
                }
            }

            if (rects.Count == 0) return rects;
            OpenCvSharp.Dnn.CvDnn.NMSBoxes(rects, confidences, MinConfidence, NmsThreshold, out int[] indices);
            return indices.Select(i => rects[i]).OrderByDescending(r => r.Width * r.Height).ToList();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _session?.Dispose();
            _session = null;
            _disposed = true;
        }
    }
}
