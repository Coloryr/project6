using Lib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ColoryrTrash.Desktop
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
        public static MainWindow MainWindow_;
        public static ListWindows ListWindows_;
        
        public static string RunLocal;
        public static bool IsLogin;

        private static App ThisApp;
        private static bool IconSet;
        private static HttpListener HttpServer = new();
        private static System.Windows.Forms.NotifyIcon notifyIcon;
        private static Logs Logs;
        private static byte[] ImgData;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                ThisApp = this;
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

                using (MemoryStream stream = new MemoryStream())
                {
                    DesktopResource.map.Save(stream, ImageFormat.Png);
                    ImgData = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(ImgData, 0, Convert.ToInt32(stream.Length));
                }

                HttpServer.Start();
                HttpServer.BeginGetContext(ContextReady, null);

                Login();
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show("内置服务器启动失败", "启动");
                Shutdown();
            }
        }

        public static void Login()
        {
            ThisApp.Dispatcher.Invoke(() =>
            {
                if (LoginWindows == null)
                {
                    LoginWindows = new Login();
                }
                LoginWindows.Show();
            });
        }

        public static void Start()
        {
            ThisApp.Dispatcher.Invoke(() =>
            {
                ListWindows_ = new();
                ListWindows_.Show();
            });
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

        public static void DisConnect()
        {
            ThisApp.Dispatcher.Invoke(() =>
            {
                if (LoginWindows == null)
                {
                    LoginWindows = new Login();
                    LoginWindows.ShowDialog();
                }
                else
                {
                    LoginWindows.Activate();
                }
            });
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
                if (res.Request.RawUrl == "/icon.png")
                {
                    res.Response.ContentType = "image/png";
                    res.Response.OutputStream.Write(ImgData);
                    res.Response.Close();
                }
                else
                {
                    var data = Encoding.UTF8.GetBytes(File.ReadAllText(@"E:\MCU's_src\project6\Up\ColoryrTrash.Desktop\Resources\web.txt"));
                    res.Response.ContentType = "text/html; charset=utf-8";
                    res.Response.OutputStream.Write(data);
                    res.Response.Close();
                }
            }
            catch
            {

            }
        }

        public static void Stop()
        {
            HttpServer.Stop();
            ThisApp.Shutdown();
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

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            notifyIcon?.Dispose();
        }
    }
}
