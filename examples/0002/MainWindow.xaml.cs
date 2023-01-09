using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using SearchAThing.Desktop;

namespace SearchAThing.DesktopExamples;

public class MainWindow : Window
{

    GridSplitterManager gridSplitterManager;

    #region CreateControlSample
    private Func<Control> _CreateControlSample = null;

    public static readonly DirectProperty<MainWindow, Func<Control>> CreateControlSampleProperty =
        AvaloniaProperty.RegisterDirect<MainWindow, Func<Control>>("CreateControlSample", o => o.CreateControlSample, (o, v) => o.CreateControlSample = v);

    public Func<Control> CreateControlSample
    {
        get => _CreateControlSample;
        set => SetAndRaise(CreateControlSampleProperty, ref _CreateControlSample, value);
    }
    #endregion

    public MainWindow()
    {
        InitializeComponent();

        gridSplitterManager = this.FindControl<GridSplitterManager>("gridSplitterMasnager");

        CreateControlSample = () =>
        {
            var ctl = new SampleControl()
            {
                GridSplitterManager = gridSplitterManager
            };
            var rnd = new Random();
            var colMaxVal = 100;
            ctl.Background = new SolidColorBrush(Color.FromRgb(
                (byte)rnd.Next(colMaxVal),
                (byte)rnd.Next(colMaxVal),
                (byte)rnd.Next(colMaxVal)));
            return ctl;
        };
        this.Opened += (s, e) =>
        {
            var fc = gridSplitterManager.FocusedControl;
            gridSplitterManager.FocusedControl = null;
            gridSplitterManager.FocusedControl = fc;
        };
    }

    private void SplitHorizontalClick(object sender, RoutedEventArgs e)
    {
        gridSplitterManager.Split(GridSplitDirection.Horizontally);
    }

    private void SplitVerticalClick(object sender, RoutedEventArgs e)
    {
        gridSplitterManager.Split(GridSplitDirection.Vertically);
    }

    private void RemoveViewClick(object sender, RoutedEventArgs e)
    {
        gridSplitterManager.Remove();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}
