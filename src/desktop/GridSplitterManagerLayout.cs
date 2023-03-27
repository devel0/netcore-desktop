namespace SearchAThing.Desktop;

/// <summary>
/// data structure to hold split layout info
/// </summary>
public class GridSplitterManagerLayoutItem
{    

    public GridSplitterManagerLayoutItem? Parent { get; set; }

    /// <summary>
    /// type of split that represent this layout item
    /// </summary>    
    public GridSplitDirection SplitDirection { get; set; }

    /// <summary>
    /// the index as child of the parent split result
    /// </summary>    
    public int Index { get; set; }

    /// <summary>
    /// list of star sizes which split the control in the direction given by SplitDirection
    /// </summary>    
    public List<double> Sizes { get; set; }

    /// <summary>
    /// list of more split in children of this split
    /// </summary>    
    public List<GridSplitterManagerLayoutItem> Children { get; set; } = new List<GridSplitterManagerLayoutItem>();

    /// <summary>
    /// UID of the control emitted during save layout ; the size equals to Sizes array ; value of element is null
    /// for the children in this control that are split, uid of control emitted elsewhere
    /// </summary>    
    public List<int?> LeafUIDs { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"[{Index}] {SplitDirection} ");

        foreach (var s in Sizes.WithIndexIsLast()) sb.Append($"{s.item}(UID{LeafUIDs[s.idx]})*{(!s.isLast ? "," : "")}");

        return sb.ToString();
    }

}