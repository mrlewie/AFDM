using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace AFDM.Helpers;
public class MultiplyIntByDoubleConverter : IValueConverter
{
    public MultiplyIntByDoubleConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // TODO: clean this up, it has no protection for nulls, bad strings
        var multiplyBy = (double)value;
        var controlValue  =System.Convert.ToDouble(parameter);

        return controlValue * multiplyBy;

    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
