using OpenCvSharp;

namespace SmartEye.Feature.Effects
{
    /// <summary>
    /// 밝기(Intensity) 조정 효과. 127=원본, 0=어둡게, 255=밝게.
    /// </summary>
    public class IntensityEffect : IImageEffect
    {
        /// <summary>0~255, 127=변화 없음, 0=검정, 255=2배 밝기</summary>
        public int Intensity { get; set; } = 127;

        public Mat Apply(Mat input)
        {
            if (Intensity == 127)
                return input.Clone();

            double factor = Intensity / 127.0;
            var output = new Mat();
            input.ConvertTo(output, -1, factor, 0);
            return output;
        }
    }
}
