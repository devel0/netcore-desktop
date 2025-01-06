using Avalonia.Interactivity;
using Newtonsoft.Json;

namespace example.Views;

public partial class MainWindow : Window
{
    GridSplitterManager<SampleControl> gridSplitterManager;

    public MainWindow()
    {
        InitializeComponent();

        Title = AppDomain.CurrentDomain.FriendlyName + " ( GridSplitterManager ) ";

        Width = 1024;
        Height = 768;

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

        tv.LayoutUpdated += (a, b) =>
        {
            void ExpandChildren(TreeViewItem tvi)
            {
                tvi.IsExpanded = true;

                if (tvi.Items is null) return;

                foreach (var child in tvi.Items)
                {
                    var q = tvi.ContainerFromItem(child);
                    if (q is TreeViewItem tviChild)
                    {
                        ExpandChildren(tviChild);
                    }
                }
            }

            foreach (var item in tvItems)
            {
                var container = tv.ContainerFromItem(item);
                if (container is TreeViewItem tvi)
                    ExpandChildren(tvi);
            }

        };

        this.KeyDown += (sender, e) =>
        {
            if (e.Key == Avalonia.Input.Key.H) SplitHorizontal();
            else if (e.Key == Avalonia.Input.Key.V) SplitVertical();
            else if (e.Key == Avalonia.Input.Key.C) Remove();
        };
    }

    public ObservableCollection<StructureInfo> tvItems { get; } = new ObservableCollection<StructureInfo>();

    private void GridSplitterManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GridSplitterManager<SampleControl>.FocusedControl))
        {
            Debug.WriteLine($"FocusedControl changed");
        }
    }

    void SplitHorizontal()
    {
        gridSplitterManager.Split(GridSplitDirection.Horizontally);
        tvRefresh();
    }

    void SplitVertical()
    {
        gridSplitterManager.Split(GridSplitDirection.Vertically);
        tvRefresh();
    }

    void Remove()
    {
        gridSplitterManager.Remove();
        tvRefresh();
    }

    private void SplitHorizontalClick(object sender, RoutedEventArgs e) => SplitHorizontal();
    private void SplitVerticalClick(object sender, RoutedEventArgs e) => SplitVertical();
    private void RemoveViewClick(object sender, RoutedEventArgs e) => Remove();

    string layoutPathfilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "layout.json");

    private void SaveLayoutClick(object sender, RoutedEventArgs e)
    {
        var layout = gridSplitterManager.SaveStructure((ctl, uid) =>
        {
            Debug.WriteLine($"UID:{uid} hs:{ctl.HashNr}");
        });
        File.WriteAllText(layoutPathfilename, JsonConvert.SerializeObject(layout, gridSplitterManager.JsonSettings));
    }

    private void RestoreLayoutClick(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(layoutPathfilename)) return;

        var layout = JsonConvert.DeserializeObject<GridSplitterManagerLayoutItem>(File.ReadAllText(layoutPathfilename));
        if (layout is not null)
        {
            gridSplitterManager.LoadStructure(layout);
        }

        tvRefresh();
    }

    private void ClearLayoutClick(object sender, RoutedEventArgs e)
    {
        gridSplitterManager.Clear();
        tvRefresh();
    }

    void tvRefresh()
    {
        tvItems.Clear();        

        var grRoot = (Grid)gridSplitterManager;

        StructureInfo Scan(Control ctl, StructureInfo? parent = null)
        {
            StructureInfo nfo;

            if (ctl is Grid gr)
            {
                var str = "Grid";
                nfo = new StructureInfo(str);
                if (parent is not null)
                    parent.Children.Add(nfo);
                if (ctl.Parent is Grid pGr)
                {
                    if (pGr.RowDefinitions.Count > 0)
                        str += $" row({Grid.GetRow(ctl)})";
                    else
                        str += $" col({Grid.GetColumn(ctl)})";

                    nfo.Desc = str;
                }

                foreach (var child in gr.Children)
                    Scan(child, nfo);
            }
            else if (ctl is Border brd)
            {
                var str = "Border";
                nfo = new StructureInfo(str);
                if (parent is not null)
                    parent.Children.Add(nfo);

                if (ctl.Parent is Grid pGr)
                {
                    if (pGr.RowDefinitions.Count > 0)
                        str += $" row({Grid.GetRow(ctl)})";
                    else
                        str += $" col({Grid.GetColumn(ctl)})";

                    nfo.Desc = str;
                }
            }
            else if (ctl is GridSplitter grSplit)
            {
                var str = "Splitter";
                nfo = new StructureInfo(str);
                if (parent is not null)
                    parent.Children.Add(nfo);

                if (ctl.Parent is Grid pGr)
                {
                    if (pGr.RowDefinitions.Count > 0)
                        str += $" row({Grid.GetRow(ctl)})";
                    else
                        str += $" col({Grid.GetColumn(ctl)})";

                    nfo.Desc = str;
                }
            }
            else
            {
                nfo = new StructureInfo($"[{ctl.GetType()}]");
                if (parent is not null)
                    parent.Children.Add(nfo);
            }

            return nfo;
        }

        tvItems.Clear();
        tvItems.Add(Scan(grRoot));
    }

}

public class StructureInfo
{

    public string Desc { get; set; }
    public List<StructureInfo> Children { get; } = new List<StructureInfo>();

    public StructureInfo(string desc)
    {
        Desc = desc;
    }

    public override string ToString() => Desc;
}