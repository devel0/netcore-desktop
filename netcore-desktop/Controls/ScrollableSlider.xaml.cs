using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace SearchAThing.Desktop;

public class ScrollableSlider : Slider, IStyleable
{

    #region ScrollStepDiv
    private double _ScrollStepDiv = 30;
    public static readonly DirectProperty<ScrollableSlider, double> ScrollStepDivProperty =
        AvaloniaProperty.RegisterDirect<ScrollableSlider, double>("ScrollStepDiv", o => o.ScrollStepDiv, (o, v) => o.ScrollStepDiv = v);
    public double ScrollStepDiv
    {
        get => _ScrollStepDiv;
        set => SetAndRaise(ScrollStepDivProperty, ref _ScrollStepDiv, value);
    }
    #endregion

    public ScrollableSlider()
    {
        this.InitializeComponent();
    }

    void HandleWheel(object sender, PointerWheelEventArgs e)
    {
        Value = (Value + (Maximum - Minimum) / ScrollStepDiv * e.Delta.Y).Clamp(Minimum, Maximum);
        e.Handled = true;
    }

    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        Parent.PointerWheelChanged += HandleWheel;

        if (Parent is Panel)
        {
            var g = Parent as Panel;
            if (g.Background is null) g.Background = new SolidColorBrush(Colors.Transparent);
        }
    }

    protected override void OnDetachedFromLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        Parent.PointerWheelChanged -= HandleWheel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    Type IStyleable.StyleKey => typeof(Slider);

}
