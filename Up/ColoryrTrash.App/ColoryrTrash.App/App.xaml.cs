
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App
{
    public partial class App : Application
    {
        public static bool IsLogin { get; set; }
        private static App ThisApp;
        public static ConfigObj Config;
        private static INotificationManager notificationManager;
        public static MqttUtils MqttUtils;

        public App()
        {
            ThisApp = this;
            InitializeComponent();

            notificationManager = DependencyService.Get<INotificationManager>();
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "config.json");
            Config = ConfigSave.Config(new ConfigObj()
            {
                AutoLogin = false,
                HttpPort = 10000,
                Token = "",
                IP = "127.0.0.1",
                Port = 12345,
                User = ""
            }, fileName);
            MqttUtils = new MqttUtils();

            MainPage = new MainPage();
        }

        public static void ShowA(string title, string text)
        {
            notificationManager.SendNotification(title, text);
        }
        public static void ShowB(string title, string text)
        {
            notificationManager.SendNotification(title, text);
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
    }
}
