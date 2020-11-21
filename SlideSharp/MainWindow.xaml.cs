using System;
using System.Windows;
using System.Windows.Controls;
using Screen_Drop_In;

namespace SlideSharp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Coordinator Coordinator;

        public MainWindow()
        {
            InitializeComponent();
            Configuration.Load();
            UpdateFromConfigs();
            tbTray.Icon = Properties.Resources.SSharp;
            Coordinator = new Coordinator();
        }

        private void TBIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            var pt = Win32Api.User32.GetCursorPos();
            Left = pt.X - (Width / 2);
            Screen? scr = Screen.FromPoint(new System.Drawing.Point((int)Left, (int)Top));
            if(scr is null) {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                Top = scr!.WorkingArea.Bottom - Height;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            static int getValueOfTextBlock(Slider control)
            {
                return (int)Math.Clamp(control.Value, control.Minimum, control.Maximum);
            }

            Config C = new();
            C.MMDRAG_DEADZONE = getValueOfTextBlock(dragDeadzoneSlider);
            C.WINDOW_ANIM_SPEED = Math.Clamp(stepSizeSlider.Value, stepSizeSlider.Minimum, stepSizeSlider.Maximum);
            C.HIDDEN_OFFSET = getValueOfTextBlock(offScreenOffsetSlider);
            Configuration.Config = C;

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
            dragDeadzoneSlider.Maximum = (int)((Screen.PrimaryScreen.Bounds.Width / 100) * 50);
            dragDeadzoneSlider.Value = Configuration.Config.MMDRAG_DEADZONE;
            stepSizeSlider.Value = Configuration.Config.WINDOW_ANIM_SPEED;
            offScreenOffsetSlider.Value = Configuration.Config.HIDDEN_OFFSET;
        }

        private void OffScreenOffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            offScreenOffset.Text = ((int)offScreenOffsetSlider.Value).ToString();
        }

        private void DragDeadzoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dragDeadzone.Text = ((int)dragDeadzoneSlider.Value).ToString();
        }

        private void StepSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            stepSize.Text = stepSizeSlider.Value.ToString("0.000");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Configuration.Save();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}