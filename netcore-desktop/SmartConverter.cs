using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Brush = Avalonia.Media.Brush;
using Color = Avalonia.Media.Color;

namespace SearchAThing
{

    /// <summary>
    /// smart convert
    ///     
    /// - *converter parameter*
    ///   - `?FN targetValueIfMatch targetValueIfNOTMatch`
    ///   - `valueToMatch targetValueIfMatch targetValueIfNOTMatch`
    /// 
    /// **parameter examples**:
    /// - `true Yellow Transparent` ( valueType: boolean, targetType:IBrush ) makes Yellow if value is true or transparent if value if false
    /// - `?Darker 0.8` ( valueType: IBrush, targetType: IBrush ) makes value brush darker reducing to 80% its bridhtness
    /// - `?IsNull 1.2 4.5` ( valueType: object, targetType: double ) makes 1.2 if value is null, 4.5 otherwise
    /// - `true false true` ( valueType: bool, targetType: bool ) makes negate boolean value
    /// 
    /// **functions**:
    /// - ?IsNull match notMatch
    /// - ?Lighter : double brighness
    /// - ?Lighter v : increase of v=0-1 brighness
    /// - ?Darker : half brighness
    /// - ?Darker v : decrease to v=0-1 brightness
    /// 
    /// **supported targetTypes**:
    /// - *boolean* ( target values : "true", "false" )
    /// - *thickness* ( target values : eg. "20" for Thickness(20) )
    /// - *brush* ( target values : eg. "Red" or "#ff0000" )
    /// - *double* ( target values : eg. "1.2" )
    /// </summary>
    public class SmartConverter : IValueConverter
    {

        static readonly Type typeofBoolean = typeof(Boolean);
        static readonly Type typeofThickness = typeof(Thickness);
        static readonly Type typeofIBrush = typeof(IBrush);
        //static readonly Type typeofVisibility = typeof(Visibility);
        static readonly Type typeofDouble = typeof(double);
        static readonly Type typeofVector3 = typeof(Vector3);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type typeOfValue = value == null ? null : value.GetType();
            var pstr = (string)(parameter.ToString());
            var ss = pstr.Split(' ');

            object res = null;
            string str = null;
            bool matches = false;
            var matchFn = false;
            if (pstr.StartsWith("?"))
            {
                matchFn = true;
                if (pstr.StartsWith("?IsNull "))
                {
                    matches = value == null;
                }
                else if (pstr == "?Darker" || pstr.StartsWith("?Darker "))
                {
                    if (value == null) return null;

                    var brush = value as SolidColorBrush;

                    var col = brush.Color;

                    var f = 0.5;

                    if (ss.Length > 1)
                        f = double.Parse(ss[1]);

                    var newCol = Color.FromArgb(col.A,
                        (byte)(col.R / 255d * f * 255d),
                        (byte)(col.G / 255d * f * 255d),
                        (byte)(col.B / 255d * f * 255d));

                    res = new SolidColorBrush(newCol);

                    return res;
                }
                else if (pstr == "?Lighter" || pstr.StartsWith("?Lighter "))
                {
                    if (value == null) return null;

                    var brush = value as SolidColorBrush;

                    var col = brush.Color;

                    var f = 0.5;

                    if (ss.Length > 1)
                        f = double.Parse(ss[1]);

                    var newCol = Color.FromArgb(col.A,
                        (byte)(col.R + (1d - col.R / 255d) * f * 255d),
                        (byte)(col.G + (1d - col.G / 255d) * f * 255d),
                        (byte)(col.B / (1d - col.B / 255d) * f * 255d));

                    res = new SolidColorBrush(newCol);

                    return res;
                }
                else matchFn = false;
            }

            if (!matchFn)
            {
                str = (string)(value == null ? "" : value.ToString());

                if (targetType == typeofBoolean || typeOfValue == typeofBoolean)
                {
                    var tl = str.ToLower();
                    if (tl == "true" || tl == "false") str = tl;
                }

                matches = str == ss[0];
                if (!matches && ss.Length <= 2) return null;
            }

            var i = matches ? 1 : 2;

            if (targetType == typeofBoolean)
            {
                res = bool.Parse(ss[i]);
            }
            else if (targetType == typeofThickness)
            {
                res = new Thickness(int.Parse(ss[i]));
            }
            else if (targetType == typeofIBrush)
            {
                res = new SolidColorBrush(Color.Parse(ss[i]));
            }
            else if (targetType == typeofDouble)
            {
                res = double.Parse(ss[i]);
            }
            else
                res = System.Convert.ChangeType(ss[i], targetType);

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}