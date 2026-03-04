using OpenCvSharp;

namespace SmartEye.Feature.Effects
{
    /// <summary>
    /// 그레이스케일 효과
    /// </summary>
    public class GrayscaleEffect : IImageEffect
    {
        public Mat Apply(Mat input)
        {
            var output = new Mat();
            Cv2.CvtColor(input, output, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(output, output, ColorConversionCodes.GRAY2BGR);
            return output;
        }
    }
}
