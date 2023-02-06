namespace SearchAThing.Desktop;

public static partial class Constants
{

    #region ScrollableSlider

    public static readonly IBrush DEFAULT_ScrollableSlider_Color =
        new SolidColorBrush(Color.Parse("#84d3ff"));

    public const TextAlignment DEFAULT_ScrollableSlider_TextAlignment = TextAlignment.Right;

    public const double DEFAULT_ScrollableSlider_ResizeHandleWidth = 10;

    #endregion

    #region GridSplitterManager

    public const double DEFAULT_GridSplitterManager_FocusedControlBorderThickness = 1;

    public static readonly IBrush DEFAULT_GridSplitterManager_FocusedControlBorderBrush =
        new SolidColorBrush(Colors.Yellow);

    public const double DEFAULT_GridSplitterManager_SplitterThickness = 10;

    public static readonly IBrush DEFAULT_GridSplitterManager_SplitterBrush =
        new SolidColorBrush(Colors.DarkGray);

    public const bool DEFAULT_GridSplitterManager_DistributeSplitSize = true;

    #endregion
}