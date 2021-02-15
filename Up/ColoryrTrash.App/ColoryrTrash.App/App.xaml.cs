using ColoryrTrash.App.Pages;
using Lib;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColoryrTrash.App
{
    public partial class App : Application
    {
        public static bool IsLogin { get; set; }
        public static ConfigObj Config;
        public static MqttUtils MqttUtils;

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

        private static App ThisApp;
        private static INotificationManager notificationManager;

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

            HttpServer.Prefixes.Add($"http://+:{Config.HttpPort}/");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

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

            MqttUtils = new MqttUtils();

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

            Task.Run(() =>
            {
                Thread.Sleep(5000);
                mapPage.AddPoint(new TrashSaveObj
                {
                    X = 116408293,
                    Y = 39918477,
                    UUID = "UUID",
                    Nick = "垃圾桶",
                });
            });
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
                    mainPage.Switch("Login");
                }
            });
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
            loginPage.Updata();
        }
        public static void LoginOut()
        {
            Config.Token = "";
            IsLogin = false;
            Save();
            mainPage.ClearName();
            loginPage.Updata();
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
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
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
