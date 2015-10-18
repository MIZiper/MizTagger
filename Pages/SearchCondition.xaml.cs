using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// SearchCondition.xaml 的交互逻辑
    /// </summary>
    public partial class SearchCondition : Page
    {
        private ObservableCollection<Filter> mFilters;

        public SearchCondition()
        {
            mFilters = new ObservableCollection<Filter>();
            InitializeComponent();
            listFilters.ItemsSource = mFilters;
            comboLogic.ItemsSource = System.Enum.GetValues(typeof(TagLogic));
            comboTypes.ItemsSource = (App.Current as App).Data.Types;
        }

        private void listFilters_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (listFilters.SelectedIndex==-1)
            {
                e.Handled = true;
            }
        }

        private void menuItemDelFilter_Click(object sender, RoutedEventArgs e)
        {
            Filter f = listFilters.SelectedItem as Filter;
            if (f != null)
            {
                mFilters.Remove(f);
            }
        }

        private void clkSearchByTags(object sender, RoutedEventArgs e)
        {
            if (mFilters.Count > 0)
            {
                Dictionary<TagLogic, byte[]> logic = mFilters.GetLogic();
                NavigationService.Navigate(new Pages.SearchResult(logic));
            }
        }

        private void comboTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb != null)
            {
                comboTypedTags.ItemsSource = (cb.SelectedItem as TagType).Tags;
            }
        }

        private void clkOK(object sender, RoutedEventArgs e)
        {
            if (comboLogic.SelectedIndex != -1 && comboTypedTags.SelectedIndex != -1)
            {
                Filter filter = new Filter() { Logic = (TagLogic)comboLogic.SelectedItem, Tag = comboTypedTags.SelectedItem as Tag };
                mFilters.Add(filter);
            }
        }

        private void clkSearchByString(object sender, RoutedEventArgs e)
        {
            string s = textSub.Text.Trim();
            if (s!=string.Empty)
            {
                NavigationService.Navigate(new Pages.SearchResult(s));
            }
        }
    }
}
