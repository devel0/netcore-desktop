using Avalonia;
using Avalonia.Markup.Xaml;
using Window = Avalonia.Controls.Window;

namespace SearchAThing.DesktopExamples
{
    public class MainWindow : Window
    {

        #region SampleNumber
        private double _SampleNumber = 0d;

        public static readonly DirectProperty<MainWindow, double> SampleNumberProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, double>("SampleNumber", o => o.SampleNumber, (o, v) => o.SampleNumber = v);

        public double SampleNumber
        {
            get => _SampleNumber;
            set => SetAndRaise(SampleNumberProperty, ref _SampleNumber, value);
        }
        #endregion    

        public MainWindow()
        {
            InitializeComponent();            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}