using System.Windows;

namespace SlideSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Coordinator Coordinator = new Coordinator();

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}