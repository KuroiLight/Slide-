using System;
using System.Windows.Forms;

namespace WindowShift
{

    public partial class SettingsPane : Form
    {
        public SettingsPane()
        {
            InitializeComponent();
        }

        private static readonly WindowManager windowManager = new WindowManager();


        private void Form1_Load(object sender, EventArgs e)
        {
            Hide();
            windowManager.StartHook();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            windowManager.EndHook();
        }
    }
}
