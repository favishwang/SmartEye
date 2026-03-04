using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SmartEye.Feature
{
    /// <summary>
    /// Raw RGB24 바이트를 PictureBox용 Bitmap으로 변환합니다.
    /// </summary>
    public class ImageProcessor
    {
        /// <summary>로그 메시지 전달용 콜백</summary>
        public Action<string>? OnLog { get; set; }

        /// <summary>적용할 효과 목록 (순서대로 적용)</summary>
        public IReadOnlyList<Effects.IImageEffect> ActiveEffects { get; set; } = Array.Empty<Effects.IImageEffect>();

        /// <summary>
        /// Raw RGB24 바이트를 Bitmap으로 변환합니다.
        /// </summary>
        public Bitmap? ProcessToBitmap(byte[] rawRgb24, int width, int height)
        {
            int expectedSize = width * height * 3;
            if (rawRgb24 == null || rawRgb24.Length < expectedSize)
            {
                Log($"[ImageProcessor] 입력 크기 부족: {rawRgb24?.Length ?? 0} / {expectedSize}");
                return null;
            }

            try
            {
                using var rgb = new Mat(height, width, MatType.CV_8UC3);
                Marshal.Copy(rawRgb24, 0, rgb.Data, expectedSize);

                using var bgr = new Mat();
                Cv2.CvtColor(rgb, bgr, ColorConversionCodes.RGB2BGR);

                using var flipped = new Mat();
                Cv2.Flip(bgr, flipped, FlipMode.X);

                Mat current = flipped.Clone();
                try
                {
                    foreach (var effect in ActiveEffects)
                    {
                        var next = effect.Apply(current);
                        current.Dispose();
                        current = next;
                    }
                    return current.ToBitmap();
                }
                finally
                {
                    current.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log($"[ImageProcessor] 변환 오류: {ex.Message}");
                return null;
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
