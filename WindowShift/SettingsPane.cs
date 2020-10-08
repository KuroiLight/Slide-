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

        private Main windowManager;// = new WindowManager();


        private void Form1_Load(object sender, EventArgs e)
        {
            windowManager = new Main();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}
