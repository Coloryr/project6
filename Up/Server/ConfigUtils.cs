using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ConfigSave
    {
        private static object Locker = new object();
        public static T Config<T>(T obj1, string FilePath) where T : new()
        {
            FileInfo file = new FileInfo(FilePath);
            T obj;
            if (!file.Exists)
            {
                if (obj1 == null)
                    obj = new T();
                else
                    obj = obj1;
                Save(obj, FilePath);
            }
            else
            {
                lock (Locker)
                {
                    obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath));
                }
            }
            return obj;
        }
        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static void Save(object obj, string FilePath)
        {
            lock (Locker)
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(obj, Formatting.Indented));
            }
        }
    }
    internal class ConfigUtil
    {
        public static string FilePath = ServerMain.RunLocal + @"Mainconfig.json";
        /// <summary>
        /// 读配置文件
        /// </summary>
        public static void Start()
        {
            ServerMain.Config = ConfigSave.Config(new ConfigObj
            {
                IP = "127.0.0.1",
                Port = 80,
                ThreadNumber = 100,
                User = new Dictionary<string, string>()
                {
                    { "Admin" , "4e7afebcfbae000b22c7c85e5560f89a2a0280b4" }
                }
            }, FilePath);
        }

        public static void Save()
        {
            ConfigSave.Save(ServerMain.Config, FilePath);
        }
    }
}
