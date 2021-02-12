using CefSharp;
using System.ComponentModel;
using System.Windows;

namespace ColoryrTrash.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        private bool IsLoad;
        public MapWindow()
        {
            InitializeComponent();
            App.MainWindow_ = this;
            Map.IsBrowserInitializedChanged += Map_IsBrowserInitializedChanged;
        }

        private void Map_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoad && Map.IsBrowserInitialized)
            {
                Map.Load($"http://127.0.0.1:{App.Config.HttpPort}/");
                IsLoad = true;
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

        public async void AddPoint(double x, double y, string title, string text)
        {
            double X = x / 1000;
            double Y = y / 1000;
            string temp = $"addpoint({X}, {Y},'{title}','{text}')";
            var res = await Map.EvaluateScriptAsync(temp);
            App.Log(res.Message + "|" + res.Result + "|" + temp);
        }

        public void ClearPoint()
        {
            Map.EvaluateScriptAsync("clearpoint()");
        }
    }
}
