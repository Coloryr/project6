using Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// UserListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserListWindow : Window
    {
        private UserDataSaveObj obj;
        private List<string> list;
        public UserListWindow()
        {
            InitializeComponent();
        }

        public void GetList()
        {
            App.MqttUtils.GetUserGroups();
        }

        private void GetInfo(string group)
        {
            App.MqttUtils.GetUserGroupInfo(group);
        }
        private void Window_Loaded(object sender, EventArgs e)
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

        public void SetList(List<string> list)
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

        public void SetInfo(UserDataSaveObj list)
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

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var res = new UserWindow().Set();
            if (string.IsNullOrWhiteSpace(res.UUID) || string.IsNullOrWhiteSpace(res.Nick))
            {
                return;
            }
            App.MqttUtils.AddUser(res.UUID, Tools.EnBase64(res.Nick));
        }

        private void MoveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as UserSaveObj;
            var res = new GroupChose(list, obj.ID, GroupList.SelectedItem as string).Set();
            if (res != GroupList.SelectedItem as string)
            {
                App.MqttUtils.MoveUserGroup(obj.ID, res);
            }
        }

        private void SetPass_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as UserSaveObj;
            var res = new UserWindow(new SearchObj
            {
                UUID = obj.ID
            }).Set();
            if (string.IsNullOrWhiteSpace(res.Nick))
                return;
            App.MqttUtils.SetUserPass(obj.ID, Tools.EnBase64(res.Nick));
        }
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as UserSaveObj;
            var res = new ChoseWindow("删除账户", $"是否要删除账户[{obj.ID}]").Set();
            if (res)
            {
                App.MqttUtils.DeleteUser(obj.ID);
            }
        }
        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var res = new InputWindow("创建账户组").Set();
            if (!string.IsNullOrWhiteSpace(res))
                App.MqttUtils.AddUserGroup(res);
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var res = new InputWindow("搜索账户").Set();
            if (string.IsNullOrWhiteSpace(res))
                return;
            foreach (var item in obj.List)
            {
                if (item.Key.Contains(res))
                {
                    List.SelectedItem = item.Value;
                    List.ScrollIntoView(item.Value);
                    return;
                }
            }
        }
        private void Re_Click(object sender, RoutedEventArgs e)
        {
            GroupList.Items.Clear();
            List.Items.Clear();
            GetList();
        }
        private void RenameGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem == null)
                return;
            var res = new InputWindow("设置组名字", GroupList.SelectedItem as string).Set();
            if (res == GroupList.SelectedItem as string)
                return;
            if (!string.IsNullOrWhiteSpace(res))
                App.MqttUtils.RenameUserGroup(GroupList.SelectedItem as string, res);
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
        public void MoveGroup(string id, string group)
        {
            Dispatcher.Invoke(() =>
            {
                if (GroupList.SelectedItem as string != group)
                {
                    foreach (var item in List.Items)
                    {
                        var obj = item as UserSaveObj;
                        if (obj.ID == id)
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
        private void Bind_Click(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem == null)
                return;
            if (App.BindWindow_ == null)
            {
                new BindWindow(GroupList.SelectedItem as string).Show();
            }
            else
            {
                App.BindWindow_.Activate();
            }
        }

        public void AddGroup(string data)
        {
            Dispatcher.Invoke(() =>
            {
                list.Add(data);
                GroupList.Items.Add(data);
            });
        }

        public void AddUser(string data)
        {
            Dispatcher.Invoke(() =>
            {
                if (GroupList.SelectedItem as string == data)
                {
                    GetInfo(data);
                }
            });
        }
        public void RemoveUser(string id)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var item in List.Items)
                {
                    var temp = item as UserSaveObj;
                    if (temp.ID == id)
                    {
                        List.Items.Remove(item);
                        return;
                    }
                }
            });
        }
    }
}
