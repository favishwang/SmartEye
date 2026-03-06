using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;

namespace SmartEye.Feature.Effects
{
    /// <summary>
    /// ROI(관심 영역) 추정 및 표시 효과. ROIManager와 연동하여 인덱스 순서로 표시합니다.
    /// </summary>
    public class ROIDetectorEffect : IImageEffect
    {
        public ROIManager ROIManager { get; set; } = null!;

        /// <summary>true면 검출 생략, ROIManager에 저장된 영역만 표시</summary>
        public bool SkipDetection { get; set; }

        public Scalar RectColor { get; set; } = new Scalar(0, 255, 0);
        public int RectThickness { get; set; } = 2;
        public double MinContourArea { get; set; } = 1000;

        public Mat Apply(Mat input)
        {
            var output = input.Clone();
            var rects = new List<Rect>();

            if (SkipDetection)
            {
                if (ROIManager != null)
                {
                    foreach (var r in ROIManager.Regions)
                        rects.Add(r.Rect);
                }
            }
            else
            {
                using var gray = new Mat();
                Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY);

                using var blurred = new Mat();
                Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(5, 5), 0);

                using var binary = new Mat();
                Cv2.Threshold(blurred, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                Cv2.FindContours(binary, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    if (area < MinContourArea) continue;
                    rects.Add(Cv2.BoundingRect(contour));
                }

                var sortedRects = rects.OrderByDescending(r => r.Width * r.Height).ToList();
                ROIManager?.Update(sortedRects);
                rects = sortedRects;
            }

            for (int i = 0; i < rects.Count; i++)
            {
                var rect = rects[i];
                Cv2.Rectangle(output, rect, RectColor, RectThickness);

                var label = $"{i}";
                var textOrigin = new OpenCvSharp.Point(rect.X, rect.Y - 5);
                if (textOrigin.Y < 15)
                    textOrigin.Y = rect.Y + 20;

                Cv2.PutText(output, label, textOrigin, HersheyFonts.HersheySimplex, 0.8, RectColor, 2, LineTypes.AntiAlias);
            }

            return output;
        }
    }
}
