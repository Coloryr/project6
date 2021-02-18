using ColoryrTrash.App.Pages;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColoryrTrash.App
{
    public partial class App : Application
    {
        public static bool IsLogin { get; set; }
        public static string GroupName;
        public static ConfigObj Config;

        public static MainPage mainPage;
        public static HelloPage helloPage;
        public static InfoPage infoPage;
        public static LoginPage loginPage;
        public static MapPage mapPage;

        public static NavigationPage helloPage_;
        public static NavigationPage infoPage_;
        public static NavigationPage loginPage_;
        public static NavigationPage mapPage_;

        public static IMessageHand MessageHand;
        public static INotificationManager notificationManager;

        private static App ThisApp;
        private static HttpListener HttpServer = new HttpListener();
        private static byte[] ImgData;
        private static byte[] WebData;

        private static string fileName;

        public App()
        {
            ThisApp = this;
            InitializeComponent();

            notificationManager = DependencyService.Get<INotificationManager>();
            MessageHand = DependencyService.Get<IMessageHand>();

            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "config.json");
            Config = ConfigSave.Config(new ConfigObj()
            {
                AutoLogin = false,
                HttpPort = 10000,
                Token = "",
                IP = "127.0.0.1",
                Port = 12345,
                User = ""
            }, fileName);

            MqttUtils.Init();

            mainPage = new MainPage();
            helloPage = new HelloPage();
            infoPage = new InfoPage();
            loginPage = new LoginPage();
            mapPage = new MapPage();

            helloPage_ = new NavigationPage(helloPage);
            infoPage_ = new NavigationPage(infoPage);
            loginPage_ = new NavigationPage(loginPage);
            mapPage_ = new NavigationPage(mapPage);

            MainPage = mainPage;
        }

        public static void Show(string title, string text)
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                ThisApp.Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    MessageHand.Message(title, text);
                });
            }
            else
            {
                notificationManager.SendNotification(title, text);
            }
        }
        public static void LoginDone()
        {
            IsLogin = true;
            Save();
            mainPage.SetName(Config.User);
            helloPage.SetName(Config.User);
            loginPage.Updata();
            mainPage.Switch(PageName.MainPage);
            MqttUtils.GetInfo();
            MqttUtils.GetItems();
        }
        public static void LoginOut()
        {
            IsLogin = false;
            Save();
            mainPage.ClearName();
            helloPage.ClearName();
            helloPage.ClearGroup();
            loginPage.Updata();
            infoPage.Clear();
            mapPage.Clear();
            mainPage.Switch(PageName.LoginPage);
        }
        private static void Save()
        {
            if (Config.AutoLogin)
            {
                Config.Token = MqttUtils.Token;
            }
            ConfigSave.Save(Config, fileName);
        }

        public static async void Login(string pass)
        {
            if (!await MqttUtils.Start())
            {
                return;
            }
            MqttUtils.Login(pass);
        }

        protected override void OnStart()
        {
            HttpServer.Prefixes.Add($"http://+:{Config.HttpPort}/");

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("ColoryrTrash.App.map.png");

            ImgData = new byte[stream.Length];
            stream.Read(ImgData);
            stream.Close();

            stream = assembly.GetManifestResourceStream("ColoryrTrash.App.html.txt");

            WebData = new byte[stream.Length];
            stream.Read(WebData);
            stream.Close();

            HttpServer.Start();
            HttpServer.BeginGetContext(ContextReady, null);

            Task.Run(() =>
            {
                Thread.Sleep(1000);
                if (MqttUtils.Start().Result && Config.AutoLogin)
                {
                    MqttUtils.Token = Config.Token;
                    MqttUtils.CheckLogin(Config.Token);
                }
                if (!IsLogin)
                {
                    mainPage.Switch(PageName.LoginPage);
                }
            });
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            if (Config.AutoLogin)
            {
                Task.Run(async () =>
                {
                    if (!MqttUtils.Client.IsConnected && !await MqttUtils.Start())
                    {
                        Show("服务器", "服务器连接断开");
                        LoginOut();
                        return;
                    }

                    MqttUtils.CheckLogin(Config.Token);
                });
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                HttpServer.BeginGetContext(ContextReady, null);
                var res = HttpServer.EndGetContext(ar);
                if (res.Request.RawUrl == "/icon.png")
                {
                    res.Response.ContentType = "image/png";
                    res.Response.OutputStream.Write(ImgData);
                    res.Response.Close();
                }
                else
                {
                    res.Response.ContentType = "text/html; charset=utf-8";
                    res.Response.OutputStream.Write(WebData);
                    res.Response.Close();
                }
            }
            catch
            {

            }
        }
    }
}
