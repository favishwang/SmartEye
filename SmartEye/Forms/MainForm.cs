using SmartEye.Feature;
using SmartEye.Feature.Effects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SmartEye
{
    public partial class MainForm : Form
    {
        private UnityCameraReader _cameraReader = null!;
        private ImageProcessor _imageProcessor = null!;
        private byte[] _frameBuffer = null!;
        private Timer _timer = null!;
        private int _retryCount;

        private readonly IntensityEffect _intensityEffect = new();
        private readonly GrayscaleEffect _grayscaleEffect = new();
        private readonly ThresholdEffect _thresholdEffect = new();
        private readonly ROIManager _roiManager = new();
        private readonly ROIDetectorEffect _roiEffect = new();
        private LogForm? _logForm;

        private bool _grayscaleEnabled;
        private bool _roiEnabled;

        private long _frameCount;
        private DateTime _fpsStartTime = DateTime.Now;
        private int _lastFps;

        private const int RetryIntervalMs = 2000;
        private const int FrameIntervalMs = 15;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
        }

        private void InitializeServices()
        {
            _cameraReader = new UnityCameraReader { OnLog = AddLog };
            _imageProcessor = new ImageProcessor { OnLog = AddLog };
            _roiEffect.ROIManager = _roiManager;
            _logForm = new LogForm();
            _frameBuffer = new byte[UnityCameraReader.FrameSize];

            IntensitySlider.Value = 127;
            ThresholdSlider.Value = 127;

            UpdateEffectPipeline();

            if (_cameraReader.TryConnect())
                StartFrameTimer();
            else
                StartRetryTimer();
        }

        private void UpdateEffectPipeline()
        {
            _intensityEffect.Intensity = IntensitySlider.Value;
            _thresholdEffect.ThresholdValue = ThresholdSlider.Value;

            var effects = new List<IImageEffect>();
            if (ChkIntensity.Checked) effects.Add(_intensityEffect);
            if (_grayscaleEnabled) effects.Add(_grayscaleEffect);
            if (ChkThreshold.Checked) effects.Add(_thresholdEffect);
            if (_roiEnabled) effects.Add(_roiEffect);

            _imageProcessor.ActiveEffects = effects;
        }

        private void BtnGrayscale_Click(object? sender, EventArgs e)
        {
            _grayscaleEnabled = !_grayscaleEnabled;
            UpdateButtonState(BtnGrayscale, _grayscaleEnabled);
            UpdateEffectPipeline();
        }

        private void BtnROI_Click(object? sender, EventArgs e)
        {
            _roiEnabled = !_roiEnabled;
            UpdateButtonState(BtnROI, _roiEnabled);
            UpdateEffectPipeline();
            UpdateRoiDisplay();
        }

        private static void UpdateButtonState(Button btn, bool enabled)
        {
            btn.BackColor = enabled ? Color.LightGreen : SystemColors.Control;
        }

        private void StartFrameTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = new Timer { Interval = FrameIntervalMs };
            _timer.Tick += OnFrameTick;
            _timer.Start();
        }

        private void StartRetryTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = new Timer { Interval = RetryIntervalMs };
            _timer.Tick += OnRetryTick;
            _timer.Start();
        }

        private void OnRetryTick(object? sender, EventArgs e)
        {
            _retryCount++;
            if (_cameraReader.TryConnect())
            {
                AddLog($"[MainForm] UnityCamera 연결됨 (재시도 {_retryCount}회)");
                StartFrameTimer();
            }
        }

        private void OnFrameTick(object? sender, EventArgs e)
        {
            if (!_cameraReader.ReadFrame(_frameBuffer))
                return;

            UpdateEffectPipeline();

            var bitmap = _imageProcessor.ProcessToBitmap(
                _frameBuffer,
                UnityCameraReader.Width,
                UnityCameraReader.Height);

            if (bitmap == null)
                return;

            _frameCount++;
            var elapsed = (DateTime.Now - _fpsStartTime).TotalSeconds;
            _lastFps = elapsed > 0 ? (int)(_frameCount / elapsed) : 0;

            UpdateCameraView(bitmap);
            if (_roiEnabled)
                UpdateRoiDisplay();
        }

        private void UpdateRoiDisplay()
        {
            if (TxtRoiInfo == null || TxtRoiInfo.IsDisposed)
                return;

            var text = _roiEnabled ? _roiManager.ToDisplayText() : "ROI: 비활성";
            void SetText()
            {
                TxtRoiInfo.Text = text;
            }
            if (TxtRoiInfo.InvokeRequired)
                TxtRoiInfo.Invoke(SetText);
            else
                SetText();
        }

        private void UpdateCameraView(Bitmap bitmap)
        {
            if (CameraViewArea.IsDisposed)
                return;

            using var displayBitmap = (Bitmap)bitmap.Clone();
            using var g = Graphics.FromImage(displayBitmap);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using var brush = new SolidBrush(Color.Red);
            using var font = new Font("Consolas", 14, FontStyle.Bold);
            var text = $"Frame: {_frameCount}  FPS: {_lastFps}";
            var size = g.MeasureString(text, font);
            var x = displayBitmap.Width - size.Width - 10;
            var y = 10f;
            g.DrawString(text, font, brush, x, y);

            var old = CameraViewArea.Image;
            CameraViewArea.Image = (Bitmap)displayBitmap.Clone();
            old?.Dispose();
        }

        private void AddLog(string message)
        {
            if (_logForm == null) return;
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _logForm.AppendLog(line);
        }

        private void BtnLog_Click(object? sender, EventArgs e)
        {
            if (_logForm == null) return;
            _logForm.Owner = this;
            _logForm.Show();
            _logForm.BringToFront();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            _cameraReader?.Dispose();
            CameraViewArea.Image?.Dispose();
            _logForm?.Close();
            base.OnFormClosed(e);
        }
    }
}
