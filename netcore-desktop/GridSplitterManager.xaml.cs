using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace SearchAThing.Desktop;

/// <summary>
/// splitting direction
/// </summary>
public enum GridSplitDirection
{
    /// <summary>
    /// horizontally splitting works on Columns
    /// </summary>
    Horizontally,
    /// <summary>
    /// vertically splitting works on Rows
    /// </summary>
    Vertically
};

public class GridSplitterManager : UserControl
{
    static SmartConverter smartConverter = new SmartConverter();
    static FocusedControlConverter focusedControlConverter = new FocusedControlConverter();

    #region FocusedControlConverter
    /// <summary>
    /// first binding must be GridSplitterManager;
    /// second binding is the object A to test;
    /// parameter is the object B to test for equality;
    /// if object A and B equals then return FocusedControlBorderBrush otherwise Transparent
    /// </summary>
    public class FocusedControlConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var trBrush = new SolidColorBrush(Colors.Transparent);
            if (values is null) return trBrush;
            var gridSplitterManager = values[0] as GridSplitterManager;
            var objA = values[1];
            var objB = parameter;
            if (objA == objB)
                return gridSplitterManager?.FocusedControlBorderBrush;

            return trBrush;
        }
    }
    #endregion

    Grid grRoot;

    /// <summary>
    /// dictionary that keep track of visited control age, used to retrieve last visited when remove
    /// </summary>    
    Dictionary<Control, int> visitedControlDict = new Dictionary<Control, int>();
    int visitedControlAge = 0;

    #region FocusedControl
    private Control? _FocusedControl = null;

    public static readonly DirectProperty<GridSplitterManager, Control?> FocusedControlProperty =
        AvaloniaProperty.RegisterDirect<GridSplitterManager, Control?>("FocusedControl",
        o => o.FocusedControl, (o, v) => o.FocusedControl = v);

    /// <summary>
    /// current focused control
    /// </summary>
    /// <value></value>
    public Control? FocusedControl
    {
        get => _FocusedControl;
        set
        {
            SetAndRaise(FocusedControlProperty, ref _FocusedControl, value);
            if (value is not null)
            {
                value.Focus();
                if (visitedControlDict.ContainsKey(value))
                    visitedControlDict[value] = ++visitedControlAge;
                else
                    visitedControlDict.Add(value, ++visitedControlAge);
            }
        }
    }
    #endregion

    #region FocusedControlBorderThickness
    private double _FocusedControlBorderThickness = 1;

    public static readonly DirectProperty<GridSplitterManager, double> FocusedControlBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<GridSplitterManager, double>("FocusedControlBorderThickness",
        o => o.FocusedControlBorderThickness, (o, v) => o.FocusedControlBorderThickness = v);

    public double FocusedControlBorderThickness
    {
        get => _FocusedControlBorderThickness;
        set => SetAndRaise(FocusedControlBorderThicknessProperty, ref _FocusedControlBorderThickness, value);
    }
    #endregion

    #region FocusedControlBorderBrush
    private IBrush _FocusedControlBorderBrush = new SolidColorBrush(Colors.Yellow);

    public static readonly DirectProperty<GridSplitterManager, IBrush> FocusedControlBorderBrushProperty =
        AvaloniaProperty.RegisterDirect<GridSplitterManager, IBrush>("FocusedControlBorderBrush",
        o => o.FocusedControlBorderBrush, (o, v) => o.FocusedControlBorderBrush = v);

    public IBrush FocusedControlBorderBrush
    {
        get => _FocusedControlBorderBrush;
        set => SetAndRaise(FocusedControlBorderBrushProperty, ref _FocusedControlBorderBrush, value);
    }
    #endregion

    #region CreateControl
    private Func<Control>? _CreateControl = null;

    public static readonly DirectProperty<GridSplitterManager, Func<Control>?> CreateControlProperty =
        AvaloniaProperty.RegisterDirect<GridSplitterManager, Func<Control>?>("CreateControl",
        o => o.CreateControl, (o, v) => o.CreateControl = v);

    public Func<Control>? CreateControl
    {
        get => _CreateControl;
        set
        {
            if (_CreateControl is not null) throw new Exception($"cannot initialize twice");
            SetAndRaise(CreateControlProperty, ref _CreateControl, value);
            if (value is not null) CreateInitialControl();
        }
    }
    #endregion

    #region SplitterThickness
    private double _SplitterThickness = 10;

    public static readonly DirectProperty<GridSplitterManager, double> SplitterThicknessProperty =
        AvaloniaProperty.RegisterDirect<GridSplitterManager, double>("SplitterThickness",
        o => o.SplitterThickness, (o, v) => o.SplitterThickness = v);

    public double SplitterThickness
    {
        get => _SplitterThickness;
        set => SetAndRaise(SplitterThicknessProperty, ref _SplitterThickness, value);
    }
    #endregion

    #region SplitterBrush
    private IBrush _SplitterBrush = new SolidColorBrush(Colors.DarkGray);

    public static readonly DirectProperty<GridSplitterManager, IBrush> SplitterBrushProperty =
        AvaloniaProperty.RegisterDirect<GridSplitterManager, IBrush>("SplitterBrush",
        o => o.SplitterBrush, (o, v) => o.SplitterBrush = v);

    public IBrush SplitterBrush
    {
        get => _SplitterBrush;
        set => SetAndRaise(SplitterBrushProperty, ref _SplitterBrush, value);
    }
    #endregion

    #region DistributeSplitSize
    private bool _DistributeSplitSize = true;

    public static readonly DirectProperty<GridSplitterManager, bool> DistributeSplitSizeProperty =
        AvaloniaProperty.RegisterDirect<GridSplitterManager, bool>("DistributeSplitSize",
        o => o.DistributeSplitSize, (o, v) => o.DistributeSplitSize = v);

    /// <summary>
    /// if false split will half its size; if true (default) it creates a def star so balanced between N axial parallel controls
    /// </summary>        
    public bool DistributeSplitSize
    {
        get => _DistributeSplitSize;
        set => SetAndRaise(DistributeSplitSizeProperty, ref _DistributeSplitSize, value);
    }
    #endregion

    /// <summary>
    /// set to a valid text writer (eg.Console.Out) to debug structure processing;
    /// if null no debug output
    /// </summary>
    public TextWriter? DebugWriter { get; set; } = null;

    public GridSplitterManager()
    {
        InitializeComponent();

        grRoot = this.FindControl<Grid>("grRoot");
    }

    /// <summary>
    /// create new grid splitter for given direction and set its pos
    /// </summary>
    /// <param name="dir">container grid direction</param>
    /// <param name="pos">pos to set the gridsplitter ( col or row depending on dir )</param>
    /// <returns>new gridsplitter</returns>
    GridSplitter newGridSplitter(GridSplitDirection dir, int pos)
    {
        var res = new GridSplitter();

        switch (dir)
        {
            case GridSplitDirection.Horizontally:
                {
                    res.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                    res.Width = SplitterThickness;
                }
                break;
            case GridSplitDirection.Vertically:
                {
                    res.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
                    res.Height = SplitterThickness;
                }
                break;
        }
        SetPos(dir, res, pos);

        return res;
    }

    /// <summary>
    /// retrieve counting of grid children foreach type
    /// </summary>
    /// <param name="gr">grid to count children</param>
    /// <returns>(tot, grCnt, brdCnt, splitterCnt) tuple</returns>
    (int tot, int grCnt, int brdCnt, int splitterCnt) GridGetChildrenCount(Grid? gr)
    {
        int grCnt = 0;
        int brdCnt = 0;
        int splitterCnt = 0;

        if (gr is not null)
        {
            foreach (var x in gr.Children.Cast<Control>())
            {
                if (x is Grid) ++grCnt;
                else if (x is Border) ++brdCnt;
                else if (x is GridSplitter) ++splitterCnt;
            }
        }

        return (grCnt + brdCnt + splitterCnt, grCnt, brdCnt, splitterCnt);
    }

    /// <summary>
    /// debug print info
    /// </summary>
    /// <param name="wr">output (default console.out)</param>
    /// <param name="highLightControl">show an arrow on control focused</param>
    /// <param name="breakOnWarn">break debugger if warn encountered</param>
    public void PrintStructure(TextWriter? wr = null, Control? highLightControl = null, bool breakOnWarn = false)
    {
        if (wr is null) wr = Console.Out;

        CustomScanGrid((x) =>
        {
            var ctl = x.ctl;
            var ctlPos = x.ctlPos;
            var gr = x.gr;
            var grDir = x.grDir;
            var lvl = x.lvl;
            var ctlDir = x.ctlDir;

            var ctlTypeStr = ctl.GetType().Name;

            var indentStr = " ".Repeat(lvl);
            var dirStr = "";
            var posStr = grDir == GridSplitDirection.Horizontally ? "col:" : "row:";
            posStr += ctlPos;
            if (ctl is Grid ctlGrid) dirStr = $"[{ctlDir} cnt:{(ctlGrid).Children.Count(w => !(w is GridSplitter))}] ";

            if (ctl.Parent is Grid ctlParentGrid && ctlPos >= ctlParentGrid.Children.Count)
                Debugger.Break();

            var highStr = "";
            {
                if (highLightControl is not null &&
                    ctl is Border ctlBorder &&
                    ctlBorder.Child == highLightControl)
                    highStr = " <===";
            }

            var hcStr = ctl.GetHashCode().ToString();
            {
                if (ctl is Border ctlBorder)
                    hcStr += "," + ctlBorder.Child.GetHashCode().ToString();
            }

            var warnStr = "";
            if (ctl is Grid g)
            {
                var gc = GridGetChildrenCount(g);
                if (gc.grCnt == 1 && gc.brdCnt == 0 && g.Children.FirstOrDefault() is Grid gg)
                {                    
                    gc = GridGetChildrenCount(gg);
                    if (gc.grCnt == 1 && gc.brdCnt == 0)
                    {
                        warnStr = " ***";
                        if (breakOnWarn) Debugger.Break();
                    }
                }
            }

            wr.WriteLine($"{indentStr} {ctlTypeStr} {dirStr}({posStr}) <{hcStr}> {highStr}{warnStr}");

            return true;
        });
    }

    /// <summary>
    /// create a border with binding to highlight when FocusedControl is the given child
    /// </summary>
    /// <param name="child">child to insert int the border content</param>
    /// <returns>border</returns>
    Border newBorder(Control child)
    {
        var brd = new Border()
        {
            BorderThickness = new Thickness(FocusedControlBorderThickness)
        };
        brd.PointerPressed += (s, e) =>
        {
            if (s is Border sBrd)
            {
                FocusedControl = BorderGetChildControl(sBrd);
                var ctl = BorderGetChildControl(sBrd);
                ctl?.Focus();
            }
        };
        var bind = new MultiBinding()
        {
            Converter = focusedControlConverter,
            ConverterParameter = child
        };
        bind.Bindings.Add(new Binding()
        {
            Source = this
        });
        bind.Bindings.Add(new Binding("FocusedControl")
        {
            Source = this
        });
        brd.Bind(Border.BorderBrushProperty, bind);
        brd.Child = child;

        return brd;
    }

    RowDefinition RowStar(double size = 1) => new RowDefinition(size, GridUnitType.Star);
    ColumnDefinition ColStar(double size = 1) => new ColumnDefinition(size, GridUnitType.Star);

    /// <summary>
    /// called once after CreateControl method setup;
    /// it creates a grid (vertically splitted with 1 rows) with a border that contains the control;        
    /// </summary>
    void CreateInitialControl()
    {
        var initialChild = CreateControl!.Invoke();

        var brd = newBorder(initialChild);

        var gr = new Grid();
        gr.RowDefinitions.Add(RowStar());
        gr.Children.Add(brd);

        grRoot.Children.Add(gr);

        FocusedControl = initialChild;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// retrieve border associated with control;
    /// row and column attached properties always belong to control border and grid
    /// </summary>
    /// <param name="ctl">control</param>
    /// <returns>border</returns>
    Border? ControlGetParentBorder(Control ctl) => ctl.Parent as Border;

    /// <summary>
    /// retrieve control from given border
    /// </summary>
    /// <param name="brd">border that contains the control</param>
    /// <returns>control inside border</returns>
    Control? BorderGetChildControl(Border brd) => brd.Child as Control;

    /// <summary>
    /// retrieve grid associated to given ctl;
    /// a ctl in this manager always has a border parent and that border always has a grid
    /// </summary>
    /// <param name="ctl">control for which to retrieve associated grid</param>
    /// <returns>grid associated to the given ctl</returns>
    Grid? ControlGetParentGrid(Control ctl) => ControlGetParentBorder(ctl)?.Parent as Grid;

    /// <summary>
    /// retrieve split direction of the grid associated to the given ctl
    /// </summary>
    /// <param name="ctl">ctl for which to retrieve grid's associated direction</param>
    /// <returns>ctl associated grid direction</returns>
    GridSplitDirection? ControlGetSplitDirection(Control ctl)
    {
        if (ctl is null) return null;

        var ctlParentGrid = ControlGetParentGrid(ctl);

        if (ctlParentGrid is null) return null;

        return GridGetSplitDirection(ctlParentGrid);
    }

    /// <summary>
    /// retrieve split direction of the given grid
    /// </summary>
    /// <param name="gr">grid to retrieve direction</param>
    /// <returns>grid direction</returns>
    GridSplitDirection GridGetSplitDirection(Grid gr)
    {
        if (gr.RowDefinitions.Count > 0) return GridSplitDirection.Vertically;
        if (gr.ColumnDefinitions.Count > 0) return GridSplitDirection.Horizontally;
        throw new Exception("invalid grid without row or col definitions");
    }

    /// <summary>
    /// retrieve col or row depending on given dir horizontally or vertically
    /// </summary>
    /// <param name="dir">splitting direction</param>
    /// <param name="ctl">control to retrieve attached property</param>
    /// <returns>col or row control position</returns>
    int GetPos(GridSplitDirection dir, Control ctl)
    {
        switch (dir)
        {
            case GridSplitDirection.Horizontally: return Grid.GetColumn(ctl);
            case GridSplitDirection.Vertically: return Grid.GetRow(ctl);
        }
        throw new ArgumentException("invaild given dir");
    }

    /// <summary>
    /// set control col or row depending on given dir horizontally or vertically
    /// </summary>
    /// <param name="dir">splitting direction</param>
    /// <param name="ctl">control to set attached property</param>
    /// <param name="newPos">col or row new position</param>        
    void SetPos(GridSplitDirection dir, Control ctl, int newPos)
    {
        switch (dir)
        {
            case GridSplitDirection.Horizontally: Grid.SetColumn(ctl, newPos); break;
            case GridSplitDirection.Vertically: Grid.SetRow(ctl, newPos); break;
            default: throw new Exception("invalid grid without row or col definitions");
        }
    }

    /// <summary>
    /// insert a new Star col or row depending on given dir horizontally or vertically
    /// </summary>
    /// <param name="gr">grid to alter defs</param>
    /// <param name="dir">splitting direction</param>
    /// <param name="insPos">index where insert new star def</param>
    /// <param name="size">width or height of new star def</param>
    void InsertDef(Grid gr, GridSplitDirection dir, int insPos, double size = 1)
    {
        switch (dir)
        {
            case GridSplitDirection.Horizontally: gr.ColumnDefinitions.Insert(insPos, ColStar(size)); break;
            case GridSplitDirection.Vertically: gr.RowDefinitions.Insert(insPos, RowStar(size)); break;
            default: throw new Exception("invalid grid without row or col definitions");
        }
    }

    /// <summary>
    /// retrieve star size col or row depending on given dir
    /// </summary>
    /// <param name="gr">grid containing defs</param>
    /// <param name="dir">grid splitting direction</param>
    /// <param name="pos">index of existing def</param>
    /// <returns>star size</returns>
    double GridGetDefSize(Grid gr, GridSplitDirection dir, int pos)
    {
        GridLength gl;
        switch (dir)
        {
            case GridSplitDirection.Horizontally: gl = gr.ColumnDefinitions[pos].Width; break;
            case GridSplitDirection.Vertically: gl = gr.RowDefinitions[pos].Height; break;
            default: throw new Exception("invalid grid without row or col definitions");
        }
        if (!gl.IsStar) throw new Exception($"invalid grid col/row def not star");

        return gl.Value;
    }

    /// <summary>
    /// remove grid definition
    /// </summary>
    /// <param name="gr">grid of which remove a definition</param>
    /// <param name="dir">grid splitting direction</param>
    /// <param name="pos">position of grid definition to remove</param>
    void GridRemoveDef(Grid gr, GridSplitDirection dir, int pos)
    {
        switch (dir)
        {
            case GridSplitDirection.Horizontally: gr.ColumnDefinitions.RemoveAt(pos); break;
            case GridSplitDirection.Vertically: gr.RowDefinitions.RemoveAt(pos); break;
            default: throw new Exception("invalid grid without row or col definitions");
        }
    }

    /// <summary>
    /// set grid definition star size
    /// </summary>
    /// <param name="gr">grid to set definition size</param>
    /// <param name="dir">grid splitting direction</param>
    /// <param name="pos">pos of grid definition to change</param>
    /// <param name="starSize">new grid definition star size</param>
    void GridSetDefSize(Grid gr, GridSplitDirection dir, int pos, double starSize)
    {
        switch (dir)
        {
            case GridSplitDirection.Horizontally: gr.ColumnDefinitions[pos].Width = new GridLength(starSize, GridUnitType.Star); break;
            case GridSplitDirection.Vertically: gr.RowDefinitions[pos].Height = new GridLength(starSize, GridUnitType.Star); break;
            default: throw new Exception("invalid grid without row or col definitions");
        }
    }

    /// <summary>
    /// retrieve other dir (horizontally if given vertically, vertically if given horizontally)
    /// </summary>
    /// <param name="dir">input direction</param>
    /// <returns>other direction</returns>
    GridSplitDirection OtherDir(GridSplitDirection dir)
    {
        switch (dir)
        {
            case GridSplitDirection.Horizontally: return GridSplitDirection.Vertically;
            case GridSplitDirection.Vertically: return GridSplitDirection.Horizontally;
            default: throw new Exception("invalid grid without row or col definitions");
        }
    }

    /// <summary>
    /// split focused control over given direction; does nothing if focused control is null
    /// </summary>
    /// <param name="dir">split direction</param>
    public void Split(GridSplitDirection dir)
    {
        if (FocusedControl is null || CreateControl is null) return;

        var fCtl = FocusedControl;
        var fDir = ControlGetSplitDirection(fCtl);
        if (fDir is null) return;

        var fBrd = ControlGetParentBorder(fCtl);
        if (fBrd is null) return;

        var fBrdPos = GetPos(fDir.Value, fBrd);
        var fGr = ControlGetParentGrid(fCtl);
        if (fGr is null) return;

        var nBrd = newBorder(CreateControl());

        if (fDir == dir) // parallel
        {
            var halfSize = GridGetDefSize(fGr, fDir.Value, fBrdPos) / 2;

            GridRemoveDef(fGr, fDir.Value, fBrdPos);
            InsertDef(fGr, fDir.Value, fBrdPos, DistributeSplitSize ? 1 : halfSize);
            InsertDef(fGr, fDir.Value, fBrdPos + 1, DistributeSplitSize ? 1 : halfSize);
            foreach (var x in fGr.Children.Where(y => !(y is GridSplitter)).Cast<Control>())
            {
                var pos = GetPos(fDir.Value, x);
                if (pos > fBrdPos) SetPos(fDir.Value, x, pos + 1);
            }

            SetPos(fDir.Value, nBrd, fBrdPos + 1);
            fGr.Children.Add(nBrd);
        }
        else // transverse
        {
            fGr.Children.Remove(fBrd);

            var oGr = new Grid();
            SetPos(fDir.Value, oGr, fBrdPos); // new grid oGr take pos of fBrd
            fGr.Children.Add(oGr);

            var oDir = OtherDir(fDir.Value);
            InsertDef(oGr, oDir, 0);
            SetPos(oDir, fBrd, 0);
            oGr.Children.Add(fBrd);

            InsertDef(oGr, oDir, 1);
            SetPos(oDir, nBrd, 1);
            oGr.Children.Add(nBrd);
        }

        Adjust();

        FocusedControl = BorderGetChildControl(nBrd);
    }

    /// <summary>
    /// removed currently focused control; does nothing if focused control is null
    /// </summary>
    public void Remove()
    {
        if (FocusedControl is null) return;

        if (DebugWriter is not null)
        {
            DebugWriter.WriteLine();
            DebugWriter.WriteLine($"BEFORE REMOVAL");
            PrintStructure(DebugWriter, FocusedControl);
        }

        var fCtl = FocusedControl;
        var fDir = ControlGetSplitDirection(fCtl);
        if (fDir is null) return;

        var fBrd = ControlGetParentBorder(fCtl);
        if (fBrd is null) return;

        var fBrdPos = GetPos(fDir.Value, fBrd);
        var fGr = ControlGetParentGrid(fCtl);
        if (fGr is null) return;

        var fGrCnt = fGr.Children.Count(x => !(x is GridSplitter));

        if (fGr.Parent == grRoot && fGrCnt == 1) return;

        var removed = false;

        if (fGrCnt > 1)
        {
            var sizeToReintegrate = GridGetDefSize(fGr, fDir.Value, fBrdPos) / (fGrCnt - 1);

            foreach (var x in fGr.Children.Where(y => !(y is GridSplitter)).Cast<Control>())
            {
                var pos = GetPos(fDir.Value, x);
                if (pos > fBrdPos)
                {
                    var prevSize = GridGetDefSize(fGr, fDir.Value, pos);
                    SetPos(fDir.Value, x, pos - 1);
                    GridSetDefSize(fGr, fDir.Value, pos - 1, prevSize + sizeToReintegrate);
                }
            }
            fGr.Children.Remove(fBrd);
            GridRemoveDef(fGr, fDir.Value, fGrCnt - 1);

            removed = true;
        }
        else if (visitedControlDict.Count > 1) // remove fGr and integrate to parent
        {
            if (fGr.Parent is Grid pGr)
            {
                var pGrDir = GridGetSplitDirection(pGr);
                if (pGrDir != fDir)
                {
                    var fGrPos = GetPos(pGrDir, fGr);
                    var pGrCnt = pGr.Children.Count(x => !(x is GridSplitter));
                    var sizeToReintegrate = GridGetDefSize(pGr, pGrDir, fGrPos) / (pGrCnt - 1);
                    pGr.Children.Remove(fGr);
                    GridRemoveDef(pGr, pGrDir, fGrPos);
                    var distributeToNexts = fGrPos < pGrCnt - 1;
                    foreach (var x in pGr.Children.Where(y => !(y is GridSplitter)).Cast<Control>())
                    {
                        var pos = GetPos(pGrDir, x);
                        if (distributeToNexts) // removed internal then distribute to nexts
                        {
                            if (pos > fGrPos)
                            {
                                var prevSize = GridGetDefSize(pGr, pGrDir, pos - 1);
                                SetPos(pGrDir, x, pos - 1);
                                GridSetDefSize(pGr, pGrDir, pos - 1, prevSize + sizeToReintegrate);
                            }
                        }
                        else // removed last then distribute to prevs
                        {
                            var prevSize = GridGetDefSize(pGr, pGrDir, pos);
                            GridSetDefSize(pGr, pGrDir, pos, prevSize + sizeToReintegrate);
                        }
                    }
                }
                removed = true;
            }
        }

        if (removed)
        {
            visitedControlDict.Remove(fCtl);

            if (visitedControlDict.Count > 0)
                FocusedControl = visitedControlDict.OrderByDescending(w => w.Value).First().Key;
            else
                FocusedControl = null;
        }

        Adjust();

        var aliveControls = new HashSet<Control>();
        CustomScanGrid((x) =>
        {
            if (x.ctl is Border xBrd && xBrd.Child is Control brdChild)
                aliveControls.Add(brdChild);
            return true;
        });
        var removedVisitedCtls = visitedControlDict.Where(r => !aliveControls.Contains(r.Key)).Select(w => w.Key).ToList();

        foreach (var x in removedVisitedCtls) visitedControlDict.Remove(x);

        if (DebugWriter is not null)
        {
            DebugWriter.WriteLine();
            DebugWriter.WriteLine($"AFTER REMOVAL");
            PrintStructure(DebugWriter, null, true);
        }
    }

    /// <summary>
    /// compact grid with only 1 grid child, recalc GridSplitter and control Margin
    /// </summary>
    void Adjust()
    {
        var totBefore = GetTotalControlsCount();

        if (DebugWriter is not null)
        {
            DebugWriter.WriteLine();
            DebugWriter.WriteLine($"BEFORE ADJUST");
            PrintStructure(DebugWriter);
        }

        // search leaf grids            

        var leafGrids = new List<Grid>();

        void SearchLeafGrid(Grid gr)
        {
            var c = GridGetChildrenCount(gr);

            if (c.grCnt == 0)
            {
                leafGrids.Add(gr);
            }
            else if (c.grCnt > 0)
            {
                foreach (var x in gr.Children.OfType<Grid>())
                {
                    SearchLeafGrid(x);
                }
            }
        }

        var qGrRootFirstChild = grRoot.Children.FirstOrDefault();

        if (qGrRootFirstChild is null || qGrRootFirstChild is not Grid grRootFirstChildGrid)
            return;

        SearchLeafGrid(grRootFirstChildGrid);

        foreach (var gr in leafGrids)
        {
            if (gr.Parent is null) continue; // during this process an existing leaf can removed
            if (gr.Parent == grRoot) continue;

            Grid? pGr = null;

            if (gr.Parent is Grid qpGr)
            {
                pGr = qpGr;

                while (pGr is not null && pGr.Parent != grRoot)
                {
                    var c = GridGetChildrenCount(pGr);
                    if (c.grCnt == 1 && c.brdCnt == 0 && pGr.Parent is Grid tpGr)
                    {
                        var tc = GridGetChildrenCount(tpGr);
                        if (tc.grCnt == 1 && tc.brdCnt == 0 && pGr.Parent is Grid pGrParentGrid)
                            pGr = pGrParentGrid;
                        else
                            break;
                    }
                    else
                        break;
                }
            }

            if (pGr is not null && pGr != gr.Parent)
            {
                var grDir = GridGetSplitDirection(gr);
                var pGrdir = GridGetSplitDirection(pGr);

                if (grDir == pGrdir)
                {
                    var chlds = gr.Children.ToList();

                    var rowsDefs = gr.RowDefinitions.ToList();
                    var colsDefs = gr.ColumnDefinitions.ToList();
                    // if (rowsDefs.Count == 0 && colsDefs.Count == 0)
                    //     ;
                    gr.Children.Clear();

                    pGr.Children.Clear();
                    pGr.RowDefinitions.Clear();
                    foreach (var r in rowsDefs) pGr.RowDefinitions.Add(r);
                    pGr.ColumnDefinitions.Clear();
                    foreach (var c in colsDefs) pGr.ColumnDefinitions.Add(c);
                    foreach (var x in chlds) pGr.Children.Add(x);
                }
            }
        }
        //---

        // retrieve grids with only 1 border child

        var gridWithOneBorderChild = new List<Grid>();

        void SearchGridWithOneBorderChild(Grid gr)
        {
            var c = GridGetChildrenCount(gr);

            if (c.brdCnt == 1 && c.grCnt == 0)
            {
                gridWithOneBorderChild.Add(gr);
            }
            else if (c.grCnt > 0)
            {
                foreach (var x in gr.Children.OfType<Grid>())
                {
                    SearchGridWithOneBorderChild(x);
                }
            }
        }

        {
            if (grRoot.Children.FirstOrDefault() is Grid grRootFirstChild)
                SearchGridWithOneBorderChild(grRootFirstChild);
        }

        foreach (var gr in gridWithOneBorderChild)
        {
            var brd = gr.Children.First(w => w is Border) as Border;

            Grid? pGrLeader = null;

            var xGr = gr;
            while (xGr?.Parent != grRoot)
            {
                var pGr = xGr?.Parent as Grid;
                var pC = GridGetChildrenCount(pGr);
                if (pC.brdCnt == 0 && pC.grCnt == 1)
                {
                    pGrLeader = pGr;
                }
                else break;

                xGr = pGr;
            }

            if (pGrLeader is not null)
            {
                pGrLeader.Children.Clear();
                Grid.SetRow(brd, 0);
                Grid.SetColumn(brd, 0);
                (brd?.Parent as Grid)?.Children.Remove(brd);
                pGrLeader.Children.Add(brd);
            }
        }

        //---

        void RebuildSplitterAndSetMargin(Grid gr)
        {
            var grDir = GridGetSplitDirection(gr);
            // purge existing grid splitters
            foreach (var x in gr.Children.OfType<GridSplitter>().ToList()) gr.Children.Remove(x);

            foreach (var x in gr.Children.OfType<Control>().ToList())
            {
                var xPos = GetPos(grDir, x);

                if (xPos > 0)
                {
                    var xSplitter = newGridSplitter(grDir, xPos);
                    gr.Children.Add(xSplitter);
                }

                x.Margin = grDir switch
                {
                    GridSplitDirection.Horizontally => new Thickness(xPos == 0 ? 0 : SplitterThickness, 0, 0, 0),
                    GridSplitDirection.Vertically => new Thickness(0, xPos == 0 ? 0 : SplitterThickness, 0, 0),
                    _ => throw new ArgumentException($"invalid dir {grDir}")
                };

                if (x is Grid xGr)
                {
                    if (xGr.Children.Count == 0)
                        (xGr.Parent as Grid)?.Children.Remove(xGr);

                    else
                        RebuildSplitterAndSetMargin(xGr);
                }
            }
        }

        {
            if (grRoot.Children.FirstOrDefault() is Grid grRootFirstChild)
                RebuildSplitterAndSetMargin(grRootFirstChild);
        }

        var totAfter = GetTotalControlsCount();

        if (totAfter != totBefore)
        {
            if (DebugWriter is not null)
            {
                DebugWriter.WriteLine();
                DebugWriter.WriteLine($"ERROR");
                PrintStructure(DebugWriter);
            }
            throw new Exception($"internal error: adjust removed some controls");
        }
    }

    int GetTotalControlsCount()
    {
        int tot = 0;

        CustomScanGrid((x) =>
        {
            if (x.ctl is Border) ++tot;

            return true;
        });

        return tot;
    }

    /// <summary>
    /// helper walker
    /// - grDir : splitting direction of the gr
    /// - gr : grid that contains ctl
    /// - ctlPos : pos of the ctl in the gr
    /// - ctl : control ( it can grid, border, gridsplitter )
    /// - ctlDir : splitting direction of the ctl ( if its a grid )
    /// - lvl : nest level
    /// </summary>          
    /// <param name="continueScan">function that return true if scan continue or false if some break condition occurs</param>
    void CustomScanGrid(Func<(
        GridSplitDirection grDir,
        Grid gr,
        int ctlPos,
        Control ctl,
        GridSplitDirection? ctlDir,
        int lvl), bool> continueScan)
    {
        var lvl = 0;

        void ScanGrid(Grid gr)
        {
            var grDir = GridGetSplitDirection(gr);

            foreach (var x in gr.Children.OfType<Control>().ToList())
            {
                var xPos = GetPos(grDir, x);

                var ctlDir = (x is Grid grS) ? GridGetSplitDirection(grS) : new GridSplitDirection?();

                if (!continueScan((grDir, gr, xPos, x, ctlDir, lvl))) break;

                ++lvl;
                if (x is Grid xGr) ScanGrid(xGr);
                --lvl;
            }
        }

        if (grRoot.Children.FirstOrDefault() is Grid cGr)
            ScanGrid(cGr);
    }

}
