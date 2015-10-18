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

namespace MizTagger.Pages
{
    /// <summary>
    /// EditItem.xaml 的交互逻辑
    /// </summary>
    public partial class EditItem : Page
    {
        private ObservableCollection<Tag> mTags;
        private string mPath;
        private Item mItem;
        private ShitAllInOne mData;

        public EditItem()
        {
            mData = (App.Current as App).Data;
            mTags = new ObservableCollection<MizTagger.Tag>();
            mItem = null;
            mPath = string.Empty;
            InitializeComponent();
            listTags.ItemsSource = mTags;
            comboTypes.ItemsSource = mData.Types;
        }

        public EditItem(Item item)
            : this()
        {
            mTags = new ObservableCollection<MizTagger.Tag>(item.Tags);
            mItem = item;
            listTags.ItemsSource = mTags;

            textDesc.Text = item.Description;
            textTitle.Text = item.Title;
            textPath.Text = item.FileName;
        }

        private void listTags_menuOpening(object sender, ContextMenuEventArgs e)
        {
            if (listTags.SelectedIndex == -1)
            {
                e.Handled = true;
            }
        }

        private void btnPickFile_Click(object sender, RoutedEventArgs e)
        {
            if (mItem == null)
            {
                OpenFileDialog f = new OpenFileDialog();
                if (f.ShowDialog() == DialogResult.OK)
                {
                    mPath = f.FileName;
                    textPath.Text = mPath;
                    textTitle.Text = System.IO.Path.GetFileNameWithoutExtension(mPath);
                }
            }
        }

        private void menuItemDelTag_Click(object sender, RoutedEventArgs e)
        {
            MizTagger.Tag t = listTags.SelectedItem as MizTagger.Tag;
            if (t != null)
            {
                mTags.Remove(t);
            }
        }

        private void clkOK(object sender, RoutedEventArgs e)
        {
            Item i = new Item(mData.Tags)
            {
                Title = textTitle.Text.Trim(),
                Description = textDesc.Text.Trim(),
                FileName = mPath == string.Empty ? string.Empty : Guid.NewGuid().ToString() + System.IO.Path.GetExtension(mPath),
                Tagum = mTags.GetTagum()
            };
            i.ClearChangeStatus();//for newly added
            if (mItem == null)
            {
                mData.AddItem(i, mPath);
            }
            else
            {
                mData.SaveItem(mItem, i);
            }
            NavigationService.GoBack();
        }

        private void comboTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cb = sender as System.Windows.Controls.ComboBox;
            if (cb != null)
            {
                comboTypedTags.ItemsSource = (cb.SelectedItem as TagType).Tags;
            }
        }

        private void clkAdd(object sender, RoutedEventArgs e)
        {
            if (comboTypedTags.SelectedIndex != -1)
            {
                MizTagger.Tag tag = comboTypedTags.SelectedItem as Tag;
                mTags.Add(tag);
            }
        }
    }
}
