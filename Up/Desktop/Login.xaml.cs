using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Desktop
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
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
            var res = await App.HttpUtils.Start();
            if (!res)
            {
                App.ShowB("登录", "服务器连接失败");
            }
            else
            {
                App.HttpUtils.CheckLogin(App.Config.User, App.Config.Token);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            App.LoginWindows = null;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Config.IP = IP.Text;
            App.Config.User = User.Text;
            bool res = await App.HttpUtils.Start();
            if (!res)
            {
                App.ShowB("登录", "服务器连接失败");
                return;
            }
            App.HttpUtils.Login(User.Text, Pass.Password);
        }

        private void Token_Checked(object sender, RoutedEventArgs e)
        {
            App.Config.AutoLogin = Token.IsChecked == true;
        }

        public void LoginRes(bool res)
        {
            if (res)
            {
                Close();
            }
            else
            {
                App.ShowB("登录", "登录失败");
            }
        }
    }
}
