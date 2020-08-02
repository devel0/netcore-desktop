using System;
using System.Numerics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using static System.Math;

namespace SearchAThing
{

    /// <summary>
    /// specify StepValue if want step by fixed value or specify StepFactor (eg.0.1 for 10% of step) or
    /// specify StepMultiple if want step to positions multiple of a given value
    /// </summary>
    public class TextBoxSlider : UserControl
    {

        #region Value
        private object _Value = null;

        public static readonly DirectProperty<TextBoxSlider, object> ValueProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, object>("Value", o => o.Value, (o, v) => o.Value = v);

        public object Value
        {
            get => _Value;
            set
            {
                SetAndRaise(ValueProperty, ref _Value, value);
                InvalidateVisual();
            }
        }
        #endregion    

        #region Minimum
        private object _Minimum = null;

        public static readonly DirectProperty<TextBoxSlider, object> MinimumProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, object>("Minimum", o => o.Minimum, (o, v) => o.Minimum = v);

        public object Minimum
        {
            get => _Minimum;
            set => SetAndRaise(MinimumProperty, ref _Minimum, value);
        }
        #endregion    

        #region Maximum
        private object _Maximum = null;

        public static readonly DirectProperty<TextBoxSlider, object> MaximumProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, object>("Maximum", o => o.Maximum, (o, v) => o.Maximum = v);

        public object Maximum
        {
            get => _Maximum;
            set => SetAndRaise(MaximumProperty, ref _Maximum, value);
        }
        #endregion    

        #region SliderColor
        private IBrush _SliderColor = new SolidColorBrush(Color.Parse("#84d3ff"));//.FromRgb(255, 197, 131));

        public static readonly DirectProperty<TextBoxSlider, IBrush> SliderColorProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, IBrush>("SliderColor", o => o.SliderColor, (o, v) => o.SliderColor = v);

        public IBrush SliderColor
        {
            get => _SliderColor;
            set => SetAndRaise(SliderColorProperty, ref _SliderColor, value);
        }
        #endregion

        #region StepValue
        private object _StepValue = null;

        public static readonly DirectProperty<TextBoxSlider, object> StepValueProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, object>("StepValue", o => o.StepValue, (o, v) => o.StepValue = v);

        public object StepValue
        {
            get => _StepValue;
            set => SetAndRaise(StepValueProperty, ref _StepValue, value);
        }
        #endregion    

        #region StepFactor
        private object _StepFactor = null;

        public static readonly DirectProperty<TextBoxSlider, object> StepFactorProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, object>("StepFactor", o => o.StepFactor, (o, v) => o.StepFactor = v);

        /// <summary>
        /// eg. 0.1 for 10% of step
        /// </summary>
        /// <value></value>
        public object StepFactor
        {
            get => _StepFactor;
            set => SetAndRaise(StepFactorProperty, ref _StepFactor, value);
        }
        #endregion

        #region StepMultiple
        private object _StepMultiple = null;

        public static readonly DirectProperty<TextBoxSlider, object> StepMultipleProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, object>("StepMultiple", o => o.StepMultiple, (o, v) => o.StepMultiple = v);

        public object StepMultiple
        {
            get => _StepMultiple;
            set => SetAndRaise(StepMultipleProperty, ref _StepMultiple, value);
        }
        #endregion            

        #region AutoRoundDigits
        private int? _AutoRoundDigits = null;

        public static readonly DirectProperty<TextBoxSlider, int?> AutoRoundDigitsProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, int?>("AutoRoundDigits", o => o.AutoRoundDigits, (o, v) => o.AutoRoundDigits = v);

        public int? AutoRoundDigits
        {
            get => _AutoRoundDigits;
            set => SetAndRaise(AutoRoundDigitsProperty, ref _AutoRoundDigits, value);
        }
        #endregion                      

        #region TextAlignment
        private TextAlignment? _TextAlignment = Avalonia.Media.TextAlignment.Right;

        public static readonly DirectProperty<TextBoxSlider, TextAlignment?> TextAlignmentProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, TextAlignment?>("TextAlignment", o => o.TextAlignment, (o, v) => o.TextAlignment = v);

        bool applyTextAlignmentPending = false;

        public TextAlignment? TextAlignment
        {
            get => _TextAlignment;
            set
            {
                applyTextAlignmentPending = true;
                SetAndRaise(TextAlignmentProperty, ref _TextAlignment, value);
            }
        }
        #endregion       

        #region ResizerHandleWidth
        private double _ResizerHandleWidth = 10;

        public static readonly DirectProperty<TextBoxSlider, double> ResizerHandleWidthProperty =
            AvaloniaProperty.RegisterDirect<TextBoxSlider, double>("ResizerHandleWidth", o => o.ResizerHandleWidth, (o, v) => o.ResizerHandleWidth = v);

        bool ResizerHandleWidthPending = false;

        public double ResizerHandleWidth
        {
            get => _ResizerHandleWidth;
            set
            {
                SetAndRaise(ResizerHandleWidthProperty, ref _ResizerHandleWidth, value);
                ResizerHandleWidthPending = true;
            }
        }
        #endregion

        Grid grRoot;
        TextBox tbox;
        Border brd;
        Border brdResizer;

        public TextBoxSlider()
        {
            InitializeComponent();

            grRoot = this.FindControl<Grid>("grRoot");
            tbox = this.FindControl<TextBox>("tbox");
            brd = this.FindControl<Border>("brd");
            brdResizer = this.FindControl<Border>("brdResizer");

            DataContext = this;

            brd.BorderThickness = new Thickness(0, 0, ResizerHandleWidth, 0);

            tbox.PointerWheelChanged += tboxPointerWheelChanged;
            tbox.PointerMoved += tboxPointerMoved;
            brdResizer.PointerMoved += brdResizerPointerMoved;
            brdResizer.PointerWheelChanged += tboxPointerWheelChanged;
            brdResizer.PointerPressed += brdResizerPressed;
            brdResizer.PointerReleased += brdResizerReleased;

            // preview textbox press/release events
            this.tbox.AddHandler(TextBox.PointerPressedEvent, tboxPressed, RoutingStrategies.Tunnel);
            this.tbox.AddHandler(TextBox.PointerReleasedEvent, tboxReleased, RoutingStrategies.Tunnel);
        }

        PointerPoint? tboxPressedPos = null;

        void tboxPressed(object sender, PointerPressedEventArgs e)
        {
            tboxPressedPos = e.GetCurrentPoint(brd);

            evalTboxRightClick(e);
        }

        void tboxReleased(object sender, PointerReleasedEventArgs e)
        {
            tboxPressedPos = null;
        }

        PointerPoint? brdResizerPressedPos = null;

        void brdResizerPressed(object sender, PointerPressedEventArgs e)
        {
            brdResizerPressedPos = e.GetCurrentPoint(brd);
        }

        void brdResizerReleased(object sender, PointerReleasedEventArgs e)
        {
            brdResizerPressedPos = null;
        }

        public override void Render(Avalonia.Media.DrawingContext context)
        {
            base.Render(context);

            if (applyTextAlignmentPending)
            {
                tbox.TextAlignment = TextAlignment.HasValue ? TextAlignment.Value : Avalonia.Media.TextAlignment.Left;
                applyTextAlignmentPending = false;
            }

            if (ResizerHandleWidthPending)
            {
                brd.BorderThickness = new Thickness(0, 0, ResizerHandleWidth, 0);
                ResizerHandleWidthPending = false;
            }

            Recompute_brd();
        }

        void Recompute_brd()
        {
            if (Minimum == null || Maximum == null)
                brd.IsVisible = false;
            else
            {
                var val = Convert.ToDouble(Value);
                var min = Convert.ToDouble(Minimum);
                var max = Convert.ToDouble(Maximum);
                if (val < min)
                    brd.Width = 0;
                else if (val > max)
                    brd.Width = Bounds.Width;
                else
                    brd.Width = Bounds.Width * ((val - min) / (max - min));
                brd.IsVisible = true;
            }
        }

        void tboxPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (Minimum != null && Maximum != null)
                ChangeVal(x => x.val + x.step * e.Delta.Y);

            EvalResizer(e.GetCurrentPoint(brd));
        }

        void ChangeVal(Func<(double min, double max, double step, double val), double> fn)
        {
            var min = Convert.ToDouble(Minimum);
            var max = Convert.ToDouble(Maximum);
            var val = Convert.ToDouble(Value);

            var rngDelta = max - min;
            var step = rngDelta * .05;
            if (StepFactor != null)
                step = rngDelta * Convert.ToDouble(StepFactor);
            else if (StepValue != null)
                step = Convert.ToDouble(StepValue);
            else if (StepMultiple != null)
                step = Convert.ToDouble(StepMultiple);

            var newVal = fn((min, max, step, val));

            if (StepMultiple != null) newVal = newVal.MRound(step);

            if (AutoRoundDigits != null)
            {
                var autoRoundDigits = Convert.ToInt32(AutoRoundDigits);
                newVal = Round(newVal, autoRoundDigits);
            }
            
            var finalVal = newVal.Clamp(min, max);

            Value = finalVal;
        }

        void EvalResizer(PointerPoint brdRelativePoint)
        {
            Recompute_brd();

            if (Minimum == null || Maximum == null) return;

            var xdiff = Abs(brdRelativePoint.Position.X - brd.Width);

            var brdResizerVisible = false;
            var rW = Max(4, ResizerHandleWidth);
            if (xdiff <= rW)
            {
                brdResizer.Width = rW;
                brdResizer.Margin = new Thickness(brd.Width - rW, 0, 0, 0);
                brdResizerVisible = true;
            }
            brdResizer.IsVisible = brdResizerVisible;
        }

        void brdResizerPointerMoved(object sender, PointerEventArgs e)
        {
            if (Minimum == null || Maximum == null) return;

            if (brdResizerPressedPos != null)
            {
                var posx = e.GetCurrentPoint(brd).Position.X;

                var brdW = grRoot.Bounds.Width;

                var f = posx / brdW;

                ChangeVal(x => x.min + (x.max - x.min) * f);
            }
        }

        void evalTboxRightClick(PointerEventArgs e)
        {
            if (tboxPressedPos != null && tboxPressedPos.Properties.IsRightButtonPressed)
            {
                var posx = e.GetCurrentPoint(brd).Position.X;

                var brdW = grRoot.Bounds.Width;

                var f = posx / brdW;

                ChangeVal(x => x.min + (x.max - x.min) * f);
            }
        }

        void tboxPointerMoved(object sender, PointerEventArgs e)
        {
            evalTboxRightClick(e);
            EvalResizer(e.GetCurrentPoint(brd));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }

}