using System;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServerMain
    {
        public static string RunLocal;
        public static ConfigObj Config;

        private static Logs Logs;
        private static bool isGo;

        static void Main(string[] args)
        {
            isGo = true;
            //初始化运行路径
            RunLocal = AppDomain.CurrentDomain.BaseDirectory;
            //设置编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //创建日志文件
            Logs = new Logs(RunLocal);
            Logs.LogOut("服务器启动中");
            while (true)
            { 
                string data = Console.ReadLine();
                switch (data)
                {
                    case "stop":
                        Stop();
                        break;
                }
            }
        }

        private static void Stop()
        {
            isGo = false;
        }

        /// <summary>
        /// 写错误到日志中
        /// </summary>
        /// <param name="e">Exception</param>
        public static void LogError(Exception e)
        {
            Task.Factory.StartNew(() =>
            {
                string a = "[错误]" + e.ToString();
                Logs.LogWrite(a);
                Console.WriteLine(a);
            });
        }
        /// <summary>
        /// 写错误到日志中
        /// </summary>
        /// <param name="a">信息</param>
        public static void LogError(string a)
        {
            a = "[错误]" + a;
            Task.Factory.StartNew(() => Logs.LogWrite(a));
            Console.WriteLine(a);
        }
        /// <summary>
        /// 写信息到日志中
        /// </summary>
        /// <param name="a">信息</param>
        public static void LogOut(string a)
        {
            a = "[信息]" + a;
            Task.Factory.StartNew(() => Logs.LogWrite(a));
            Console.WriteLine(a);
        }
    }
}
