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

        }

        private void SetPass_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {

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
            if (!string.IsNullOrWhiteSpace(res))
                App.MqttUtils.RenameUserGroup(GroupList.SelectedItem as string, res);
        }
        private void Bind_Click(object sender, RoutedEventArgs e)
        {
            GroupList.Items.Clear();
            List.Items.Clear();
            GetList();
        }
    }
}
