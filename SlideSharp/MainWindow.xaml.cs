using System;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using WpfScreenHelper;

namespace SlideSharp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static Configuration config;
        private readonly Coordinator Coordinator = new Coordinator();
        internal TaskbarIcon TBIcon;

        public MainWindow()
        {
            InitializeComponent();
            Hide();
            config = Configuration.Load();
            UpdateInterFaceConfigs();
            TBIcon = new TaskbarIcon
            {
                Icon = Properties.Resources.SSharp
            };
            TBIcon.TrayMouseDoubleClick += TBIcon_TrayMouseDoubleClick;
        }

        private void TBIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            Left = MouseHelper.MousePosition.X - Width / 2;
            Top = Screen.FromPoint(new Point(Left, Top)).WorkingArea.Bottom - Height;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            static int getValueOfTextBlock(Slider control)
            {
                return (int) Math.Clamp(control.Value, control.Minimum, control.Maximum);
            }

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
            Hide();
        }

        private void UpdateInterFaceConfigs()
        {
            dragDeadzoneSlider.Value = config.Middle_Button_DeadZone;
            stepSizeSlider.Value = config.Window_Movement_Rate;
            responseSlider.Value = config.Update_Interval;
            offScreenOffsetSlider.Value = config.Window_Offscreen_Offset;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            TBIcon.Visibility = Visibility.Hidden;
        }
    }
}