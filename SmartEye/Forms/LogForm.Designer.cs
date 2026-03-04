namespace SmartEye
{
    partial class LogForm
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
            LogListBox = new ListBox();
            SuspendLayout();
            //
            // LogListBox
            //
            LogListBox.BackColor = Color.FromArgb(30, 30, 30);
            LogListBox.Dock = DockStyle.Fill;
            LogListBox.Font = new Font("Consolas", 9F);
            LogListBox.ForeColor = Color.LightGray;
            LogListBox.FormattingEnabled = true;
            LogListBox.HorizontalScrollbar = true;
            LogListBox.ItemHeight = 18;
            LogListBox.Location = new Point(0, 0);
            LogListBox.Name = "LogListBox";
            LogListBox.Size = new Size(384, 361);
            LogListBox.TabIndex = 0;
            //
            // LogForm
            //
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 361);
            Controls.Add(LogListBox);
            MinimumSize = new Size(300, 200);
            Name = "LogForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "시스템 로그";
            ResumeLayout(false);
        }

        #endregion

        private ListBox LogListBox = null!;
    }
}
