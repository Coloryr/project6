using Lib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// ListWindows.xaml 的交互逻辑
    /// </summary>
    public partial class ListWindows : Window
    {
        private DataSaveObj obj;
        private List<string> list;
        public ListWindows()
        {
            InitializeComponent();
        }

        public void GetList()
        {
            App.MqttUtils.GetGroups();
        }

        private void GetInfo(string group)
        {
            App.MqttUtils.GetGroupInfo(group);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var res = new ChoseWindow("退出", "关闭后会直接关闭软件，你确定吗？").Set();
            if (res)
            {
                App.Stop();
            }
            else
            {
                e.Cancel = true;
            }
        }

        internal void SetList(List<string> list)
        {
            Dispatcher.Invoke(() =>
            {
                this.list = list;
                GroupList.Items.Clear();
                foreach (var item in list)
                {
                    GroupList.Items.Add(item);
                }
                if (GroupList.Items.Contains("空的组"))
                {
                    GroupList.SelectedItem = "空的组";
                }
            });
        }

        internal void SetInfo(DataSaveObj list)
        {
            obj = list;
            Dispatcher.Invoke(() =>
            {
                List.Items.Clear();
                foreach (var item in obj.List)
                {
                    List.Items.Add(item.Value);
                }
            });
        }

        private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupList.SelectedItem == null)
                return;
            string item = GroupList.SelectedItem as string;
            GetInfo(item);
        }

        private void Re_Click(object sender, RoutedEventArgs e)
        {
            GroupList.Items.Clear();
            List.Items.Clear();
            GetList();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var res = new SearchWindow().Set();
            if (string.IsNullOrWhiteSpace(res.UUID) && string.IsNullOrWhiteSpace(res.Nick))
                return;
            foreach (var item in obj.List)
            {
                if (item.Key.Contains(res.UUID))
                {
                    List.SelectedItem = item.Value;
                    List.ScrollIntoView(item.Value);
                    return;
                }
                if (item.Value.Nick.Contains(res.Nick))
                {
                    List.SelectedItem = item.Value;
                    List.ScrollIntoView(item.Value);
                    return;
                }
            }
        }

        private string GetString(ItemSaveObj item)
        {
            return $"垃圾桶别称:{item.Nick}<br/>垃圾桶坐标:{(double)item.X / 1000}, {(double)item.Y / 1000}" +
                $"<br/>垃圾桶容量:{item.Capacity}<br/>垃圾桶是否打开{item.Open}" +
                $"<br/>垃圾桶状态:{item.State}<br/>垃圾桶上线时间:{item.Time}";
        }

        private void Track_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as ItemSaveObj;
            if (obj.X == 0 && obj.Y == 0)
            {
                App.ShowA("追踪", "无法追踪");
                return;
            }
            else
            {
                App.MainWindow_.ClearPoint();
                App.MainWindow_.AddPoint(obj.X, obj.Y, obj.UUID, GetString(obj));
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var res = new InputWindow("创建组").Set();
            if (!string.IsNullOrWhiteSpace(res))
                App.MqttUtils.AddGroup(res);
        }

        private void RenameGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem == null)
                return;
            var res = new InputWindow("设置组名字", GroupList.SelectedItem as string).Set();
            if (!string.IsNullOrWhiteSpace(res))
                App.MqttUtils.RenameGroup(GroupList.SelectedItem as string, res);
        }

        public void AddGroup(string data)
        {
            Dispatcher.Invoke(() =>
            {
                list.Add(data);
                GroupList.Items.Add(data);
            });
        }

        public void RenameGroup(string old, string group)
        {
            Dispatcher.Invoke(() =>
            {
                if (GroupList.Items.Contains(old))
                {
                    list.Remove(old);
                    list.Add(group);
                    bool chose = false;
                    if (GroupList.SelectedItem as string == old)
                    {
                        chose = true;
                    }
                    GroupList.Items.Add(group);
                    if (chose)
                    {
                        GroupList.SelectedItem = group;
                    }
                    GroupList.Items.Remove(old);
                }
            });
        }

        private void MoveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as ItemSaveObj;
            var res = new GroupChose(list, obj.UUID, GroupList.SelectedItem as string).Set();
            if (res != GroupList.SelectedItem as string)
            {
                App.MqttUtils.MoveGroup(obj.UUID, res);
            }
        }

        private void SetNick_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as ItemSaveObj;
            var res = new InputWindow("设置备注", obj.Nick).Set();
            if (string.IsNullOrWhiteSpace(res))
                return;
            App.MqttUtils.SetNick(obj.UUID, res);
        }

        public void MoveGroup(string uuid, string group)
        {
            Dispatcher.Invoke(() =>
            {
                if (GroupList.SelectedItem as string != group)
                {
                    foreach (var item in List.Items)
                    {
                        var obj = item as ItemSaveObj;
                        if (obj.UUID == uuid)
                        {
                            List.Items.Remove(obj);
                            return;
                        }
                    }
                }
                else
                {
                    GetInfo(group);
                }
            });
        }

        public void Updata(string group, ItemSaveObj item)
        {
            Dispatcher.Invoke(() =>
            {
                if (GroupList.SelectedItem as string == group)
                {
                    foreach (var item1 in List.Items)
                    {
                        var obj = item1 as ItemSaveObj;
                        if (obj.UUID == item.UUID)
                        {
                            List.Items.Remove(item1);
                            List.Items.Add(item);
                            List.SelectedItem = item;
                            List.ScrollIntoView(item);
                            return;
                        }
                    }
                }
            });
        }
    }
}
