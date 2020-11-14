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
        private Coordinator Coordinator;
        internal TaskbarIcon TBIcon;

        public MainWindow()
        {
            InitializeComponent();
            Configuration.Load();
            UpdateFromConfigs();
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

            Configuration.Config.MMDRAG_DEADZONE = getValueOfTextBlock(dragDeadzoneSlider);
            Configuration.Config.WINDOW_ANIM_SPEED = Math.Clamp(stepSizeSlider.Value, stepSizeSlider.Minimum, stepSizeSlider.Maximum);
            Configuration.Config.HIDDEN_OFFSET = getValueOfTextBlock(offScreenOffsetSlider);

            Configuration.Save();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Configuration.LoadDefaults();
            UpdateFromConfigs();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateFromConfigs();
            Hide();
        }

        private void UpdateFromConfigs()
        {
            dragDeadzoneSlider.Maximum = (int)((WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width / 100) * 50);
            dragDeadzoneSlider.Value = Configuration.Config.MMDRAG_DEADZONE;
            stepSizeSlider.Value = Configuration.Config.WINDOW_ANIM_SPEED;
            offScreenOffsetSlider.Value = Configuration.Config.HIDDEN_OFFSET;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            TBIcon.Visibility = Visibility.Hidden;
            TBIcon.Dispose();
        }

        private void offScreenOffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            offScreenOffset.Text = ((int)offScreenOffsetSlider.Value).ToString();
        }

        private void dragDeadzoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dragDeadzone.Text = ((int)dragDeadzoneSlider.Value).ToString();
        }

        private void stepSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            stepSize.Text = stepSizeSlider.Value.ToString("0.000");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Configuration.Save();
            Coordinator = null;
            GC.Collect();
        }
    }
}