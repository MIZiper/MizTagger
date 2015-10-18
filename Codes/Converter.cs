using System.Windows.Data;
using System.Windows.Media;

namespace MizTagger.Converter
{
    class Logic2Color:IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TagLogic logic = (TagLogic)value;
            SolidColorBrush clr;

            switch (logic)
            {
                case TagLogic.ContainOne:
                    clr = new SolidColorBrush(Colors.Blue);
                    break;
                case TagLogic.ContainAll:
                    clr = new SolidColorBrush(Colors.Green);
                    break;
                case TagLogic.NotAll:
                    clr = new SolidColorBrush(Colors.Pink);
                    break;
                case TagLogic.NotAny:
                    clr = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    clr = new SolidColorBrush(Colors.Purple);
                    break;
            }
            return clr;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Priv2Bool:IValueConverter
    {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            UserPrivilege priv = (UserPrivilege)value;
            UserPrivilege para = (UserPrivilege)parameter;
            return priv.HasFlag(para);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}