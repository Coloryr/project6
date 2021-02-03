﻿using Lib;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Version = "1.0.0";
        
        public static ConfigObj Config;
        public static MqttUtils HttpUtils;
        public static Login LoginWindows;
        
        public static string RunLocal;

        private static bool IconSet;
        private static HttpListener HttpServer = new();
        private static System.Windows.Forms.NotifyIcon notifyIcon;
        private static Logs Logs;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                RunLocal = AppDomain.CurrentDomain.BaseDirectory;

                Config = ConfigSave.Config(new ConfigObj()
                {
                    AutoLogin = false,
                    HttpPort = 10000,
                    Token = "",
                    IP = "127.0.0.1",
                    Port = 12345,
                    User = ""
                }, RunLocal + "MainConfig.json");

                Logs = new Logs(RunLocal);

                HttpServer.Prefixes.Add($"http://127.0.0.1:{Config.HttpPort}/");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                notifyIcon = new();
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipText = "ColoryrTrash";

                HttpUtils = new MqttUtils();

                LoginWindows = new Login();
                LoginWindows.ShowDialog();

                HttpServer.Start();
                HttpServer.BeginGetContext(ContextReady, null);
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowB("启动", "内置服务器启动失败");
                Shutdown();
            }
        }

        internal static void SetIcon(Icon icon)
        {
            if (!IconSet)
            {
                IconSet = true;
                notifyIcon.Icon = icon;
            }
        }

        public static void ShowA(string title, string data)
        {
            notifyIcon.ShowBalloonTip(300, title, data, System.Windows.Forms.ToolTipIcon.Info);
            Log(title + "|" + data);
        }
        public static void ShowB(string title, string data)
        {
            notifyIcon.ShowBalloonTip(300, title, data, System.Windows.Forms.ToolTipIcon.Error);
            LogError(title + "|" + data);
        }

        public static void CheckLogin(bool login)
        {
            if (login)
            {
                ShowA("登录", "自动登录成功");
            }
            else
            {
                ShowB("登录", "自动登录失败");
            }
        }

        public static void DisConnect()
        {
            LoginWindows = new Login();
            LoginWindows.ShowDialog();
        }

        public static void Log(string data)
        {
            Logs.LogOut(data);
        }

        public static void LogError(string data)
        {
            Logs.LogError(data);
        }

        public static void LogError(Exception e)
        {
            Logs.LogError(e);
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                HttpServer.BeginGetContext(ContextReady, null);
                var res = HttpServer.EndGetContext(ar);
                var data = Encoding.UTF8.GetBytes(File.ReadAllText(@"E:\MCU's_src\project6\Up\Desktop\Resources\web.txt"));
                res.Response.ContentType = "text/html; charset=utf-8";
                res.Response.OutputStream.Write(data);
                res.Response.Close();
            }
            catch
            {

            }
        }

        public static void Stop()
        {
            HttpServer.Stop();
        }

        public static void Save()
        {
            ConfigSave.Save(Config, RunLocal + "MainConfig.json");
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                MessageBox.Show("捕获未处理异常:" + e.Exception.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误" + ex.ToString());
            }

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("发生错误，将关闭\n");
            }
            sbEx.Append("捕获未处理异常：");
            if (e.ExceptionObject is Exception)
            {
                sbEx.Append(((Exception)e.ExceptionObject).ToString());
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            MessageBox.Show(sbEx.ToString());
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show("捕获线程内未处理异常：" + e.Exception.ToString());
            e.SetObserved();
        }
    }
}
