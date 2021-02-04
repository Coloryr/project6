using Lib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ColoryrTrash.Desktop
{
    /// <summary>
    /// ListWindows.xaml 的交互逻辑
    /// </summary>
    public partial class ListWindows : Window
    {
        private DataSaveObj obj;
        public ListWindows()
        {
            InitializeComponent();
        }

        private void GetList()
        {
            App.HttpUtils.GetGroups();
        }

        private void GetInfo(string group)
        {
            App.HttpUtils.GetGroupInfo(group);
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
                GroupList.Items.Clear();
                foreach (var item in list)
                {
                    GroupList.Items.Add(item);
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
            {
                return;
            }
            string item = GroupList.SelectedItem as string;
            GetInfo(item);
        }

        private void Re_Click(object sender, RoutedEventArgs e)
        {
            Clear_Click(sender, e);
            GetList();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            GroupList.Items.Clear();
            List.Items.Clear();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
