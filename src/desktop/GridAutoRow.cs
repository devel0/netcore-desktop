using Avalonia.Data;
using Avalonia.Interactivity;

namespace SearchAThing.Desktop;

// refs: https://docs.avaloniaui.net/docs/data-binding/creating-and-binding-attached-properties

public class GridAutoRow : AvaloniaObject
{

    static GridAutoRow()
    {
        AutoRowDefinitionsProperty.Changed.Subscribe(x => HandleAutoRowDefinitionsChanged(x.Sender, x.NewValue.GetValueOrDefault<bool>()));
    }

    private static void HandleAutoRowDefinitionsChanged(IAvaloniaObject sender, bool v)
    {
        if (v && sender is Grid grid)
        {
            grid.AttachedToVisualTree += Grid_AttachedToVisualTree;
        }
    }

    private static void Grid_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Grid grid && !GridAutoRow.GetSetupDone(grid))
        {
            int gridRow = 0;

            var namesConfigured = new Dictionary<string, int>();

            foreach (var child in grid.Children.OfType<Control>())
            {
                if (GridAutoRow.GetName(child) is string childAutoRowName)
                {
                    if (!namesConfigured.TryGetValue(childAutoRowName, out var configuredRow))
                    {
                        configuredRow = gridRow++;
                        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
                        namesConfigured.Add(childAutoRowName, configuredRow);
                    }
                    Grid.SetRow(child, configuredRow);
                }
                else
                    gridRow = Grid.GetRow(child);
            }

            GridAutoRow.SetSetupDone(grid, true);
        }
    }

    #region Name ( Attached Property )

    public static readonly AttachedProperty<string?> NameProperty =
        AvaloniaProperty.RegisterAttached<GridAutoRow, Interactive, string?>("Name", null, false, BindingMode.OneTime);

    public static void SetName(AvaloniaObject element, string? parameter) =>
        element.SetValue(NameProperty, parameter);

    public static string? GetName(AvaloniaObject element) =>
        element.GetValue(NameProperty);

    #endregion

    #region SetupDone ( Attached Property )

    public static readonly AttachedProperty<bool> SetupDoneProperty =
        AvaloniaProperty.RegisterAttached<GridAutoRow, Interactive, bool>("SetupDone", false, false, BindingMode.OneTime);

    public static void SetSetupDone(AvaloniaObject element, bool parameter) =>
        element.SetValue(SetupDoneProperty, parameter);

    public static bool GetSetupDone(AvaloniaObject element) =>
        element.GetValue(SetupDoneProperty);

    #endregion

    #region AutoRowDefinitions ( Attached Property )

    public static readonly AttachedProperty<bool> AutoRowDefinitionsProperty =
        AvaloniaProperty.RegisterAttached<GridAutoRow, Interactive, bool>("AutoRowDefinitions", false, false, BindingMode.OneTime);

    public static void SetAutoRowDefinitions(AvaloniaObject element, bool parameter) =>
        element.SetValue(AutoRowDefinitionsProperty, parameter);

    public static bool GetAutoRowDefinitions(AvaloniaObject element) =>
        element.GetValue(AutoRowDefinitionsProperty);

    #endregion

}