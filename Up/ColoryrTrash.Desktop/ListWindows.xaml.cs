using System.Windows;

namespace ColoryrTrash.Desktop
{
    /// <summary>
    /// ListWindows.xaml 的交互逻辑
    /// </summary>
    public partial class ListWindows : Window
    {
        public ListWindows()
        {
            InitializeComponent();
        }

        private void GetList()
        {
            App.HttpUtils.GetGroups();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetList();
        }
    }
}
