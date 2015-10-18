using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MizTagger
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShitAllInOne mData;

        public MainWindow()
        {
            mData = (App.Current as App).Data;
            InitializeComponent();
            comboUsers.ItemsSource = mData.Users;
            comboTypes.ItemsSource = mData.Types;
        }

        private void clkFrameBack(object sender, RoutedEventArgs e)
        {
            if (frame.CanGoBack)
            {
                frame.GoBack();
            }
        }

        private void comboUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb != null)
            {
                tabUser.DataContext = cb.SelectedItem as User;
            }
        }

        private void comboTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb != null)
            {
                //listTypedTags.ItemsSource = (cb.SelectedItem as TagType).Tags;
                tabTags.DataContext = cb.SelectedItem as TagType;
            }
        }

        private void clkAddTag(object sender, RoutedEventArgs e)
        {
            string s = textNewTag.Text.Trim();
            if (comboTypes.SelectedIndex!=-1 && s!=string.Empty)
            {
                TagType t = comboTypes.SelectedItem as TagType;
                mData.AddTag2Type(s, t);
                textNewTag.Text = string.Empty;
                textNewTag.Focus();
            }
        }

        private void clkAddType(object sender, RoutedEventArgs e)
        {
            string s = textNewType.Text.Trim();
            if (s!=string.Empty)
            {
                mData.AddTagType(s);
                textNewType.Text = string.Empty;
            }
        }


    }
}
