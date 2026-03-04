using OpenCvSharp;

namespace SmartEye.Feature.Effects
{
    /// <summary>
    /// 이진화(Threshold) 효과
    /// </summary>
    public class ThresholdEffect : IImageEffect
    {
        public int ThresholdValue { get; set; } = 127;
        public int MaxValue { get; set; } = 255;
        public ThresholdTypes Type { get; set; } = ThresholdTypes.Binary;

        public Mat Apply(Mat input)
        {
            using var gray = new Mat();
            Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY);

            var output = new Mat();
            Cv2.Threshold(gray, output, ThresholdValue, MaxValue, Type);
            Cv2.CvtColor(output, output, ColorConversionCodes.GRAY2BGR);
            return output;
        }
    }
}
