using OpenCvSharp;

namespace SmartEye.Feature.Effects
{
    /// <summary>
    /// 이미지에 적용할 수 있는 효과 인터페이스
    /// </summary>
    public interface IImageEffect
    {
        /// <summary>
        /// 입력 이미지에 효과를 적용합니다.
        /// </summary>
        /// <param name="input">BGR 형식 입력 이미지</param>
        /// <returns>효과가 적용된 이미지 (호출자가 Dispose 책임)</returns>
        Mat Apply(Mat input);
    }
}
