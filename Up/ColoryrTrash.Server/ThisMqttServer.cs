﻿using Lib;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Server
{
    class ThisMqttServer
    {
        private static MqttServer Server;
        private static readonly ConcurrentDictionary<string, string> Tokens = new();
        public static bool IsRun { get; set; }//是否在运行

        public static bool CheckLogin(string user, string token)
        {
            if (Tokens.ContainsKey(user) && Tokens[user] == token)
                return true;
            return false;
        }

        public static async void Start()
        {
            ServerMain.LogOut("MQTT服务器正在启动");
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(ServerMain.Config.MQTT.Port)
                .WithConnectionValidator(Check)
                .WithSubscriptionInterceptor(c => c.AcceptSubscription = true)
                .WithApplicationMessageInterceptor(c => c.AcceptPublish = true);

            Server = new MqttFactory().CreateMqttServer() as MqttServer;

            Server.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(OnConnected);
            Server.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(OnDisconnected);
            Server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(OnSubscribedTopic);
            Server.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(OnUnsubscribedTopic);
            Server.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnMessageReceived);

            await Server.StartAsync(optionsBuilder.Build());

            IsRun = true;
            ServerMain.LogOut("MQTT服务器已启动");
        }

        private static void OnMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                string data = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                var obj = JsonConvert.DeserializeObject<DataPackObj>(data);
                string User = arg.ClientId;
                if (User == null)
                    return;
                string Token = obj.Token;
                switch (obj.Type)
                {
                    case DataType.CheckLogin:
                        if (ServerMain.Config.User.ContainsKey(User))
                        {
                            if (Tokens.ContainsKey(User))
                            {
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.CheckLogin,
                                    Res = Tokens[User] == Token
                                });
                                break;
                            }
                        }
                        SendItem(arg.ClientId, new DataPackObj
                        {
                            Type = DataType.CheckLogin,
                            Res = false
                        });
                        break;
                    case DataType.Login:
                        if (ServerMain.Config.User.ContainsKey(User))
                        {
                            string data1 = obj.Data as string;
                            if(string.IsNullOrWhiteSpace(data1))
                            {
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.Login,
                                    Res = false,
                                    Data = "账户或密码错误"
                                });
                            }
                            string data2 = ServerMain.Config.User[User];
                            if (data2 != data1)
                            {
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.Login,
                                    Res = false,
                                    Data = "账户或密码错误"
                                });
                            }
                            else
                            {
                                string uuid = Guid.NewGuid().ToString();
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.Login,
                                    Res = true,
                                    Data = uuid
                                });
                                if (Tokens.ContainsKey(User))
                                {
                                    Tokens[User] = uuid;
                                }
                                else
                                {
                                    Tokens.TryAdd(User, uuid);
                                }
                            }
                        }
                        else
                        {
                            SendItem(arg.ClientId, new DataPackObj
                            {
                                Type = DataType.Login,
                                Res = false,
                                Data = "账户或密码错误"
                            });
                        }
                        break;
                    case DataType.GetGroups:
                        if (!CheckLogin(User, Token))
                        {
                            SendItem(arg.ClientId, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = false,
                                Data = "账户错误"
                            });
                        }
                        else
                        {
                            SendItem(arg.ClientId, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = true,
                                Data = JsonConvert.SerializeObject(ServerMain.SaveData.Groups.Keys)
                            });
                        }
                        break;
                    case DataType.GetGroupInfo:
                        if (!CheckLogin(User, Token))
                        {
                            SendItem(arg.ClientId, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = false,
                                Data = "账户错误"
                            });
                        }
                        else
                        {
                            string temp = obj.Data as string;
                            if (ServerMain.SaveData.Groups.ContainsKey(temp))
                            {
                                var group = ServerMain.SaveData.Groups[temp];
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.GetGroupInfo,
                                    Res = true,
                                    Data = JsonConvert.SerializeObject(group)
                                });
                            }
                            else
                            {
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.GetGroupInfo,
                                    Res = true,
                                    Data1 = $"没有组[{temp}]"
                                });
                            }
                        }
                        break;
                    case DataType.AddGroup:
                        if (!CheckLogin(User, Token))
                        {
                            SendItem(arg.ClientId, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = false,
                                Data = "账户错误"
                            });
                        }
                        else
                        {
                            string temp = obj.Data as string;
                            var res = ServerMain.SaveData.AddGroup(temp);
                            if (res)
                            {
                                ServerMain.UserLogOut($"用户[{User}]创建组[{temp}]");
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.AddGroup,
                                    Res = true,
                                    Data = $"组[{temp}]创建成功"
                                });
                            }
                            else
                            {
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.AddGroup,
                                    Res = true,
                                    Data = $"组[{temp}]创建失败"
                                });
                            }
                        }
                        break;
                    case DataType.RenameGroup:
                        if (!CheckLogin(User, Token))
                        {
                            SendItem(arg.ClientId, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = false,
                                Data = "账户错误"
                            });
                        }
                        else
                        {
                            string temp = obj.Data as string;
                            string temp1 = obj.Data1 as string;
                            var res = ServerMain.SaveData.RenameGroup(temp, temp1);
                            if (res)
                            {
                                ServerMain.UserLogOut($"用户[{User}]修改组[{temp}]为[{temp1}]");
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.AddGroup,
                                    Res = true,
                                    Data = $"组[{temp}]已重命名为[{temp1}]"
                                });
                            }
                            else
                            {
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.AddGroup,
                                    Res = true,
                                    Data = $"组[{temp}]重命名失败"
                                });
                            }
                        }
                        break;
                    case DataType.MoveGroup:
                        if (!CheckLogin(User, Token))
                        {
                            SendItem(arg.ClientId, new DataPackObj
                            {
                                Type = DataType.GetGroups,
                                Res = false,
                                Data = "账户错误"
                            });
                        }
                        else
                        {
                            string temp = obj.Data as string;
                            string temp1 = obj.Data1 as string;
                            var res = ServerMain.SaveData.MoveGroup(temp, temp1);
                            if (res)
                            {
                                ServerMain.UserLogOut($"用户[{User}]移动[{temp}]到组[{temp1}]");
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.AddGroup,
                                    Res = true,
                                    Data = $"[{temp}]已移动到[{temp1}]"
                                });
                            }
                            else
                            {
                                SendItem(arg.ClientId, new DataPackObj
                                {
                                    Type = DataType.AddGroup,
                                    Res = true,
                                    Data = $"[{temp}]移动失败"
                                });
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ServerMain.LogError(e);
            }
        }

        private static void OnUnsubscribedTopic(MqttServerClientUnsubscribedTopicEventArgs arg)
        {
            ServerMain.LogOut($"客户端[{arg.ClientId}]取消订阅频道[{arg.TopicFilter}]");
        }

        private static void OnSubscribedTopic(MqttServerClientSubscribedTopicEventArgs arg)
        {
            ServerMain.LogOut($"客户端[{arg.ClientId}]订阅频道[{arg.TopicFilter.Topic}]");
        }

        private static void OnDisconnected(MqttServerClientDisconnectedEventArgs arg)
        {
            ServerMain.LogOut($"客户端[{arg.ClientId}]断开连接");
        }

        private static void OnConnected(MqttServerClientConnectedEventArgs arg)
        {
            ServerMain.LogOut($"客户端[{arg.ClientId}]连接");
        }

        public static async void Stop()
        {
            ServerMain.LogOut("MQTT服务器正在关闭");
            IsRun = false;
            await Server.StopAsync();
            ServerMain.LogOut("MQTT服务器已关闭");
        }

        public static async void SendAll(string data)
        {
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicServer,
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await Server.PublishAsync(message);
        }

        public static async void SendItem(string uuid, object obj)
        {
            string data = JsonConvert.SerializeObject(obj);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicServer + "/" + uuid,
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await Server.PublishAsync(message);
        }

        public static async void SendItem(string uuid, string data)
        {
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicServer + "/" + uuid,
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await Server.PublishAsync(message);
        }

        private static void Check(MqttConnectionValidatorContext arg)
        {
            string user = arg.Username;
            if (ServerMain.Config.User.ContainsKey(user))
            {
                arg.ReasonCode = MqttConnectReasonCode.Success;
            }
            else
            {
                string pass = arg.Password;
                if (user == ServerMain.Config.MQTT.User && pass == ServerMain.Config.MQTT.Password)
                {
                    arg.ReasonCode = MqttConnectReasonCode.Success;
                }
                else
                {
                    arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                }
            }
        }


        public static void AddGroup(string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.AddGroup,
                Res = true,
                Data = group
            };
            Task.Run(() => SendAll(JsonConvert.SerializeObject(send)));
        }

        public static void RenameGroup(string old, string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.RenameGroup,
                Res = true,
                Data = old, 
                Data1 = group
            };
            Task.Run(() => SendAll(JsonConvert.SerializeObject(send)));
        }

        public static void MoveGroup(string uuid, string group)
        {
            var send = new DataPackObj
            {
                Type = DataType.MoveGroup,
                Res = true,
                Data = uuid,
                Data1 = group
            };
            Task.Run(() => SendAll(JsonConvert.SerializeObject(send)));
        }

        public static void UpdateItem(string group, ItemSaveObj obj)
        {
            var send = new DataPackObj
            {
                Type = DataType.Updata,
                Res = true,
                Data = group,
                Data1 = obj
            };
            Task.Run(() => SendAll(JsonConvert.SerializeObject(send)));
        }
    }
}