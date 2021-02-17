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
    public partial class ListWindow : Window
    {
        private TrashDataSaveObj obj;
        private List<string> list;
        public ListWindow()
        {
            InitializeComponent();
        }

        public void GetList()
        {
            App.MqttUtils.GetTrashGroups();
        }

        private void GetInfo(string group)
        {
            App.MqttUtils.GetTrashGroupInfo(group);
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

        internal void SetInfo(TrashDataSaveObj list)
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

        private string GetString(TrashSaveObj item)
        {
            return $"别称:{item.Nick}<br/>坐标:{item.X}, {item.Y}" +
                $"<br/>容量:{item.Capacity}<br/>是否打开:{item.Open}" +
                $"<br/>状态:{item.State}<br/>上线时间:{item.Time}" +
                $"<br/>SIM卡号:{item.SIM}<br/>电量:{item.Battery}";
        }

        private void Track_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as TrashSaveObj;
            if (ConvertGPS.outOfChina(obj.Y, obj.X))
            {
                App.ShowA("追踪", "无法追踪");
                return;
            }
            else
            {
                App.MainWindow_.ClearPoint();
                App.MainWindow_.AddPoint(obj.X, obj.Y, $"垃圾桶:{obj.UUID}", GetString(obj));
                App.MainWindow_.Turn(obj.X, obj.Y);
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var res = new InputWindow("创建组").Set();
            if (!string.IsNullOrWhiteSpace(res))
                App.MqttUtils.AddTrashGroup(res);
        }

        private void RenameGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem == null)
                return;
            var res = new InputWindow("设置组名字", GroupList.SelectedItem as string).Set();
            if (!string.IsNullOrWhiteSpace(res))
                App.MqttUtils.RenameTrashGroup(GroupList.SelectedItem as string, res);
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
            var obj = List.SelectedItem as TrashSaveObj;
            var res = new GroupChose(list, obj.UUID, GroupList.SelectedItem as string).Set();
            if (res != GroupList.SelectedItem as string)
            {
                App.MqttUtils.MoveTrashGroup(obj.UUID, res);
            }
        }

        private void SetNick_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as TrashSaveObj;
            var res = new InputWindow("设置备注", obj.Nick).Set();
            if (string.IsNullOrWhiteSpace(res))
                return;
            App.MqttUtils.SetTrashNick(obj.UUID, res);
        }

        public void MoveGroup(string uuid, string group)
        {
            Dispatcher.Invoke(() =>
            {
                if (GroupList.SelectedItem as string != group)
                {
                    foreach (var item in List.Items)
                    {
                        var obj = item as TrashSaveObj;
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

        public void Updata(string group, TrashSaveObj item)
        {
            Dispatcher.Invoke(() =>
            {
                if (GroupList.SelectedItem as string == group)
                {
                    foreach (var item1 in List.Items)
                    {
                        var obj = item1 as TrashSaveObj;
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

        private void Flash_Click(object sender, RoutedEventArgs e)
        {
            if (App.MakeHardway_ == null)
            {
                App.MakeHardway_ = new MakeHardway();
                App.MakeHardway_.Show();
            }
            else
            {
                App.MakeHardway_.Activate();
            }
        }

        private void ReadNow_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as TrashSaveObj;
            App.MqttUtils.ReadNow(obj.UUID);
        }
    }
}
