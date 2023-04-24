using Newtonsoft.Json;

namespace SearchAThing.Desktop;

/// <summary>
/// data structure to hold split layout info
/// </summary>
public class GridSplitterManagerLayoutItem
{

    /// <summary>
    /// Nesting level.
    /// </summary>
    [JsonIgnore]
    public int Level { get; set; }

    /// <summary>
    /// The item is a grid that split in this direction with <see cref="Sizes"/>.
    /// </summary>    
    public GridSplitDirection? SplitDirection { get; set; }

    /// <summary>
    /// If the item is a grid, this specify the list of star sizes for rows/cols depending of <see cref="SplitDirection"/>.
    /// </summary>    
    public List<double>? Sizes { get; set; }

    /// <summary>
    /// List of chidren items ( they can be grid or leafs ).
    /// </summary>    
    public List<GridSplitterManagerLayoutItem>? Children { get; set; }

    /// <summary>
    /// If this item is a leaf the value specify the UID of the control emitted during save layout.
    /// </summary>    
    public int? LeafUID { get; set; }

    /// <summary>
    /// Index of the child.
    /// </summary>
    // [JsonIgnore]
    public int Index { get; set; }

    /// <summary>
    /// Debug purpose field.
    /// </summary>    
#if !DEBUG
    [JsonIgnore]
#endif
    public string? Debug { get; set; }

    public void SortChildren()
    {
        if (Children is not null)
        {
            Children = Children.OrderBy(w => w.Index).ToList();

            foreach (var x in Children) x.SortChildren();
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (SplitDirection is not null && Sizes is not null)
            return $"{"  ".Repeat(Level)}GRID {SplitDirection} sizes:{string.Join(",", Sizes.ToString())}";

        else
            return $"{"  ".Repeat(Level)}LEAF uid:{LeafUID}";
    }
}
