using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private static HttpListener HttpServer = new();
        public static ConfigObj Config;
        public static HttpUtils HttpUtils;
        public static string Local;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Local = AppDomain.CurrentDomain.BaseDirectory;
            Config = ConfigSave.Config(new ConfigObj()
            {
                AutoLogin = false,
                Port = 10000,
                Token = "",
                Url = "https://",
                User = ""
            }, Local + "MainConfig.json");
            HttpServer.Prefixes.Add($"http://127.0.0.1:{Config.Port}/");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            HttpUtils = new();
            
            try 
            {
                HttpServer.Start();
                HttpServer.BeginGetContext(ContextReady, null);
            }
            catch
            {
                MessageBox.Show("内置服务器启动失败");
                Shutdown();
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            HttpServer.BeginGetContext(ContextReady, null);
            var res = HttpServer.EndGetContext(ar);
            var data = Encoding.UTF8.GetBytes(DesktopResource.web);
            res.Response.ContentType = "text/xml; charset=utf-8";
            res.Response.OutputStream.Write(data);
            res.Response.Close();
        }

        public static void Stop()
        {
            HttpServer.Stop();
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
