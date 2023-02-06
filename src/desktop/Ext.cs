namespace SearchAThing.Desktop;

public static partial class Ext
{

    /// <summary>
    /// Search from this control upward in the hierarchy searching given template type
    /// for all levels until root if default maxLevel (null) or for at most maxLevel of parents.
    /// </summary>
    public static T? SearchParent<T>(this IControl ctl, int? maxLevel = null) where T : Control
    {
        if (ctl.Parent is null) return null;
        if (ctl.Parent is T t) return t;
        if (maxLevel is not null)
        {
            --maxLevel;
            if (maxLevel <= 0) return null;
        }

        return ctl.Parent.SearchParent<T>(maxLevel);
    }

}