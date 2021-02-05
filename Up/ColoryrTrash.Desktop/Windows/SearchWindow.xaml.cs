using System.Windows;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// SearchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchObj obj { get; set; }
        public SearchWindow()
        {
            InitializeComponent();
            obj = new SearchObj();
            DataContext = obj;
        }

        public SearchObj Set()
        {
            ShowDialog();
            return obj;
        }
    }
}
