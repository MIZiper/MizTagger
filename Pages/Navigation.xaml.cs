using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MizTagger.Pages
{
    /// <summary>
    /// Navigation.xaml 的交互逻辑
    /// </summary>
    public partial class Navigation : Page
    {
        public static Dictionary<string, string> BTN2PAGE = new Dictionary<string, string>()
        {
            {"List All","Pages/SearchResult.xaml"},
            {"Add Item","Pages/EditItem.xaml"},
            {"Search","Pages/SearchCondition.xaml"}
        };

        public Navigation()
        {
            InitializeComponent();
        }

        private void clkNavBtn(object sender, RoutedEventArgs e)
        {
            Button s = sender as Button;
            if (s != null)
            {
                string str = s.Content as string;
                if (BTN2PAGE.ContainsKey(str))
                {
                    NavigationService.Navigate(new Uri(BTN2PAGE[str], UriKind.Relative));
                }
            }
        }
    }
}
