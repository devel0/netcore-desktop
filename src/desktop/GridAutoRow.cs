using Avalonia.Data;
using Avalonia.Interactivity;
using System.Reactive;

namespace SearchAThing.Desktop;

// refs: https://docs.avaloniaui.net/docs/data-binding/creating-and-binding-attached-properties

/// <summary>
/// Auto attach Grid.Row property to rows and expands RowDefinitions accordingly.
/// </summary>
public class GridAutoRow : AvaloniaObject
{

    static GridAutoRow()
    {
        AutoRowDefinitionsProperty.Changed.Subscribe(x =>
            HandleAutoRowDefinitionsChanged(x.Sender, x.NewValue.GetValueOrDefault<bool>()));
    }

    private static void HandleAutoRowDefinitionsChanged(AvaloniaObject sender, bool v)
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
                if (GridAutoRow.GetRef(child) is string childAutoRowName)
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

    #region Ref ( Attached Property )

    public static readonly AttachedProperty<string?> RefProperty =
        AvaloniaProperty.RegisterAttached<GridAutoRow, Interactive, string?>("Ref", null, false, BindingMode.OneTime);

    public static void SetRef(AvaloniaObject element, string? parameter) =>
        element.SetValue(RefProperty, parameter);

    public static string? GetRef(AvaloniaObject element) =>
        element.GetValue(RefProperty);

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