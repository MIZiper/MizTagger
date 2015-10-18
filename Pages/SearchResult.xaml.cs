using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace MizTagger.Pages
{
    /// <summary>
    /// SearchResult.xaml 的交互逻辑
    /// </summary>
    public partial class SearchResult : Page
    {
        private ObservableCollection<Item> mItems;
        private ShitAllInOne mData;

        public SearchResult()
        {
            mData = (App.Current as App).Data;
            InitializeComponent();
            mItems = mData.Items;
            listItems.ItemsSource = mItems;
        }

        public SearchResult(Dictionary<TagLogic, byte[]> logic)
        {
            mData = (App.Current as App).Data;
            InitializeComponent();
            int i = 0;
            mItems = new ObservableCollection<Item>();
            listItems.ItemsSource = mItems;
            prg.Maximum = mData.Items.Count;
            foreach (Item item in mData.Items)
            {
                if (item.Tagum.MatchLogic(logic))
                {
                    mItems.Add(item);
                }
                i++;
                prg.Value = i;
            }
        }

        public SearchResult(string s)
        {
            mData = (App.Current as App).Data;
            InitializeComponent();
            int i = 0;
            mItems = new ObservableCollection<Item>();
            listItems.ItemsSource = mItems;
            prg.Maximum = mData.Items.Count;
            foreach (Item item in mData.Items)
            {
                if (item.Title.Contains(s))
                {
                    mItems.Add(item);
                }
                i++;
                prg.Value = i;
            }
        }

        private void listItems_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Item item = listItems.SelectedItem as Item;
            if (item != null)
            {
                menuItemFile.Visibility = item.FileName == string.Empty ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void menuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            Item item = listItems.SelectedItem as Item;
            if (item != null)
            {
                NavigationService.Navigate(new Pages.EditItem(item));
            }
        }

        private void menuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            Item item = listItems.SelectedItem as Item;
            if (item != null)
            {
                mData.DelItem(item);
                if (mItems != mData.Items)
                {
                    mItems.Remove(item);
                }
            }
        }

        private void menuItemOpenFile_Click(object sender, RoutedEventArgs e)
        {
            Item item = listItems.SelectedItem as Item;
            if (item != null && item.FileName != string.Empty)
            {
                try
                {
                    System.Diagnostics.Process.Start(SomeConst.WORKDIR + SomeConst.FILEDIR + item.FileName);
                }
                catch (Exception)
                {
                    System.Windows.Forms.MessageBox.Show("File doesn't exist.");
                }
            }
        }

        private void menuItemCopyTo_Click(object sender, RoutedEventArgs e)
        {
            Item item = listItems.SelectedItem as Item;
            if (item != null && item.FileName != string.Empty)
            {
                SaveFileDialog f = new SaveFileDialog();
                string ext = System.IO.Path.GetExtension(item.FileName);
                f.FileName = item.Title + ext;
                f.Filter = "|*" + ext + "|All File|*.*";
                if (f.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.Copy(SomeConst.FILEDIR + item.FileName, f.FileName, true);
                    }
                    catch (Exception)
                    {
                        System.Windows.Forms.MessageBox.Show("Maybe file doesn't exist or some privilege issue.");
                    }
                }
            }
        }

        private void menuItemShowFile_Click(object sender, RoutedEventArgs e)
        {
            Item item = listItems.SelectedItem as Item;
            if (item != null && item.FileName != string.Empty)
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("explorer.exe");
                psi.Arguments = "/select, " + SomeConst.WORKDIR + SomeConst.FILEDIR + item.FileName;
                System.Diagnostics.Process.Start(psi);
            }
        }
    }
}
