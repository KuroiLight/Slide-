using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls;
using WpfScreenHelper;

namespace SlideSharp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Coordinator Coordinator;
        internal TaskbarIcon TBIcon;

        public MainWindow()
        {
            InitializeComponent();
            Hide();
            Configuration.SettingsInstance = Configuration.Load();
            UpdateInterfaceConfigs();
            TBIcon = new TaskbarIcon
            {
                Icon = Properties.Resources.SSharp
            };
            TBIcon.TrayMouseDoubleClick += TBIcon_TrayMouseDoubleClick;

            Coordinator = new Coordinator();
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
                return (int)Math.Clamp(control.Value, control.Minimum, control.Maximum);
            }

            Configuration.SettingsInstance.Middle_Button_DeadZone = getValueOfTextBlock(dragDeadzoneSlider);
            Configuration.SettingsInstance.Window_Movement_Rate = getValueOfTextBlock(stepSizeSlider);
            Configuration.SettingsInstance.Update_Interval = getValueOfTextBlock(responseSlider);
            Configuration.SettingsInstance.Window_Offscreen_Offset = getValueOfTextBlock(offScreenOffsetSlider);

            Configuration.Save();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Configuration.SettingsInstance = Configuration.Defaults();
            UpdateInterfaceConfigs();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void UpdateInterfaceConfigs()
        {
            dragDeadzoneSlider.Value = Configuration.SettingsInstance.Middle_Button_DeadZone;
            stepSizeSlider.Value = Configuration.SettingsInstance.Window_Movement_Rate;
            responseSlider.Value = Configuration.SettingsInstance.Update_Interval;
            offScreenOffsetSlider.Value = Configuration.SettingsInstance.Window_Offscreen_Offset;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            TBIcon.Visibility = Visibility.Hidden;
            TBIcon.Dispose();
        }
    }
}