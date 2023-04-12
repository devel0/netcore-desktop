using Avalonia.Interactivity;

namespace example.Views;

public partial class MainWindow : Window
{
    GridSplitterManager<SampleControl> gridSplitterManager;

    public MainWindow()
    {
        InitializeComponent();

        Title = AppDomain.CurrentDomain.FriendlyName + " ( GridSplitterManager ) ";

        gridSplitterManager = new GridSplitterManager<SampleControl>()
        {
            FocusedControlBorderThickness = 0,
            Margin = new Thickness(0, 10, 0, 0),
            CreateControl = () =>
            {
                var ctl = new SampleControl();
                var rnd = new Random();
                var colMaxVal = 100;
                ctl.Background = new SolidColorBrush(Color.FromRgb(
                    (byte)rnd.Next(colMaxVal),
                    (byte)rnd.Next(colMaxVal),
                    (byte)rnd.Next(colMaxVal)));
                return ctl;
            }
        };

        grSplitContainer.Children.Add(gridSplitterManager);

        gridSplitterManager.PropertyChanged += GridSplitterManager_PropertyChanged;
    }

    private void GridSplitterManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GridSplitterManager<SampleControl>.FocusedControl))
        {
            Debug.WriteLine($"FocusedControl changed");
        }
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

}