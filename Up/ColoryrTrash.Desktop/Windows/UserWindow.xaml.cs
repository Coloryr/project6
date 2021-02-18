using System.Windows;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// UserWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserWindow : Window
    {
        public SearchObj obj { get; set; }
        public UserWindow(SearchObj obj = null)
        {
            InitializeComponent();
            if (obj == null)
            {
                this.obj = new();
            }
            else
            {
                User.IsReadOnly = true;
                this.obj = obj;
            }
            DataContext = this.obj;
        }
        public SearchObj Set()
        {
            ShowDialog();
            obj.Nick = Pass.Password;
            return obj;
        }
    }
}
