using System;
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
                this.obj = obj;
            }
            DataContext = obj;
        }
        public SearchObj Set()
        {
            ShowDialog();
            obj.Nick = Pass.Password;
            return obj;
        }
    }
}
