using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CarDetectionTest
{
    public partial class Form1 : Form
    {
        private readonly CarDetector _carDetector = new();
        private Bitmap? _currentImage;

        public Form1()
        {
            InitializeComponent();
            _carDetector.OnLog = msg => System.Diagnostics.Debug.WriteLine(msg);
        }

        private void ImageDisplayArea_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.bmp|모든 파일|*.*",
                Title = "이미지 선택"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var old = _currentImage;
                _currentImage = new Bitmap(dlg.FileName);
                old?.Dispose();

                ImageDisplayArea.Image?.Dispose();
                ImageDisplayArea.Image = (Bitmap)_currentImage.Clone();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 로드 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCarDetect_Click(object? sender, EventArgs e)
        {
            if (_currentImage == null)
            {
                MessageBox.Show("먼저 이미지를 로드하세요. (이미지 영역 클릭)", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var mat = BitmapConverter.ToMat(_currentImage);
                using var bgr = new Mat();
                Cv2.CvtColor(mat, bgr, ColorConversionCodes.RGB2BGR);

                var rects = _carDetector.Detect(bgr);

                using var resultBitmap = (Bitmap)_currentImage!.Clone();
                using var g = Graphics.FromImage(resultBitmap);
                using var pen = new Pen(Color.Lime, 2);

                foreach (var rect in rects)
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                ImageDisplayArea.Image?.Dispose();
                ImageDisplayArea.Image = (Bitmap)resultBitmap.Clone();

                MessageBox.Show($"{rects.Count}대 검출됨", "차량 검출", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"검출 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _currentImage?.Dispose();
            ImageDisplayArea.Image?.Dispose();
            _carDetector.Dispose();
            base.OnFormClosed(e);
        }
    }
}
