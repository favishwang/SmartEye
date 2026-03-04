using System.Windows.Forms;

namespace SmartEye
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
        }

        public void AppendLog(string message)
        {
            if (LogListBox.IsDisposed)
                return;

            void Add()
            {
                LogListBox.Items.Add(message);
                LogListBox.TopIndex = LogListBox.Items.Count - 1;
            }

            if (LogListBox.InvokeRequired)
                LogListBox.Invoke(Add);
            else
                Add();
        }
    }
}
