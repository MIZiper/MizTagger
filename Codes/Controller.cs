using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;

namespace MizTagger
{
    public class ShitAllInOne
    {
        private List<Tag> _tags;
        public List<Tag> Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = new List<Tag>();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "select * from tags";
                        using (SQLiteDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                _tags.Add(new Tag() { Typeguid = rd.GetInt32(2), Tagname = rd.GetString(1), Tagoffset = rd.GetInt32(0) });
                            }
                        }
                    }
                }
                return _tags;
            }
        }
        private ObservableCollection<TagType> _types;
        public ObservableCollection<TagType> Types
        {
            get
            {
                if (_types == null)
                {
                    _types = new ObservableCollection<TagType>();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "select * from types";
                        using (SQLiteDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                _types.Add(new TagType(Tags) { Typeguid = rd.GetInt32(0), Typename = rd.GetString(1) });
                            }
                        }
                    }
                }
                return _types;
            }
        }
        private List<User> _users;
        public List<User> Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new List<User>();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "select * from users";
                        using (SQLiteDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                _users.Add(new User() { Email = rd.GetString(2), Name = rd.GetString(1), Privilege = (UserPrivilege)rd.GetInt32(3) });
                            }
                        }
                    }
                }
                return _users;
            }
        }
        private ObservableCollection<Item> _items;
        public ObservableCollection<Item> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<Item>();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "select * from items";
                        using (SQLiteDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                _items.Add(new Item(Tags) { Id = rd.GetInt32(0), Title = rd.GetString(1), Description = rd.GetString(2), FileName = rd.GetString(3), Tagum = (byte[])rd["tagum"] });
                            }
                        }
                    }
                }
                return _items;
            }
        }

        private SQLiteConnection conn;
        public static string ConnectionString = "Data Source=Tagger.s3db";

        public ShitAllInOne(string connStr)
        {
            if (!Directory.Exists(SomeConst.FILEDIR))
            {
                Directory.CreateDirectory(SomeConst.FILEDIR);
            }
            conn = new SQLiteConnection(connStr);
            conn.Open();
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "create table if not exists users(id integer primary key autoincrement, name text, email text, privilege integer);" +
                    "create table if not exists types(id integer primary key autoincrement, name text);" +
                    "create table if not exists tags(id integer primary key autoincrement, name text, type integer);" +
                    "create table if not exists items(id integer primary key autoincrement, title text, description text, file text, tagum blob);";
                cmd.ExecuteNonQuery();
            }
        }

        public void AddItem(Item item, string path)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "insert into items(title,description,file,tagum) values(@Title,@Description,@File,@Tagum);";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@Title", item.Title);
                cmd.Parameters.AddWithValue("@Description", item.Description);
                cmd.Parameters.AddWithValue("@File", item.FileName);
                cmd.Parameters.Add("@Tagum", System.Data.DbType.Binary).Value = item.Tagum;
                cmd.ExecuteNonQuery();
                item.Id = (int)conn.LastInsertRowId;
                Items.Add(item);
            }
            if (item.FileName!=string.Empty)
            {
                File.Copy(path, SomeConst.FILEDIR + item.FileName);
            }
        }
        public void SaveItem(Item refItem, Item newItem)
        {
            refItem.Title = newItem.Title;
            refItem.Description = newItem.Description;
            refItem.Tagum = newItem.Tagum;
            if (refItem.IsChanged())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    List<string> sList = new List<string>();

                    if (refItem.IsTitleChanged)
                    {
                        sList.Add("title=@Title");
                    }
                    if (refItem.IsDescriptionChanged)
                    {
                        sList.Add("description=@Description");
                    }
                    if (refItem.IsTagumChanged)
                    {
                        sList.Add("tagum=@Tagum");
                    }

                    cmd.CommandText = "update items set " + string.Join(",", sList) + " where id=@Id";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@Id", refItem.Id);
                    cmd.Parameters.AddWithValue("@Title", refItem.Title);
                    cmd.Parameters.AddWithValue("@Description", refItem.Description);
                    cmd.Parameters.Add("@Tagum", System.Data.DbType.Binary).Value = refItem.Tagum;
                    cmd.ExecuteNonQuery();

                    refItem.ClearChangeStatus();
                }
            }
        }
        public void DelItem(Item item)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "delete from items where id=@Id;";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.ExecuteNonQuery();

                Items.Remove(item);
            }
            if (item.FileName!=string.Empty)
            {
                try
                {
                File.Delete(SomeConst.FILEDIR + item.FileName);
                }
                catch (System.Exception)
                {
                    System.Windows.Forms.MessageBox.Show("Delete file failed.");
                }
            }
        }
        public void AddTag2Type(string tagname, TagType tagtype)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "insert into tags(name,type) values(@Name,@Type);";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@Name", tagname);
                cmd.Parameters.AddWithValue("@Type", tagtype.Typeguid);
                cmd.ExecuteNonQuery();

                Tag t = new Tag() { Tagname = tagname, Tagoffset = (int)conn.LastInsertRowId, Typeguid = tagtype.Typeguid };
                Tags.Add(t);
                tagtype.Tags.Add(t);
            }
        }
        public void AddTagType(string typename)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                cmd.CommandText = "insert into types(name) values(@Name);";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@Name", typename);
                cmd.ExecuteNonQuery();

                TagType t = new TagType(Tags) { Typeguid = (int)conn.LastInsertRowId, Typename = typename };
                Types.Add(t);
            }
        }
    }

}