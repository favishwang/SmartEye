namespace CarDetectionTest
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            ToolbarPanel = new FlowLayoutPanel();
            BtnCarDetect = new Button();
            ImageDisplayArea = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)ImageDisplayArea).BeginInit();
            SuspendLayout();
            //
            // ToolbarPanel
            //
            ToolbarPanel.Controls.Add(BtnCarDetect);
            ToolbarPanel.Dock = DockStyle.Top;
            ToolbarPanel.Location = new Point(0, 0);
            ToolbarPanel.Name = "ToolbarPanel";
            ToolbarPanel.Padding = new Padding(8, 4, 8, 4);
            ToolbarPanel.Size = new Size(800, 44);
            ToolbarPanel.TabIndex = 0;
            //
            // BtnCarDetect
            //
            BtnCarDetect.Location = new Point(11, 7);
            BtnCarDetect.Name = "BtnCarDetect";
            BtnCarDetect.Size = new Size(100, 32);
            BtnCarDetect.TabIndex = 0;
            BtnCarDetect.Text = "차량 검출";
            BtnCarDetect.UseVisualStyleBackColor = true;
            BtnCarDetect.Click += BtnCarDetect_Click;
            //
            // ImageDisplayArea
            //
            ImageDisplayArea.BackColor = Color.Black;
            ImageDisplayArea.Dock = DockStyle.Fill;
            ImageDisplayArea.Location = new Point(0, 44);
            ImageDisplayArea.Name = "ImageDisplayArea";
            ImageDisplayArea.Size = new Size(800, 406);
            ImageDisplayArea.SizeMode = PictureBoxSizeMode.Zoom;
            ImageDisplayArea.TabIndex = 1;
            ImageDisplayArea.TabStop = false;
            ImageDisplayArea.Click += ImageDisplayArea_Click;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(ImageDisplayArea);
            Controls.Add(ToolbarPanel);
            MinimumSize = new Size(400, 300);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CarDetectionTest";
            ((System.ComponentModel.ISupportInitialize)ImageDisplayArea).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel ToolbarPanel;
        private Button BtnCarDetect;
        private PictureBox ImageDisplayArea;
    }
}
