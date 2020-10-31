using System;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;

namespace SlideSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Coordinator Coordinator = new Coordinator();
        internal static Configuration config;
        internal TaskbarIcon TBIcon;

        public MainWindow()
        {
            InitializeComponent();
            this.Hide();
            config = Configuration.Load();
            UpdateInterFaceConfigs();
            TBIcon = new TaskbarIcon();
            TBIcon.Icon = SlideSharp.Properties.Resources.SSharp;
            TBIcon.TrayMouseDoubleClick += TBIcon_TrayMouseDoubleClick;
        }

        private void TBIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int getValueOfTextBlock(Slider control) => (int)Math.Clamp(control.Value, control.Minimum, control.Maximum);

            config.Middle_Button_DeadZone = getValueOfTextBlock(dragDeadzoneSlider);
            config.Window_Movement_Rate = getValueOfTextBlock(stepSizeSlider);
            config.Update_Interval = getValueOfTextBlock(responseSlider);
            config.Window_Offscreen_Offset = getValueOfTextBlock(offScreenOffsetSlider);

            Configuration.Save();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            config = Configuration.Defaults();
            UpdateInterFaceConfigs();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void UpdateInterFaceConfigs()
        {
            dragDeadzoneSlider.Value = config.Middle_Button_DeadZone;
            stepSizeSlider.Value = config.Window_Movement_Rate;
            responseSlider.Value = config.Update_Interval;
            offScreenOffsetSlider.Value = config.Window_Offscreen_Offset;
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            
        }
    }
}