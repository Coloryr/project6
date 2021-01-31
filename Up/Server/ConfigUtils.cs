using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
    public class SaveDataUtil
    { 
        public string FilePath = ServerMain.RunLocal + "Save\\";
        public readonly Dictionary<string, DataSaveObj> Groups = new();
        public readonly Dictionary<string, string> UUID_Group = new();

        private static Task thread;
        private static CancellationTokenSource token;

        private const string EmptyGroup = "*";
        private object Lock = new object();

        private static bool Save = false;

        public void AddGroup(string Name)
        { 
        
        }

        public void AddItem(string UUID)
        {
            string Time = string.Format("{0:s}", DateTime.Now);
            if (!UUID_Group.ContainsKey(UUID))
            {
                lock (Lock)
                {
                    var item = Groups[EmptyGroup];
                    item.List.Add(UUID, new ItemSaveObj
                    {
                        Capacity = 0,
                        UUID = UUID,
                        Name = "新的垃圾桶",
                        Time = Time,
                        X = -1,
                        Y = -1
                    });
                }
                SaveGroup(EmptyGroup);
            }
            else
            {
                lock (Lock)
                {
                    var group = UUID_Group[UUID];
                    var obj = Groups[group];
                    var item = obj.List[UUID];
                    item.Time = Time;
                }
            }
        }

        public void UpData(string UUID, int x, int y, int capacity, bool open)
        {
            if (UUID_Group.ContainsKey(UUID))
            {
                lock (Lock)
                {
                    string group = UUID_Group[UUID];
                    var obj = Groups[group];
                    var item = obj.List[UUID];
                    item.Capacity = capacity;
                    item.X = x;
                    item.Y = y;
                    item.Open = open;
                }
            }
        }

        public void MoveGroup(string UUID, string Group)
        {
            string oldgroup = UUID_Group[UUID];
            if (oldgroup == Group)
                return;
            lock (Lock)
            {
                var obj = Groups[oldgroup];
                var item = obj.List[UUID];
                obj.List.Remove(UUID);
                var obj1 = Groups[Group];
                obj1.List.Add(UUID, item);
                UUID_Group[UUID] = Group;
            }
        }

        public void SetName(string UUID, string Name)
        {
            lock (Lock)
            {
                if (UUID_Group.ContainsKey(UUID))
                {
                    string group = UUID_Group[UUID];
                    if (Groups.ContainsKey(group))
                    {
                        var obj = Groups[group];
                        if (obj.List.ContainsKey(UUID))
                        {
                            obj.List[UUID].Name = Name;
                        }
                    }
                }
            }
        }

        public void Start()
        {
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            var files = Directory.GetFiles(FilePath);
            foreach (var item in files)
            {
                var obj = JsonConvert.DeserializeObject<DataSaveObj>(File.ReadAllText(item));
                Groups.Add(obj.Name, obj);
                foreach (var item1 in obj.List)
                {
                    UUID_Group.Add(item1.Key, obj.Name);
                }
            }
            if (!Groups.ContainsKey(EmptyGroup))
            {
                Groups.Add(EmptyGroup, new DataSaveObj
                {
                    Name = EmptyGroup
                });
                SaveGroup(EmptyGroup);
            }

            thread = Task.Run(() =>
            {
                while (true)
                {
                    if (Save)
                    {
                        SaveAll();
                        Save = false;
                    }
                    Thread.Sleep(30000);
                }
            }, token.Token);
        }

        public void Stop()
        {
            token.Cancel();
            SaveAll();
        }

        public void SaveGroup(string group)
        {
            lock (Lock)
            {
                if (Groups.ContainsKey(group))
                {
                    var obj = Groups[group];
                    var data = JsonConvert.SerializeObject(obj);
                    File.WriteAllText($"{FilePath}{obj.Name}.json", data);
                }
            }
        }

        public void SaveUUID(string uuid)
        {
            lock (Lock)
            {
                if (UUID_Group.ContainsKey(uuid))
                {
                    var group = UUID_Group[uuid];
                    var obj = Groups[group];
                    var data = JsonConvert.SerializeObject(obj);
                    File.WriteAllText($"{FilePath}{obj.Name}.json", data);
                }
            }
        }

        public void SaveAll()
        {
            lock (Lock)
            {
                foreach (var item in Groups.Values)
                {
                    var data = JsonConvert.SerializeObject(item);
                    File.WriteAllText($"{FilePath}{item.Name}.json", data);
                }
            }
        }
    }
}
