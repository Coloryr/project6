using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ColoryrTrash.Desktop
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();

            App.LoginWindows = this;

            Token.IsChecked = App.Config.AutoLogin;
            DataContext = App.Config;

            BitmapSource m = (BitmapSource)Icon;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(m.PixelWidth, m.PixelHeight,
                PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
            new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            IntPtr iconHandle = bmp.GetHicon();
            System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(iconHandle);

            App.SetIcon(icon);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private async void Start()
        {
            if (!App.HttpUtils.Check())
            {
                var res = await App.HttpUtils.Start();
                if (!res)
                {
                    App.ShowB("登录", "服务器连接失败");
                    return;
                }
            }
            App.HttpUtils.CheckLogin(App.Config.Token);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Config.IP = IP.Text;
            App.Config.User = User.Text;
            if (string.IsNullOrWhiteSpace(Pass.Password))
            {
                App.ShowB("登录", "请输入密码");
                return;
            }
            bool res = await App.HttpUtils.Start();
            if (!res)
            {
                App.ShowB("登录", "服务器连接失败");
                return;
            }
            App.HttpUtils.Login(Pass.Password);
        }

        private void Token_Checked(object sender, RoutedEventArgs e)
        {
            App.Config.AutoLogin = Token.IsChecked == true;
        }

        public void LoginClose()
        {
            Dispatcher.Invoke(() => Close());
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (App.IsLogin)
            {
                return;
            }
            var res = new ChoseWindow("你还没有登录", "关闭后会直接关闭软件，你确定吗？").Set();
            if (res)
            {
                App.Stop();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
