using Avalonia.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

/// <summary>
/// Manage arrangement of control that allow split Vertically or Horizontally.
/// </summary>
public class GridSplitterManager<T> : Grid where T : Control, INotifyPropertyChanged
{

    #region property changed

    public new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    static SmartConverter smartConverter = new SmartConverter();

    Grid grRoot => this;

    /// <summary>
    /// dictionary that keep track of visited control age, used to retrieve last visited when remove
    /// </summary>    
    Dictionary<T, int> visitedControlDict = new Dictionary<T, int>();
    int visitedControlAge = 0;

    #region FocusedControl

    private T? _FocusedControl = null;
    /// <summary>
    /// FocusedControl
    /// </summary>
    public T? FocusedControl
    {
        get => _FocusedControl;
        set
        {
            var changed = value != _FocusedControl;
            if (changed)
            {
                _FocusedControl = value;

                if (value is not null)
                {
                    value.Focus();
                    if (visitedControlDict.ContainsKey(value))
                        visitedControlDict[value] = ++visitedControlAge;
                    else
                        visitedControlDict.Add(value, ++visitedControlAge);
                }

                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// List programmatically created controls
    /// </summary>
    public new ObservableCollection<T> Children { get; private set; } = new ObservableCollection<T>();

    #region FocusedControlBorderThickness

    private double _FocusedControlBorderThickness = DEFAULT_GridSplitterManager_FocusedControlBorderThickness;
    /// <summary>
    /// FocusedControlBorderThickness
    /// </summary>
    public double FocusedControlBorderThickness
    {
        get => _FocusedControlBorderThickness;
        set
        {
            var changed = value != _FocusedControlBorderThickness;
            if (changed)
            {
                _FocusedControlBorderThickness = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region FocusedControlBorderBrush

    private IBrush _FocusedControlBorderBrush = DEFAULT_GridSplitterManager_FocusedControlBorderBrush;
    /// <summary>
    /// FocusedControlBorderBrush
    /// </summary>
    public IBrush FocusedControlBorderBrush
    {
        get => _FocusedControlBorderBrush;
        set
        {
            var changed = value != _FocusedControlBorderBrush;
            if (changed)
            {
                _FocusedControlBorderBrush = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region CreateControl

    private Func<T>? _CreateControl = null;
    /// <summary>
    /// CreateControl
    /// </summary>
    public Func<T>? CreateControl
    {
        get => _CreateControl;
        set
        {
            var changed = value != _CreateControl;
            if (changed)
            {
                _CreateControl = value;
                OnPropertyChanged();
                if (value is not null) CreateInitialControl();
            }
        }
    }

    #endregion

    #region SplitterThickness

    private double _SplitterThickness = DEFAULT_GridSplitterManager_SplitterThickness;
    /// <summary>
    /// SplitterThickness
    /// </summary>
    public double SplitterThickness
    {
        get => _SplitterThickness;
        set
        {
            var changed = value != _SplitterThickness;
            if (changed)
            {
                _SplitterThickness = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region SplitterBrush

    private IBrush _SplitterBrush = DEFAULT_GridSplitterManager_SplitterBrush;
    /// <summary>
    /// SplitterBrush
    /// </summary>
    public IBrush SplitterBrush
    {
        get => _SplitterBrush;
        set
        {
            var changed = value != _SplitterBrush;
            if (changed)
            {
                _SplitterBrush = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region DistributeSplitSize

    private bool _DistributeSplitSize = DEFAULT_GridSplitterManager_DistributeSplitSize;
    /// <summary>
    /// DistributeSplitSize
    /// if false split will half its size; if true (default) it creates a def star so balanced between N axial parallel controls
    /// </summary>
    public bool DistributeSplitSize
    {
        get => _DistributeSplitSize;
        set
        {
            var changed = value != _DistributeSplitSize;
            if (changed)
            {
                _DistributeSplitSize = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// set to a valid text writer (eg.Console.Out) to debug structure processing;
    /// if null no debug output
    /// </summary>
    public TextWriter? DebugWriter { get; set; } = null;

    public delegate void ControlEventDelegate(T control);

    /// <summary>
    /// event fired when a child control created by split
    /// </summary>
    public event ControlEventDelegate ControlCreated;

    /// <summary>
    /// event fired when a child control removed by Remove() method
    /// </summary>
    public event ControlEventDelegate ControlRemoved;

    public GridSplitterManager()
    {
        this.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
    }

    /// <summary>
    /// create new grid splitter for given direction and set its pos
    /// </summary>
    /// <param name="dir">container grid direction</param>
    /// <param name="pos">pos to set the gridsplitter ( col or row depending on dir )</param>
    /// <returns>new gridsplitter</returns>
    GridSplitter newGridSplitter(GridSplitDirection dir, int pos)
    {
        var res = new GridSplitter() { Background = SplitterBrush };

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
    /// remove all layout except the topmost control view
    /// </summary>
    public void Clear()
    {
        T leaf;
        var g = grRoot;
        Grid? receiver = null;
        while (true)
        {
            if (g.Children.Count == 0) throw new Exception($"can't find a border");
            var q = g.Children.OfType<Border>().FirstOrDefault();
            if (q is not null && q.Child is T t)
            {
                if (receiver is not null)
                {
                    g.Children.Clear();
                    g = receiver;
                }
                g.Children.Clear();
                g.RowDefinitions.Clear();
                g.ColumnDefinitions.Clear();
                g.RowDefinitions.Add(RowStar());
                SetRow(q, 0);
                SetColumn(q, 0);
                g.Children.Add(q);
                leaf = t;
                FocusedControl = leaf;
                break;
            }
            else if (g.Children.Count == 1 && g.Children[0] is Grid)
            {
                g = (Grid)g.Children[0];
                receiver = g;
                continue;
            }

            var tmp = (Grid)g.Children[0];
            g.Children.Clear();

            g = tmp;
        }
        Children.Clear();

        ((Border)((Grid)grRoot.Children[0]).Children[0]).Margin = new Thickness();
    }

    /// <summary>
    /// configure control delegate
    /// </summary>
    /// <param name="ctl">control</param>
    /// <param name="uid">control identifier ( generated in SaveStructure )</param>
    public delegate void ConfigControlDelegate(T ctl, int uid);

    /// <summary>
    /// Json settings for <see cref="SaveStructure"/> and <see cref="LoadStructure"/>.
    /// </summary>    
    public JsonSerializerSettings JsonSettings => new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new[] { new StringEnumConverter() }
    };

    void Validate()
    {
#if !DEBUG
        return;
#endif
        if (grRoot.Children.Count != 1) throw new Exception($"grRoot Children must 1");

        void ValidFn(Control control)
        {
            if (control is Grid gr)
            {
                var cchildren = gr.Children.Where(r => r is Border || r is Grid);
                var grChildrenCount = cchildren.Count();

                if (control != grRoot && grChildrenCount == 1 && gr.Children.Count(r => r is Grid) == 1)
                    throw new Exception($"Non root grid must have more than 1 control");

                if (gr.RowDefinitions.Count != 0 && gr.ColumnDefinitions.Count != 0)
                    throw new Exception($"Grid must have rows or columns not either");

                var grDir = GridGetSplitDirection(gr);

                switch (grDir)
                {
                    case GridSplitDirection.Horizontally:
                        {
                            if (gr.ColumnDefinitions.Count != grChildrenCount)
                                throw new Exception($"Grid has {grChildrenCount} vs {gr.ColumnDefinitions.Count} col defs");
                        }
                        break;

                    case GridSplitDirection.Vertically:
                        {
                            if (gr.RowDefinitions.Count != grChildrenCount)
                                throw new Exception($"Grid has {grChildrenCount} vs {gr.RowDefinitions.Count} row defs");
                        }
                        break;
                }

                List<int> indexes = new List<int>();

                foreach (var child in cchildren)
                {
                    indexes.Add(GetPos(grDir, child));
                }

                for (int i = 0; i < indexes.Count; ++i)
                {
                    if (!indexes.Contains(i))
                        throw new Exception($"Grid children must have row or col index covering sequence 0..N-1");
                }

                if (indexes.Count > grChildrenCount)
                    throw new Exception($"Grid children must have row or col index covering sequence 0..{grChildrenCount - 1}");

                foreach (var child in cchildren)
                    ValidFn(child);
            }

            else if (control is Border brd)
            {
                if (brd.Child is not T t)
                    throw new Exception($"Border without control inside");
            }
        }

        ValidFn(grRoot);
    }

    public GridSplitterManagerLayoutItem ScanGrid(
        ConfigControlDelegate? emitControl = null,
        Action<(GridSplitterManagerLayoutItem item, Control control)>? controlVisited = null)
    {
        var lvl = 0;
        int uid = 0;

        GridSplitterManagerLayoutItem? ScanItem(GridSplitterManagerLayoutItem? parent, Control control)
        {
            int index = 0;

            if (parent is not null)
            {
                switch (parent.SplitDirection)
                {
                    case GridSplitDirection.Vertically:
                        {
                            index = Grid.GetRow(control);
                        }
                        break;

                    case GridSplitDirection.Horizontally:
                        {
                            index = Grid.GetColumn(control);
                        }
                        break;
                }
            }

            if (control is Grid gr)
            {
                var grDir = GridGetSplitDirection(gr);

                var sizes = new List<double>();

                switch (grDir)
                {
                    case GridSplitDirection.Horizontally:
                        {
                            if (gr.ColumnDefinitions.Count == 0)
                                sizes = new List<double> { 1 };
                            else
                                sizes = gr.ColumnDefinitions.Select(w => w.Width.Value).ToList();
                        }
                        break;

                    case GridSplitDirection.Vertically:
                        {
                            if (gr.RowDefinitions.Count == 0)
                                sizes = new List<double> { 1 };
                            else
                                sizes = gr.RowDefinitions.Select(w => w.Height.Value).ToList();
                        }
                        break;
                }

                var item = new GridSplitterManagerLayoutItem
                {
                    Level = lvl,
                    SplitDirection = grDir,
                    Sizes = sizes,
                    Children = new List<GridSplitterManagerLayoutItem>(),
                    Index = index
                };

                foreach (var child in gr.Children.OfType<Control>().ToList())
                {
                    ++lvl;
                    var q = ScanItem(item, child);
                    if (q is not null) item.Children.Add(q);
                    --lvl;
                }

                controlVisited?.Invoke((item, control));

                return item;
            }

            else if (control is Border ctlBorder && ctlBorder.Child is T t)
            {
                var item = new GridSplitterManagerLayoutItem
                {
                    Level = lvl,
                    Index = index,
                    LeafUID = uid,
                    Debug = t.GetHashCode().ToString()
                };

                emitControl?.Invoke(t, uid);
                ++uid;

                controlVisited?.Invoke((item, control));

                return item;
            }

            return null;
        }

        var res = ScanItem(null, grRoot.Children.First())!;

        res.SortChildren();

        return res;
    }

    /// <summary>
    /// create a serializable object that hold split configuration
    /// </summary>
    /// <param name="emitControl">optionally an action to save apart control info</param>    
    /// <seealso cref="JsonSettings"/>
    public GridSplitterManagerLayoutItem SaveStructure(ConfigControlDelegate? emitControl = null) =>
        ScanGrid(emitControl);

    /// <summary>
    /// load split layout from object ( see SaveStructure )
    /// </summary>
    /// <param name="layout">split layout object</param>
    /// <param name="reconfigureControl">optional configure control delegate</param>
    /// <seealso cref="JsonSettings"/>
    public void LoadStructure(GridSplitterManagerLayoutItem layout, ConfigControlDelegate? reconfigureControl = null)
    {
        Clear();

        void LoadLayout(GridSplitterManagerLayoutItem layout)
        {
            if (layout.SplitDirection is null || layout.Sizes is null || layout.Children is null) return;

            var ctls = Split(layout.SplitDirection.Value, layout.Sizes.ToArray());
            if (ctls is null) return;

            for (int ci = 0; ci < layout.Children.Count; ++ci)
            {
                var child = layout.Children[ci];

                FocusedControl = ctls[child.Index];

                if (child.Children is null && child.LeafUID is not null)
                    reconfigureControl?.Invoke(FocusedControl, child.LeafUID.Value);

                else
                    LoadLayout(child);
            }
        }

        LoadLayout(layout);
    }

    /// <summary>
    /// create a border with binding to highlight when FocusedControl is the given child
    /// </summary>
    /// <param name="child">child to insert int the border content</param>
    /// <returns>border</returns>
    Border newBorder(T child)
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

        brd.Child = child;

        return brd;
    }

    RowDefinition RowStar(double size = 1) => new RowDefinition(size, GridUnitType.Star);
    ColumnDefinition ColStar(double size = 1) => new ColumnDefinition(size, GridUnitType.Star);

    T? create_control()
    {
        if (CreateControl is null) return null;

        var res = CreateControl();

        ControlCreated?.Invoke(res);

        Children.Add(res);

        return res;
    }

    void control_removed(T ctl)
    {
        Children.Remove(ctl);
        ControlRemoved?.Invoke(ctl);
    }

    /// <summary>
    /// called once after CreateControl method setup;
    /// it creates a grid (vertically splitted with 1 rows) with a border that contains the control;        
    /// </summary>
    void CreateInitialControl()
    {
        var initialChild = create_control()!;

        var brd = newBorder(initialChild);

        var gr = new Grid();
        gr.RowDefinitions.Add(RowStar());
        gr.Children.Add(brd);

        grRoot.Children.Add(gr);

        FocusedControl = initialChild;
    }

    /// <summary>
    /// retrieve border associated with control;
    /// row and column attached properties always belong to control border and grid
    /// </summary>
    /// <param name="ctl">control</param>
    /// <returns>border</returns>
    Border? ControlGetParentBorder(T ctl) => ctl.Parent as Border;

    /// <summary>
    /// retrieve control from given border
    /// </summary>
    /// <param name="brd">border that contains the control</param>
    /// <returns>control inside border</returns>
    T? BorderGetChildControl(Border brd) => brd.Child as T;

    /// <summary>
    /// retrieve grid associated to given ctl;
    /// a ctl in this manager always has a border parent and that border always has a grid
    /// </summary>
    /// <param name="ctl">control for which to retrieve associated grid</param>
    /// <returns>grid associated to the given ctl</returns>
    Grid? ControlGetParentGrid(T ctl) => ControlGetParentBorder(ctl)?.Parent as Grid;

    /// <summary>
    /// retrieve split direction of the grid associated to the given ctl
    /// </summary>
    /// <param name="ctl">ctl for which to retrieve grid's associated direction</param>
    /// <returns>ctl associated grid direction</returns>
    GridSplitDirection? ControlGetSplitDirection(T ctl)
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

        return GridSplitDirection.Vertically;
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
    /// split in the direction given producing a set of given sizes children
    /// </summary>
    /// <param name="dir">direction split</param>
    /// <param name="sizes">list star size for produced children</param>
    public T[]? Split(GridSplitDirection dir, params double[] sizes)
    {
        if (FocusedControl is null || CreateControl is null) return null;

        var fCtl = FocusedControl;
        var fDir = ControlGetSplitDirection(fCtl);
        if (fDir is null) return null;

        var fBrd = ControlGetParentBorder(fCtl);
        if (fBrd is null) return null;

        var fBrdPos = GetPos(fDir.Value, fBrd);
        var fGr = ControlGetParentGrid(fCtl);
        if (fGr is null) return null;

        var splitted = new List<T>() { FocusedControl };

        for (int si = 0; si < sizes.Length - 1; ++si)
        {
            var s = Split(dir);
            if (s is null) return null;
            FocusedControl = s;
            splitted.Add(s);
        }

        for (int si = 0; si < sizes.Length; ++si)
        {
            var s = splitted[si];

            fGr = ControlGetParentGrid(s);
            GridSetDefSize(fGr, dir, si, sizes[si]);
        }

        return splitted.ToArray();
    }

    /// <summary>
    /// split focused control over given direction; does nothing if focused control is null
    /// </summary>
    /// <param name="dir">split direction</param>
    public T? Split(GridSplitDirection dir)
    {
        if (FocusedControl is null || CreateControl is null) return null;

        var fCtl = FocusedControl;
        var fDir = ControlGetSplitDirection(fCtl);
        if (fDir is null) return null;

        var fBrd = ControlGetParentBorder(fCtl);
        if (fBrd is null) return null;

        var fBrdPos = GetPos(fDir.Value, fBrd);
        var fGr = ControlGetParentGrid(fCtl);
        if (fGr is null) return null;

        var nBrdContent = create_control()!;

        var nBrd = newBorder(nBrdContent);

        if (fGr.Children.Count == 1)
        {
            fDir = dir;
            switch (fDir)
            {
                case GridSplitDirection.Horizontally:
                    {
                        if (fGr.ColumnDefinitions.Count == 0)
                        {
                            fGr.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            fGr.RowDefinitions.Clear();
                            Grid.SetColumn(fGr.Children.First(), 0);
                        }
                    }
                    break;

                case GridSplitDirection.Vertically:
                    {
                        if (fGr.RowDefinitions.Count == 0)
                        {
                            fGr.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                            fGr.ColumnDefinitions.Clear();
                            Grid.SetRow(fGr.Children.First(), 0);
                        }
                    }
                    break;
            }
        }

        if (fDir == dir) // parallel
        {
            var size = GridGetDefSize(fGr, fDir.Value, fBrdPos) / 2;

            if (DistributeSplitSize)
            {
                if (size > 1)
                {
                    var sum = 0d;
                    int pos = 0;
                    foreach (var x in fGr.Children.Where(y => !(y is GridSplitter)).Cast<Control>())
                    {
                        sum += GridGetDefSize(fGr, fDir.Value, pos++);
                    }
                    size = sum / pos;
                }
                else
                    size = 1;
            }

            GridRemoveDef(fGr, fDir.Value, fBrdPos);
            InsertDef(fGr, fDir.Value, fBrdPos, size);
            InsertDef(fGr, fDir.Value, fBrdPos + 1, size);
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

        Validate();

        return FocusedControl;
    }

    /// <summary>
    /// removed currently focused control; does nothing if focused control is null
    /// </summary>
    public void Remove()
    {
        if (FocusedControl is null) return;

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
        else if (visitedControlDict.Count >= 1) // remove fGr and integrate to parent
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
            control_removed(fCtl);

            if (visitedControlDict.Count > 0)
                FocusedControl = visitedControlDict.OrderByDescending(w => w.Value).First().Key;
            else
                FocusedControl = null;
        }

        Adjust();

        Validate();
    }

    /// <summary>
    /// compact grid with only 1 grid child, recalc GridSplitter and control Margin
    /// </summary>
    void Adjust()
    {
        var totBefore = GetTotalControlsCount();

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

        // search grid with only 1 grid

        IEnumerable<Grid> SearchGridWith1Grid(Grid gr)
        {
            var q = GridGetChildrenCount(gr);
            if (q.brdCnt == 0 && q.grCnt == 1)
                yield return gr;

            foreach (var c in gr.Children.OfType<Grid>())
            {
                foreach (var res in SearchGridWith1Grid(c))
                    yield return res;
            }
        }

        while (true)
        {
            var q = SearchGridWith1Grid(grRootFirstChildGrid).FirstOrDefault();
            if (q is null) break;

            if (q.Parent is Grid pGrid)
            {
                var pDir = GridGetSplitDirection(pGrid);
                var qDir = GridGetSplitDirection(q);
                var qSizes = new List<double>();
                switch (qDir)
                {
                    case GridSplitDirection.Horizontally:
                        {
                            foreach (var y in q.ColumnDefinitions)
                                qSizes.Add(y.Width.Value);
                        }
                        break;

                    case GridSplitDirection.Vertically:
                        {
                            foreach (var y in q.RowDefinitions)
                                qSizes.Add(y.Height.Value);
                        }
                        break;
                }

                var qIdx = pDir == GridSplitDirection.Horizontally ?
                    Grid.GetColumn(q) : Grid.GetRow(q);
                var qSize = pDir == GridSplitDirection.Horizontally ?
                    pGrid.ColumnDefinitions[qIdx].Width.Value :
                    pGrid.RowDefinitions[qIdx].Height.Value;
                pGrid.Children.Remove(q);
                var movedChilds = q.Children.OfType<Grid>().ToList();
                q.Children.Clear();

                if (movedChilds.Count != 1) throw new InternalError("");

                pGrid.Children.Add(movedChilds[0]);
                switch (pDir)
                {
                    case GridSplitDirection.Horizontally:
                        Grid.SetColumn(movedChilds[0], qIdx);
                        break;

                    case GridSplitDirection.Vertically:
                        Grid.SetRow(movedChilds[0], qIdx);
                        break;
                }
            }
            else if (q != grRoot)
                break;
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
            throw new InternalError($"internal error: adjust removed some controls");
        }
    }

    int GetTotalControlsCount()
    {
        int tot = 0;

        ScanGrid(controlVisited: (x) =>
        {
            if (x.control is Border) ++tot;
        });

        return tot;
    }

}
