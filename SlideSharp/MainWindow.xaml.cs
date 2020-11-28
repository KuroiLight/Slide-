using Screen_Drop_In;
using System.Windows;

namespace SlideSharp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Coordinator Coordinator;
        private readonly Config config = Config.GetInstance;

        public MainWindow()
        {
            InitializeComponent();
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
            if (scr is null) {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                Top = scr!.WorkingArea.Bottom - Height;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            config.MouseDragDeadzone = (int)dragDeadzoneSlider.Value;
            config.WindowHiddenOffset = (int)offScreenOffsetSlider.Value;
            config.WindowMovementSpeed = stepSizeSlider.Value;

            Config.SaveToDisk();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Config.ReadFromDisk();
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
            dragDeadzoneSlider.Value = config.MouseDragDeadzone;
            stepSizeSlider.Value = config.WindowMovementSpeed;
            offScreenOffsetSlider.Value = config.WindowHiddenOffset;
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
            Config.SaveToDisk();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}