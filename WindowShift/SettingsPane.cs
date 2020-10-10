using System;
using System.Windows.Forms;

namespace WindowShift
{

    public partial class SettingsPane : Form
    {
        private readonly Main M = new Main();

        public SettingsPane()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            M.Dispose();
        }
    }
}
