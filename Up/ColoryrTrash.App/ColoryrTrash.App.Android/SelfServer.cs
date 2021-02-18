using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace ColoryrTrash.App.Droid
{
    [Service]
    class SelfServer : Service
    {
        private Thread thread;
        private bool IsRun;
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override void OnCreate()
        {
            thread = new Thread(() =>
            {
                while (IsRun)
                {
                    try
                    {
                        //MainThread.BeginInvokeOnMainThread(() =>
                        //{
                        //    Toast.MakeText(MainActivity.MainActivity_, "检测状态", ToastLength.Short).Show();
                        //});
                        if (!MqttUtils.Client.IsConnected)
                        {
                            if (App.Config.AutoLogin)
                            {
                                if (!MqttUtils.Start().Result)
                                {
                                    App.Config.AutoLogin = false;
                                    return;
                                }
                                MqttUtils.CheckLogin(App.Config.Token);
                            }
                        }
                        Thread.Sleep(1000);
                    }
                    catch (System.Exception e)
                    {

                    }
                }
            });
            IsRun = true;
            thread.Start();
        }
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }
        public override void OnDestroy()
        { 
            
        }
    }
}