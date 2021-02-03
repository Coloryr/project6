using CefSharp;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Map.Load($"http://127.0.0.1:{App.Config.HttpPort}/");
            Task.Run(() =>
            {
                Thread.Sleep(2000);
                Map.EvaluateScriptAsync("set(116.404, 39.925,'测试','一个测试')");
            });
        }
    }
}
