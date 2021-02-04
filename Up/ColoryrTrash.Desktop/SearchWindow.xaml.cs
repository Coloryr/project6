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

namespace ColoryrTrash.Desktop
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
