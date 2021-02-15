using System;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private bool IsLoging;
        public LoginPage()
        {
            InitializeComponent();
            Updata();
        }

        public void Updata()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                IP.Text = App.Config.IP;
                Port.Text = App.Config.Port.ToString();
                User.Text = App.Config.User;
                Save.IsToggled = App.Config.AutoLogin;
                if (App.IsLogin)
                {
                    CloseAll();
                    Login_Button.Text = "登出";
                    Login_Button.IsEnabled = true;
                }
                else
                {
                    OpenAll();
                    Login_Button.Text = "登录";
                }
            });
        }

        private async void Login_Clicked(object sender, EventArgs e)
        {
            if (App.IsLogin)
            {
                if (IsLoging)
                    return;
                IsLoging = true;
                int port;
                if (!int.TryParse(Port.Text, out port))
                {
                    await DisplayAlert("错误", "端口号不正确", "确定");
                    IsLoging = false;
                    return;
                }
                if (string.IsNullOrWhiteSpace(User.Text))
                {
                    await DisplayAlert("错误", "用户名为空", "确定");
                    IsLoging = false;
                    return;
                }
                if (string.IsNullOrWhiteSpace(Pass.Text))
                {
                    await DisplayAlert("错误", "密码为空", "确定");
                    IsLoging = false;
                    return;
                }
                CloseAll();
                App.Config.IP = IP.Text;
                App.Config.Port = port;
                App.Config.User = User.Text;
                App.Config.AutoLogin = Save.IsToggled;
                Login_Button.Text = "登录中...";
                Act.IsRunning = true;

                App.Login(Pass.Text);

                OpenAll();
                Login_Button.Text = "登录";
                Act.IsRunning = false;
                IsLoging = false;
            }
            else 
            {
                App.LoginOut();
            }
        }

        private void CloseAll()
        {
            Login_Button.IsEnabled = false;
            IP.IsReadOnly = true;
            Port.IsReadOnly = true;
            User.IsReadOnly = true;
            Pass.IsReadOnly = true;
            Save.IsEnabled = false;
        }

        private void OpenAll()
        {
            Login_Button.IsEnabled = true;
            IP.IsReadOnly = false;
            Port.IsReadOnly = false;
            User.IsReadOnly = false;
            Pass.IsReadOnly = false;
            Save.IsEnabled = true;
        }
    }
}