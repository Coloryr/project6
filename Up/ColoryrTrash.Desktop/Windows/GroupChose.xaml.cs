﻿using System;
using System.Collections.Generic;
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
    /// GroupChose.xaml 的交互逻辑
    /// </summary>
    public partial class GroupChose : Window
    {
        public string UUID { get; set; }
        private string group;
        public GroupChose(List<string> groups, string UUID, string old)
        {
            InitializeComponent();
            this.UUID = UUID;
            var list = new List<string>(groups);
            list.Remove(old);
            GroupList.ItemsSource = list;
            DataContext = this;
        }
        public string Set()
        {
            ShowDialog();
            return group;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            group = GroupList.SelectedItem as string;
            Close();
        }

        private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            group = GroupList.SelectedItem as string;
        }
    }
}
