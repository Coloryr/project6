using Lib;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Server.Mqtt
{
    class AppServer
    {
        private static readonly ConcurrentDictionary<string, string> Tokens = new();
        public static async void SendAll(object data)
        {
            var temp = JsonConvert.SerializeObject(data);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicAppServer,
                QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                Payload = Encoding.UTF8.GetBytes(temp)
            };
            await ThisMqttServer.PublishAsync(message);
        }

        public static async void Send(string uuid, object obj)
        {
            string data = JsonConvert.SerializeObject(obj);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicAppServer + "/" + uuid,
                QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await ThisMqttServer.PublishAsync(message);
        }
        public static void AppReceived(string ClientId, byte[] Payload)
        {
            string data = Encoding.UTF8.GetString(Payload);
            var obj = JsonConvert.DeserializeObject<DataPackObj>(data);
            string User = ClientId;
            if (User == null)
                return;
            string Token = obj.Token;
            if (obj.Type == DataType.CheckLogin)
            {
                var res = false;
                if (Tokens.ContainsKey(User))
                {
                    res = Tokens[User] == Token;
                }
                Send(ClientId, new DataPackObj
                {
                    Type = DataType.CheckLogin,
                    Res = res
                });
                if (res)
                {
                    ServerMain.UserData.UserLogin(User);
                }
                return;
            }
            else if (obj.Type == DataType.Login)
            {
                var pass = ServerMain.UserData.GetPass(User);
                if (pass == null || obj.Data != pass)
                {
                    Send(ClientId, new DataPackObj
                    {
                        Type = DataType.Login,
                        Res = false,
                        Data = "账户或密码错误"
                    });
                }
                else
                {
                    string uuid = Guid.NewGuid().ToString();
                    if (Tokens.ContainsKey(User))
                        Tokens[User] = uuid;
                    else
                        Tokens.TryAdd(User, uuid);
                    Send(ClientId, new DataPackObj
                    {
                        Type = DataType.Login,
                        Res = true,
                        Data = uuid
                    });
                    ServerMain.UserData.UserLogin(User);
                }
                return;
            }
            if (!Tokens.ContainsKey(User) || Tokens[User] != Token)
            {
                Send(ClientId, new DataPackObj
                {
                    Type = DataType.Login,
                    Res = false,
                    Data = "账户错误"
                });
                return;
            }
            string temp = obj.Data;
            string temp1 = obj.Data1;
            switch (obj.Type)
            {
                case DataType.GetUserGroup:
                    if (ServerMain.UserData.ID_Group.ContainsKey(temp))
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetUserGroup,
                            Res = true,
                            Data = ServerMain.UserData.ID_Group[temp]
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetTrashGroupInfo,
                            Res = true,
                            Data1 = "没有组"
                        });
                    }
                    break;
                case DataType.GetUserTask:
                    if (ServerMain.UserData.ID_Group.ContainsKey(temp))
                    {
                        var group = ServerMain.UserData.ID_Group[temp];
                        var obj1 = ServerMain.UserData.Groups[group];
                        var list = new List<TrashSaveObj>();
                        foreach (var item in obj1.Bind)
                        {
                            if (ServerMain.SaveData.Groups.ContainsKey(item))
                            {
                                var list1 = ServerMain.SaveData.Groups[item].List;
                                list.AddRange(list1.Values);
                            }
                        }
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetUserTask,
                            Res = true,
                            Data = JsonConvert.SerializeObject(list)
                        });
                    }
                    else
                    {
                        Send(ClientId, new DataPackObj
                        {
                            Type = DataType.GetUserTask,
                            Res = true,
                            Data1 = "没有垃圾桶"
                        });
                    }
                    break;
                case DataType.UpdataNow:
                    if (ServerMain.SaveData.CheckUUID(temp))
                    {
                        TrashServer.Send(temp, "Up");
                    }
                    break;
            }
        }

        public static void Full(string uuid)
        {
            if (uuid == "")
            {
                var send = new DataPackObj
                {
                    Type = DataType.Full,
                    Res = true,
                    Data = "",
                    Data1 = uuid
                };
                Task.Run(() => SendAll(send));
            }
            else if (ServerMain.SaveData.CheckUUID(uuid))
            {
                var temp = ServerMain.SaveData.UUID_Group[uuid];
                var list = new List<string>();
                foreach (var item in ServerMain.UserData.Groups.Values)
                {
                    if (item.Bind.Contains(temp))
                    {
                        list.Add(item.Name);
                    }
                }
                if (list.Count > 0)
                {
                    var send = new DataPackObj
                    {
                        Type = DataType.Full,
                        Res = true,
                        Data = JsonConvert.SerializeObject(list)
                    };
                    Task.Run(() => SendAll(send));
                }
            }
        }

        public static void UpdateTrashItem(string group, TrashSaveObj item)
        {
            var send = new DataPackObj
            {
                Type = DataType.UpdataTrash,
                Res = true,
                Data = group,
                Data1 = JsonConvert.SerializeObject(item)
            };
            Task.Run(() => SendAll(send));
        }
    }
}
