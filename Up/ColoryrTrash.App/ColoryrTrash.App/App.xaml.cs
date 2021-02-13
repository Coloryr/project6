
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App
{
    public partial class App : Application
    {
        public static bool IsLogin { get; set; }
        private static App ThisApp;

        public App()
        {
            ThisApp = this;
            InitializeComponent();
            MainPage = new MainPage();
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
