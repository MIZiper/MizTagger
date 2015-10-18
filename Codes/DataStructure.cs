using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data.SQLite;

namespace MizTagger
{
    [Flags]
    public enum UserPrivilege
    {
        None,
        Edit,
        Grant
    }
    public enum TagLogic
    {
        ContainOne,
        ContainAll,
        NotAll,
        NotAny
    }

    public static class SomeConst
    {
        public static int DEFAULTBYTES = 4;
        public static string WORKDIR = Environment.CurrentDirectory+"\\";
        public static string FILEDIR = "Files\\";
    }

    public class Filter
    {
        public Tag Tag { get; set; }
        public TagLogic Logic { get; set; }
    }
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public UserPrivilege Privilege { get; set; }
    }
    public class TagType
    {
        private List<Tag> mSrcTags;
        public TagType(List<Tag> src)
        {
            mSrcTags = src;
        }

        public string Typename { get; set; }
        public int Typeguid { get; set; }

        private ObservableCollection<Tag> _tags;
        public ObservableCollection<Tag> Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = new ObservableCollection<Tag>(mSrcTags.Where(t => t.Typeguid == this.Typeguid));
                }
                return _tags;
            }
        }
    }
    public class Item : INotifyPropertyChanged
    {
        private List<Tag> mSrcTags;
        public Item(List<Tag> src)
        {
            mSrcTags = src;
            mChangeBit = 0;
            _tagum = new byte[SomeConst.DEFAULTBYTES];
        }

        public int Id { get; set; }
        private int mChangeBit;
        public bool IsTitleChanged
        {
            get
            {
                return (mChangeBit & 1) > 0;
            }
            set
            {
                mChangeBit |= 1;
            }
        }
        public bool IsDescriptionChanged
        {
            get
            {
                return (mChangeBit & 2) > 0;
            }
            set
            {
                mChangeBit |= 2;
            }
        }
        public bool IsTagumChanged
        {
            get
            {
                return (mChangeBit & 4) > 0;
            }
            set
            {
                mChangeBit |= 4;
            }
        }
        public void ClearChangeStatus() {
            mChangeBit = 0;
        }
        public bool IsChanged()
        {
            return mChangeBit > 0;
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    IsTitleChanged = true;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        private string _desc;
        public string Description
        {
            get { return _desc; }
            set
            {
                if (_desc != value)
                {
                    _desc = value;
                    IsDescriptionChanged = true;
                    NotifyPropertyChanged("Description");
                }
            }
        }
        public string FileName { get; set; }
        private byte[] _tagum;
        public byte[] Tagum
        {
            get { return _tagum; }
            set
            {
                if (!_tagum.Equal(value))
                {
                    _tagum = value;
                    IsTagumChanged = true;
                    NotifyPropertyChanged("Tags");
                    /* I was trapped for two nights, because I write NotifyPropertyChanged before assigning the value
                     * resulting that the view won't update when I EditItem, save and navigate back to SearchResult
                     * I tried to update the value in page_loaded/navigated... all kinds of test 'cause I thought the view
                     * hasn't generated before I modify the property of class, so no update view
                     */
                }
            }
        }
        public ObservableCollection<Tag> Tags
        {
            get
            {
                return new ObservableCollection<Tag>(mSrcTags.Where(t => t.InTagum(_tagum)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string prop)
        {
            PropertyChangedEventHandler h = this.PropertyChanged;
            if (h != null)
            {
                h(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
    public class Tag
    {
        public string Tagname { get; set; }
        public int Tagoffset { get; set; }
        public int Typeguid { get; set; }

        public bool InTagum(byte[] tagum)
        {
            int index, pos;
            index = Tagoffset / 8;
            pos = Tagoffset % 8;
            if (pos == 0)
            {
                pos = 8; index -= 1;
            }
            byte b = (byte)(0x01 << (pos - 1));
            if (tagum.Length > index)
            {
                return (tagum[index] & b) == b;
            }
            return false;
        }
    }

    public static class ExtensionTool
    {
        public static byte[] GetTagum(this IList<Tag> tags)
        {
            byte[] bs = new byte[SomeConst.DEFAULTBYTES];
            foreach (Tag t in tags)
            {
                bs = bs.OrOffset(t.Tagoffset);
            }
            return bs;
        }
        public static Dictionary<TagLogic, byte[]> GetLogic(this IList<Filter> filters)
        {
            Dictionary<TagLogic, byte[]> r = new Dictionary<TagLogic, byte[]>();
            foreach (Filter f in filters)
            {
                if (!r.ContainsKey(f.Logic))
                {
                    r.Add(f.Logic, new byte[SomeConst.DEFAULTBYTES]);
                }
                r[f.Logic] = r[f.Logic].OrOffset(f.Tag.Tagoffset);
            }
            return r;
        }
        public static byte[] ExpandTo(this byte[] bs, int target)
        {
            if (bs.Length < target)
            {
                byte[] r;
                r = new byte[target];
                Buffer.BlockCopy(bs, 0, r, 0, bs.Length);
                return r;
            }
            return bs;
        }
        public static bool Equal(this byte[] bs, byte[] bf)
        {
            bool flag = false;
            if (bs.Length == bf.Length)
            {
                flag = true;
                for (int i = 0, l = bf.Length; i < l; i++)
                {
                    if (bf[i] != bs[i])
                    {
                        flag = false;
                        break;
                    }
                }
            }
            //in this case (0x00 0x??)!=(0x??)
            return flag;
        }
        public static byte[] And(this byte[] bs, byte[] bf)
        {
            byte[] r = new byte[bs.Length];
            int l = bs.Length > bf.Length ? bf.Length : bs.Length;
            for (int i = 0; i < l; i++)
            {
                r[i] = bs[i];
                r[i] &= bf[i];
            }
            return r;
        }
        public static byte[] OrOffset(this byte[] bs, int offset)
        {
            int index = offset / 8,
                pos = offset % 8;
            if (pos == 0)
            {
                pos = 8; index -= 1;
            }
            bs = bs.ExpandTo(index + 1);
            bs[index] |= (byte)(0x01 << (pos - 1));
            return bs;
        }
        public static bool MatchLogic(this byte[] bs, Dictionary<TagLogic, byte[]> logic)
        {
            bool flag = true;
            foreach (var l in logic)
            {
                byte[] copy = l.Value.And(bs);
                switch (l.Key)
                {
                    case TagLogic.ContainOne:
                        flag = !copy.Equal(new byte[l.Value.Length]);
                        break;
                    case TagLogic.ContainAll:
                        flag = copy.Equal(l.Value);
                        break;
                    case TagLogic.NotAll:
                        flag = !copy.Equal(l.Value);
                        break;
                    case TagLogic.NotAny:
                        flag = copy.Equal(new byte[l.Value.Length]);
                        break;
                    default:
                        flag = false;
                        break;
                }
                if (!flag)
                {
                    break;
                }
            }
            return flag;
        }
    }
}