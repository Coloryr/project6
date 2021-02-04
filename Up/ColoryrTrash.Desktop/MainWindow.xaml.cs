using CefSharp;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ColoryrTrash.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsLoad;
        public MainWindow()
        {
            InitializeComponent();
            Map.IsBrowserInitializedChanged += Map_IsBrowserInitializedChanged;
        }

        private void Map_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoad && Map.IsBrowserInitialized)
            {
                Map.Load($"http://127.0.0.1:{App.Config.HttpPort}/");
                IsLoad = true;
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    Map.EvaluateScriptAsync("addpoint(116.404, 39.925,'测试','一个测试')");
                });
                return;
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
