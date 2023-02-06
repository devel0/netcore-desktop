using Avalonia.Interactivity;

namespace SearchAThing.DesktopExamples;

public partial class SampleControl : UserControl
{

    static int instanceCount = 0;

    int _InstanceNr = ++instanceCount;
    public int InstanceNr => _InstanceNr;

    public string HashNr => GetHashCode().ToString();

    #region GridSplitterManagerFocused
    private bool _GridSplitterManagerFocused = false;

    public static readonly DirectProperty<SampleControl, bool> GridSplitterManagerFocusedProperty =
        AvaloniaProperty.RegisterDirect<SampleControl, bool>("GridSplitterManagerFocused", o => o.GridSplitterManagerFocused, (o, v) => o.GridSplitterManagerFocused = v);

    public bool GridSplitterManagerFocused
    {
        get => _GridSplitterManagerFocused;
        set => SetAndRaise(GridSplitterManagerFocusedProperty, ref _GridSplitterManagerFocused, value);
    }
    #endregion

    #region GridSplitterManager
    private GridSplitterManager<SampleControl>? _GridSplitterManager = null;

    public static readonly DirectProperty<SampleControl, GridSplitterManager<SampleControl>?> GridSplitterManagerProperty =
        AvaloniaProperty.RegisterDirect<SampleControl, GridSplitterManager<SampleControl>?>("GridSplitterManager", o => o.GridSplitterManager, (o, v) => o.GridSplitterManager = v);

    public GridSplitterManager<SampleControl>? GridSplitterManager
    {
        get => _GridSplitterManager;
        set
        {
            SetAndRaise(GridSplitterManagerProperty, ref _GridSplitterManager, value);
            if (value is not null)
            {
                if (value.FocusedControl == this) GridSplitterManagerFocused = true;
                value.GetObservable(GridSplitterManager<SampleControl>.FocusedControlProperty).Subscribe((x) =>
                {
                    GridSplitterManagerFocused = x == this;
                });
            }
        }
    }
    #endregion

    public SampleControl()
    {
        InitializeComponent();
    }

    public override void Render(Avalonia.Media.DrawingContext context)
    {
        base.Render(context);
    }

}
