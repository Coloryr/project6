using Newtonsoft.Json;
using System.IO;

namespace ColoryrTrash.App
{
    public class ConfigObj
    {
        public int HttpPort { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Token { get; set; }
        public bool AutoLogin { get; set; }
    }

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
                    if (obj == null)
                    {
                        if (obj1 == null)
                            obj = new T();
                        else
                            obj = obj1;
                    }
                    Save(obj, FilePath);
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
}
