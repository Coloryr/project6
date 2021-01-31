using System.ComponentModel;
using System.Windows;

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
            Addr.Text = App.Config.Url;
            User.Text = App.Config.User;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void Start()
        {
            App.HttpUtils = new WebSocketUtils();
            if (!App.HttpUtils.isok)
            {
                App.HttpUtils = null;
                return;
            }
            var res = App.HttpUtils.Start();
            if (!res)
            {
                MessageBox.Show("服务器连接失败");
                App.HttpUtils.Stop();
                App.HttpUtils = null;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Config.Url = Addr.Text;
            App.Config.User = User.Text;
            if (App.HttpUtils == null)
            {
                App.HttpUtils = new WebSocketUtils();
                if (!App.HttpUtils.isok)
                {
                    MessageBox.Show("地址错误");
                    App.HttpUtils = null;
                    return;
                }
                bool res = App.HttpUtils.Start();
                if (!res)
                {
                    MessageBox.Show("地址错误");
                    App.HttpUtils = null;
                    return;
                }
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
                MessageBox.Show("登录失败");
            }
        }
    }
}
