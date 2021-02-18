using System;
using System.Collections.Generic;
using System.Windows;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// BindWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BindWindow : Window
    {
        private List<string> list = new();
        private List<string> group = new();
        private string name;
        public BindWindow(string name)
        {
            InitializeComponent();
            App.BindWindow_ = this;
            this.name = name;
            User.Content = name;
            App.MqttUtils.GetTrashGroups();
            App.MqttUtils.GetGroupBind(name);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (GroupList.SelectedItem == null)
                return;
            group.Remove(GroupList.SelectedItem as string);
            GroupList.Items.Remove(GroupList.SelectedItem);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.BindWindow_ = null;
        }

        private void ReList()
        {
            Dispatcher.Invoke(() =>
            {
                List.Items.Clear();
                foreach (var item in list)
                {
                    List.Items.Add(item);
                }
            });
        }
        public void SetList(List<string> list)
        {
            this.list = list;
            ReList();
        }

        private void ReGroup()
        {
            Dispatcher.Invoke(() =>
            {
                GroupList.Items.Clear();
                foreach (var item in group)
                {
                    GroupList.Items.Add(item);
                }
            });
        }

        public void SetGroup(List<string> group)
        {
            this.group = group;
            ReGroup();
        }

        private void Re_Click(object sender, RoutedEventArgs e)
        {
            App.MqttUtils.GetTrashGroups();
            App.MqttUtils.GetGroupBind(name);
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            group.Add(List.SelectedItem as string);
            ReGroup();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            App.MqttUtils.SetGroupBind(name, group);
            Close();
        }
    }
}
