using System.Windows;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// InputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputWindow : Window
    {
        public string Data { get; set; }
        public InputWindow(string title, string data = "")
        {
            InitializeComponent();
            Title = title;
            Data = data;
            DataContext = this;
        }

        public string Set()
        {
            ShowDialog();
            return Data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Data = Text.Text;
            Close();
        }
    }
}
