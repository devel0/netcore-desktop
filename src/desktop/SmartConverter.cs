using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace SearchAThing.Desktop;

/// <summary>
/// Smart Converter    
///     
/// <b>Parameter ( general form )</b>:<br/>
///   - `?FN targetValueIfMatch targetValueIfNOTMatch`<br/>
///   - `valueToMatch targetValueIfMatch targetValueIfNOTMatch`<br/>
///
/// <b>Functions</b>:<br/>
/// - ?IsNull : true if given argument is null<br/>
/// - ?{gt,lt,gte,ltq,eq} v : true if given argument equals to parameter v<br/>
/// - ?{add,sub} v : add/sub v<br/>
/// - ?Lighter : double given argument brighness<br/>
/// - ?Lighter v : increase of given argument v=0-1 brighness<br/>
/// - ?Darker : half brighness<br/>
/// - ?Darker v : decrease to v=0-1 brightness<br/>
/// - ?Round dec : round value number to given decimals<br/>
///
/// <b>Supported target types</b>:<br/>
/// - <b>boolean</b><br/>
/// - <b>double</b><br/>
/// - <b>Thickness</b><br/>
/// - <b>Brush</b><br/>
/// - <b>FontWeight</b><br/>
/// 
/// <b>Examples ( converter parameter )</b>:<br/>
/// - `true Yellow Transparent` ( valueType: boolean, targetType:IBrush ) converts to Yellow if value is true or transparent if value if false<br/>
/// - `?Darker 0.8` ( valueType: IBrush, targetType: IBrush ) converts to darker brush reducing at 80% its bridhtness<br/>
/// - `?IsNull 1.2 4.5` ( valueType: object, targetType: double ) converts to 1.2 if value is null, 4.5 otherwise<br/>
/// - `true false true` ( valueType: bool, targetType: bool ) converts to false if given value is true, viceversa otherwise<br/>
/// 
/// </summary>
public class SmartConverter : IValueConverter
{
    #region Instance

    static object lckInstance = new Object();
    static SmartConverter? _Instance = null;
    public static SmartConverter Instance
    {
        get
        {
            if (_Instance is null)
            {
                lock (lckInstance)
                {
                    if (_Instance is null)
                        _Instance = new SmartConverter();
                }
            }
            return _Instance;
        }
    }

    #endregion


    static readonly Type typeofInt = typeof(int);
    static readonly Type typeofDecimal = typeof(decimal);
    static readonly Type typeofFloat = typeof(float);
    static readonly Type typeofDouble = typeof(double);
    static readonly Type typeofBoolean = typeof(Boolean);
    static readonly Type typeofBooleanNullable = typeof(bool?);
    static readonly Type typeofThickness = typeof(Thickness);
    static readonly Type typeofGridLength = typeof(GridLength);
    static readonly Type typeofIBrush = typeof(IBrush);
    static readonly Type typeofFontWeight = typeof(FontWeight);
    static readonly BrushConverter brushCvt = new BrushConverter();

    public object? operand(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        Type? typeOfValue = value is null ? null : value.GetType();
        if (parameter is null) return null;

        var pstr = parameter.ToString();
        var ss = pstr.Split(' ');

        object? res = null;
        string? str = null;
        bool matches = false;
        var matchFn = false;

        int matchIPos = 1;

        #region ? operators
        if (pstr.StartsWith("?"))
        {
            matchFn = true;
            if (pstr.StartsWith("?IsNull ")) matches = value is null;

            else if (pstr == "?Darker" || pstr.StartsWith("?Darker "))
            {
                if (value is null) return null;

                var brush = value as SolidColorBrush;

                if (brush is not null)
                {
                    var col = brush.Color;

                    var f = 0.5;

                    if (ss.Length > 1)
                        f = double.Parse(ss[1]);

                    var newCol = Color.FromArgb(col.A,
                        (byte)(col.R / 255d * f * 255d),
                        (byte)(col.G / 255d * f * 255d),
                        (byte)(col.B / 255d * f * 255d));

                    res = new SolidColorBrush(newCol);
                }

                return res;
            }

            else if (pstr == "?Lighter" || pstr.StartsWith("?Lighter "))
            {
                if (value is null) return null;

                var brush = value as SolidColorBrush;

                if (brush is not null)
                {
                    var col = brush.Color;

                    var f = 0.5;

                    if (ss.Length > 1)
                        f = double.Parse(ss[1]);

                    var newCol = Color.FromArgb(col.A,
                        (byte)(col.R + (1d - col.R / 255d) * f * 255d),
                        (byte)(col.G + (1d - col.G / 255d) * f * 255d),
                        (byte)(col.B / (1d - col.B / 255d) * f * 255d));

                    res = new SolidColorBrush(newCol);
                }

                return res;
            }

            else if (pstr.StartsWith("?Round "))
            {
                var dec = int.Parse(ss[1]);

                if (value is not null)
                {
                    if (typeOfValue == typeofDecimal) res = Round((decimal)value, dec);
                    else if (typeOfValue == typeofFloat) res = Round((float)value, dec);
                    else if (typeOfValue == typeofDouble) res = Round((double)value, dec);
                }

                return System.Convert.ChangeType(res, targetType);
            }

            else if (
                pstr.StartsWith("?gt ") || pstr.StartsWith("?gte ") ||
                pstr.StartsWith("?lt ") || pstr.StartsWith("?lte ") ||
                pstr.StartsWith("?eq "))
            {
                ++matchIPos;

                bool operand<T>(T x, T y)
                {
                    var cmp = Comparer<T>.Default.Compare(x, y);

                    if (pstr.StartsWith("?gt ")) return cmp > 0;
                    else if (pstr.StartsWith("?gte ")) return cmp > 0 || cmp == 0;
                    else if (pstr.StartsWith("?lt ")) return cmp < 0;
                    else if (pstr.StartsWith("?lte ")) return cmp < 0 || cmp == 0;
                    else if (pstr.StartsWith("?eq ")) return cmp == 0;

                    return false;
                }

                if (value is not null)
                {
                    if (typeOfValue == typeofDecimal) matches = operand((decimal)value, decimal.Parse(ss[1], CultureInfo.InvariantCulture));
                    else if (typeOfValue == typeofFloat) matches = operand((float)value, float.Parse(ss[1], CultureInfo.InvariantCulture));
                    else if (typeOfValue == typeofDouble) matches = operand((double)value, double.Parse(ss[1], CultureInfo.InvariantCulture));
                    else if (typeOfValue == typeofInt) matches = operand((int)value, int.Parse(ss[1], CultureInfo.InvariantCulture));
                }
            }

            else if (
                pstr.StartsWith("?add ") || pstr.StartsWith("?sub "))
            {

                if (value is not null)
                {
                    if (typeOfValue == typeofDecimal)
                    {
                        var y = decimal.Parse(ss[1], CultureInfo.InvariantCulture);
                        if (pstr.StartsWith("?add ")) res = (decimal)value + y;
                        else if (pstr.StartsWith("?sub ")) res = (decimal)value - y;
                    }

                    else if (typeOfValue == typeofFloat)
                    {
                        var y = float.Parse(ss[1], CultureInfo.InvariantCulture);
                        if (pstr.StartsWith("?add ")) res = (float)value + y;
                        else if (pstr.StartsWith("?sub ")) res = (float)value - y;
                    }

                    else if (typeOfValue == typeofDouble)
                    {
                        var y = double.Parse(ss[1], CultureInfo.InvariantCulture);
                        if (pstr.StartsWith("?add ")) res = (double)value + y;
                        else if (pstr.StartsWith("?sub ")) res = (double)value - y;
                    }

                    else if (typeOfValue == typeofInt)
                    {
                        var y = int.Parse(ss[1], CultureInfo.InvariantCulture);
                        if (pstr.StartsWith("?add ")) res = (int)value + y;
                        else if (pstr.StartsWith("?sub ")) res = (int)value - y;
                    }

                    if (res is not null)
                    {
                        return System.Convert.ChangeType(res, targetType);
                    }
                }
            }

            else matchFn = false;
        }
        #endregion

        else if (typeOfValue?.IsEnum == true)
        {
            matchFn = true;
            matches = value is not null ? (value.ToString() == ss[0]) : false;
        }

        if (!matchFn)
        {
            str = value is null ? "" : value.ToString();

            if (targetType == typeofBoolean || typeOfValue == typeofBoolean)
            {
                var tl = str.ToLower();
                if (tl == "true" || tl == "false") str = tl;
            }

            matches = str == ss[0].ToLower();
            if (!matches && ss.Length <= 2) return null;
        }

        var i = matches ? matchIPos : matchIPos + 1;

        if (targetType == typeofBoolean)
        {
            res = bool.Parse(ss[i]);
        }

        else if (targetType == typeofBooleanNullable)
        {
            res = (bool?)bool.Parse(ss[i]);
        }

        else if (targetType == typeofThickness)
        {
            res = new Thickness(int.Parse(ss[i]));
        }

        else if (targetType == typeofIBrush)
        {
            res = brushCvt.ConvertFromString(ss[i]);
        }

        else if (targetType == typeofDouble)
        {
            res = double.Parse(ss[i]);
        }

        else if (targetType == typeofFontWeight)
        {
            switch (ss[i])
            {
                case "Normal": res = FontWeight.Normal; break;
                case "Bold": res = FontWeight.Bold; break;
                default: throw new Exception($"unsupported fontweight converter for {ss[i]}");
            }
        }

        else if (targetType == typeofGridLength)
        {

            switch (ss[i])
            {
                case "*": res = new GridLength(1, GridUnitType.Star); break;
                case "Auto": res = new GridLength(1, GridUnitType.Auto); break;
                default: res = new GridLength(1, GridUnitType.Auto); break;
            }
        }

        else if (targetType == typeofIBrush)
        {
            res = brushCvt.ConvertFrom(ss[i]);
        }

        else
            res = System.Convert.ChangeType(ss[i], targetType);

        return res;
    }

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        operand(value, targetType, parameter, culture);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        operand(value, targetType, parameter, culture) ?? value;

}
