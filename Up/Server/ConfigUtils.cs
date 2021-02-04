using Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public record ConfigResObj
    { 
        public bool Res { get; set; }
        public string Message { get; set; }
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
                ThreadNumber = 100,
                User = new()
                {
                    { "Admin", "4e7afebcfbae000b22c7c85e5560f89a2a0280b4" }
                },
                Mysql = new()
                {
                    Enable = false,
                    IP = "127.0.0.1",
                    Port = 3306,
                    User = "root",
                    Password = "123456",
                    Database = "test"
                },
                Socket = new()
                {
                    Enable = true,
                    IP = "127.0.0.1",
                    Port = 2678
                },
                MQTT = new()
                {
                    Port = 12345,
                    User = "Admin",
                    Password = "123456",
                    Topic = "Topic/Server/#"
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

        private Task thread;
        private CancellationTokenSource token = new();

        private const string EmptyGroup = "@";
        private object Lock = new object();

        private bool Save = false;

        public ConfigResObj AddGroup(string Name)
        {
            if (Groups.ContainsKey(Name))
            {
                return new ConfigResObj
                {
                    Res = false,
                    Message = $"组{Name}已存在"
                };
            }
            else
            {
                lock (Lock)
                {
                    Groups.Add(Name, new DataSaveObj
                    {
                        Name = Name,
                        List = new()
                    });
                    ThisMqttServer.AddGroup(Name);
                }
                SaveGroup(Name);
                return new ConfigResObj
                {
                    Res = true,
                    Message = $"组{Name}已创建"
                };
            }
        }

        public void AddItem(string UUID)
        {
            string Time = string.Format("{0:s}", DateTime.Now);
            if (!UUID_Group.ContainsKey(UUID))
            {
                lock (Lock)
                {
                    var group = Groups[EmptyGroup];
                    var item = new ItemSaveObj
                    {
                        Capacity = 0,
                        UUID = UUID,
                        Nick = "新的垃圾桶",
                        Time = Time,
                        X = -1,
                        Y = -1
                    };
                    group.List.Add(UUID, item);
                    ThisMqttServer.UpdateItem(EmptyGroup, item);
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
                    ThisMqttServer.UpdateItem(group, item);
                }
            }
        }

        public void UpData(string UUID, int x, int y, int capacity, bool open, ItemState state)
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
                    item.State = state;
                    ThisMqttServer.UpdateItem(group, item);
                }
            }
        }

        public void MoveGroup(string uuid, string group)
        {
            string oldgroup = UUID_Group[uuid];
            if (oldgroup == group)
                return;
            lock (Lock)
            {
                var obj = Groups[oldgroup];
                var item = obj.List[uuid];
                obj.List.Remove(uuid);
                var obj1 = Groups[group];
                obj1.List.Add(uuid, item);
                UUID_Group[uuid] = group;
                ThisMqttServer.MoveGroup(uuid, group);
            }
        }

        public void SetNick(string uuid, string nick)
        {
            lock (Lock)
            {
                if (UUID_Group.ContainsKey(uuid))
                {
                    string group = UUID_Group[uuid];
                    if (Groups.ContainsKey(group))
                    {
                        var obj = Groups[group];
                        if (obj.List.ContainsKey(uuid))
                        {
                            var item = obj.List[uuid];
                            item.Nick = nick;
                            ThisMqttServer.UpdateItem(group, item);
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
                    Name = EmptyGroup,
                    List = new()
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
