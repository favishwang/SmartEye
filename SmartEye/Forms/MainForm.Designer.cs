namespace SmartEye
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            MainTableLayout = new TableLayoutPanel();
            CameraPanel = new Panel();
            ToolbarPanel = new FlowLayoutPanel();
            LblIntensity = new Label();
            IntensitySlider = new TrackBar();
            ChkIntensity = new CheckBox();
            LblThreshold = new Label();
            ThresholdSlider = new TrackBar();
            ChkThreshold = new CheckBox();
            BtnGrayscale = new Button();
            BtnROI = new Button();
            BtnLog = new Button();
            CameraViewArea = new PictureBox();
            RoiPanel = new Panel();
            TxtRoiInfo = new TextBox();
            MainTableLayout.SuspendLayout();
            CameraPanel.SuspendLayout();
            ToolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)IntensitySlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ThresholdSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CameraViewArea).BeginInit();
            RoiPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainTableLayout
            // 
            MainTableLayout.ColumnCount = 2;
            MainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            MainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            MainTableLayout.Controls.Add(CameraPanel, 0, 0);
            MainTableLayout.Controls.Add(RoiPanel, 1, 0);
            MainTableLayout.Dock = DockStyle.Fill;
            MainTableLayout.Location = new Point(0, 0);
            MainTableLayout.Margin = new Padding(0);
            MainTableLayout.Name = "MainTableLayout";
            MainTableLayout.Padding = new Padding(8);
            MainTableLayout.RowCount = 1;
            MainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            MainTableLayout.Size = new Size(1280, 600);
            MainTableLayout.TabIndex = 0;
            // 
            // CameraPanel
            // 
            CameraPanel.Controls.Add(ToolbarPanel);
            CameraPanel.Controls.Add(CameraViewArea);
            CameraPanel.Dock = DockStyle.Fill;
            CameraPanel.Location = new Point(11, 11);
            CameraPanel.Name = "CameraPanel";
            CameraPanel.Size = new Size(978, 578);
            CameraPanel.TabIndex = 0;
            // 
            // ToolbarPanel
            // 
            ToolbarPanel.Controls.Add(LblIntensity);
            ToolbarPanel.Controls.Add(IntensitySlider);
            ToolbarPanel.Controls.Add(ChkIntensity);
            ToolbarPanel.Controls.Add(LblThreshold);
            ToolbarPanel.Controls.Add(ThresholdSlider);
            ToolbarPanel.Controls.Add(ChkThreshold);
            ToolbarPanel.Controls.Add(BtnGrayscale);
            ToolbarPanel.Controls.Add(BtnROI);
            ToolbarPanel.Controls.Add(BtnLog);
            ToolbarPanel.Dock = DockStyle.Top;
            ToolbarPanel.Location = new Point(0, 0);
            ToolbarPanel.Name = "ToolbarPanel";
            ToolbarPanel.Padding = new Padding(0, 0, 0, 4);
            ToolbarPanel.Size = new Size(978, 40);
            ToolbarPanel.TabIndex = 0;
            // 
            // LblIntensity
            // 
            LblIntensity.AutoSize = true;
            LblIntensity.Location = new Point(3, 0);
            LblIntensity.Name = "LblIntensity";
            LblIntensity.Size = new Size(66, 20);
            LblIntensity.TabIndex = 4;
            LblIntensity.Text = "Intensity";
            LblIntensity.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // IntensitySlider
            // 
            IntensitySlider.Location = new Point(75, 3);
            IntensitySlider.Maximum = 255;
            IntensitySlider.Minimum = 0;
            IntensitySlider.Name = "IntensitySlider";
            IntensitySlider.Size = new Size(130, 56);
            IntensitySlider.TabIndex = 3;
            // 
            // ChkIntensity
            // 
            ChkIntensity.AutoSize = true;
            ChkIntensity.Location = new Point(211, 10);
            ChkIntensity.Name = "ChkIntensity";
            ChkIntensity.Size = new Size(55, 24);
            ChkIntensity.TabIndex = 7;
            ChkIntensity.Text = "적용";
            ChkIntensity.UseVisualStyleBackColor = true;
            // 
            // LblThreshold
            // 
            LblThreshold.AutoSize = true;
            LblThreshold.Location = new Point(272, 0);
            LblThreshold.Name = "LblThreshold";
            LblThreshold.Size = new Size(76, 20);
            LblThreshold.TabIndex = 6;
            LblThreshold.Text = "Threshold";
            LblThreshold.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ThresholdSlider
            // 
            ThresholdSlider.Location = new Point(354, 3);
            ThresholdSlider.Maximum = 255;
            ThresholdSlider.Minimum = 0;
            ThresholdSlider.Name = "ThresholdSlider";
            ThresholdSlider.Size = new Size(130, 56);
            ThresholdSlider.TabIndex = 5;
            ThresholdSlider.Value = 127;
            // 
            // ChkThreshold
            // 
            ChkThreshold.AutoSize = true;
            ChkThreshold.Location = new Point(490, 10);
            ChkThreshold.Name = "ChkThreshold";
            ChkThreshold.Size = new Size(55, 24);
            ChkThreshold.TabIndex = 8;
            ChkThreshold.Text = "적용";
            ChkThreshold.UseVisualStyleBackColor = true;
            // 
            // BtnGrayscale
            // 
            BtnGrayscale.Location = new Point(551, 3);
            BtnGrayscale.Name = "BtnGrayscale";
            BtnGrayscale.Size = new Size(100, 32);
            BtnGrayscale.TabIndex = 0;
            BtnGrayscale.Text = "Grayscale";
            BtnGrayscale.UseVisualStyleBackColor = true;
            BtnGrayscale.Click += BtnGrayscale_Click;
            // 
            // BtnROI
            // 
            BtnROI.Location = new Point(657, 3);
            BtnROI.Name = "BtnROI";
            BtnROI.Size = new Size(100, 32);
            BtnROI.TabIndex = 2;
            BtnROI.Text = "ROI";
            BtnROI.UseVisualStyleBackColor = true;
            BtnROI.Click += BtnROI_Click;
            // 
            // BtnLog
            // 
            BtnLog.Location = new Point(763, 3);
            BtnLog.Name = "BtnLog";
            BtnLog.Size = new Size(80, 32);
            BtnLog.TabIndex = 3;
            BtnLog.Text = "로그";
            BtnLog.UseVisualStyleBackColor = true;
            BtnLog.Click += BtnLog_Click;
            // 
            // CameraViewArea
            // 
            CameraViewArea.BackColor = Color.Black;
            CameraViewArea.Dock = DockStyle.Fill;
            CameraViewArea.Location = new Point(0, 0);
            CameraViewArea.Margin = new Padding(0);
            CameraViewArea.Name = "CameraViewArea";
            CameraViewArea.Size = new Size(978, 578);
            CameraViewArea.SizeMode = PictureBoxSizeMode.Zoom;
            CameraViewArea.TabIndex = 1;
            CameraViewArea.TabStop = false;
            // 
            // RoiPanel
            // 
            RoiPanel.Controls.Add(TxtRoiInfo);
            RoiPanel.Dock = DockStyle.Fill;
            RoiPanel.Location = new Point(995, 11);
            RoiPanel.Name = "RoiPanel";
            RoiPanel.Size = new Size(274, 578);
            RoiPanel.TabIndex = 1;
            // 
            // TxtRoiInfo
            // 
            TxtRoiInfo.BackColor = Color.FromArgb(40, 40, 40);
            TxtRoiInfo.BorderStyle = BorderStyle.FixedSingle;
            TxtRoiInfo.Dock = DockStyle.Fill;
            TxtRoiInfo.Font = new Font("Consolas", 9F);
            TxtRoiInfo.ForeColor = Color.LightGray;
            TxtRoiInfo.Location = new Point(0, 0);
            TxtRoiInfo.Multiline = true;
            TxtRoiInfo.Name = "TxtRoiInfo";
            TxtRoiInfo.ReadOnly = true;
            TxtRoiInfo.ScrollBars = ScrollBars.Both;
            TxtRoiInfo.Size = new Size(274, 578);
            TxtRoiInfo.TabIndex = 0;
            TxtRoiInfo.Text = "ROI: 없음";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 600);
            Controls.Add(MainTableLayout);
            MinimumSize = new Size(640, 400);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SmartEye";
            MainTableLayout.ResumeLayout(false);
            CameraPanel.ResumeLayout(false);
            ToolbarPanel.ResumeLayout(false);
            ToolbarPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)IntensitySlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)ThresholdSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)CameraViewArea).EndInit();
            RoiPanel.ResumeLayout(false);
            RoiPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel MainTableLayout;
        private Panel CameraPanel;
        private FlowLayoutPanel ToolbarPanel;
        private Button BtnGrayscale;
        private Button BtnROI;
        private PictureBox CameraViewArea;
        private Panel RoiPanel;
        private TextBox TxtRoiInfo;
        private Button BtnLog;
        private Label LblIntensity;
        private TrackBar IntensitySlider;
        private CheckBox ChkIntensity;
        private Label LblThreshold;
        private TrackBar ThresholdSlider;
        private CheckBox ChkThreshold;
    }
}
