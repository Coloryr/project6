using ColoryrTrash.Server.Mqtt;
using Lib;
using System;
using System.Text;

namespace ColoryrTrash.Server
{
    class ServerMain
    {
        public static string RunLocal;
        public static ConfigObj Config;
        public static SaveDataUtil SaveData;
        public static SaveUserUtil UserData;

        private static Logs Logs;
        private static Logs UserLog;

        static void Main(string[] args)
        {
            //初始化运行路径
            RunLocal = AppDomain.CurrentDomain.BaseDirectory;
            //设置编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ConfigUtil.Start();

            Logs = new Logs(RunLocal);
            Logs.LogOut("服务器启动中");

            UserLog = new Logs(RunLocal)
            {
                log = "UserLog.log"
            };

            SaveData = new();
            SaveData.Start();

            UserData = new();
            UserData.Start();
            //创建日志文件

            SocketServer.Start();
            ThisMqttServer.Start();

            while (true)
            {
                string data = Console.ReadLine();
                switch (data)
                {
                    case "stop":
                        Stop();
                        return;
                    case "test":
                        AppServer.Full("");
                        break;
                }
            }
        }

        public static void UserLogOut(string data)
        {
            UserLog.LogOut(data);
        }

        private static void Stop()
        {
            SocketServer.Stop();
            ThisMqttServer.Stop();
            SaveData.Stop();
            UserData.Stop();
        }

        /// <summary>
        /// 写错误到日志中
        /// </summary>
        /// <param name="e">Exception</param>
        public static void LogError(Exception e)
        {
            Logs.LogOut(e.ToString());
        }
        /// <summary>
        /// 写错误到日志中
        /// </summary>
        /// <param name="a">信息</param>
        public static void LogError(string a)
        {
            Logs.LogOut(a);
        }
        /// <summary>
        /// 写信息到日志中
        /// </summary>
        /// <param name="a">信息</param>
        public static void LogOut(string a)
        {
            Logs.LogOut(a);
        }
    }
}
