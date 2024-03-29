﻿using Lib;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Formatter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.App
{
    public class MqttUtils
    {
        private static string SelfServerTopic;
        private static string SelfClientTopic;
        private static bool IsConnecting;

        public static string Token { get; set; }
        public static MqttClient Client;
        public static void Init()
        {
            Client = new MqttFactory().CreateMqttClient() as MqttClient;
            Client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnMqttClientConnected);
            Client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnMqttClientDisConnected);
            Client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnSubscriberMessageReceived);
        }

        public static void OnSubscriberMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                if (arg.ApplicationMessage.Topic == SelfServerTopic)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    if (obj.Type == DataType.Login)
                    {
                        if (obj.Res)
                        {
                            Token = obj.Data;
                            App.LoginDone();
                            App.IsLogin = true;
                            App.Show("登录", "登录成功");
                        }
                        else
                        {
                            App.IsLogin = false;
                            App.Show("登录", obj.Data);
                        }
                        return;
                    }
                    else if (obj.Type == DataType.CheckLogin)
                    {
                        if (obj.Res == false)
                        {
                            App.Show("自动登录", "自动登录失败");
                        }
                        else
                        {
                            App.IsLogin = true;
                            Token = App.Config.Token;
                            App.LoginDone();
                            App.Show("自动登录", "已自动登录");
                        }
                        return;
                    }
                    if (!obj.Res)
                    {
                        App.IsLogin = false;
                        App.Show("登录", obj.Data);
                        App.LoginOut();
                        return;
                    }
                    switch (obj.Type)
                    {
                        case DataType.GetUserGroup:
                            if (obj.Data == null)
                            {
                                App.Show("组获取", obj.Data1);
                            }
                            else
                            {
                                App.helloPage.SetGroup(obj.Data);
                                App.GroupName = obj.Data;
                            }
                            break;
                        case DataType.GetUserTask:
                            if (obj.Data == null)
                            {
                                App.Show("垃圾桶列表获取", obj.Data1);
                            }
                            else
                            {
                                var list = JsonConvert.DeserializeObject<List<TrashSaveObj>>(obj.Data);
                                App.infoPage.SetList(list);
                            }
                            break;
                    }
                }
                else if (arg.ApplicationMessage.Topic == DataArg.TopicAppServer)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    switch (obj.Type)
                    {
                        case DataType.SetUserGroupBind:
                            if (App.GroupName == obj.Data)
                            {
                                GetItems();
                            }
                            break;
                        case DataType.UpdataTrash:
                            if (App.GroupName == obj.Data)
                            {
                                App.infoPage.Update(JsonConvert.DeserializeObject<TrashSaveObj>(obj.Data1));
                            }
                            break;
                        case DataType.Full:
                            if (obj.Data == "")
                            {
                                App.notificationManager.SendNotification("垃圾桶", $"有垃圾桶快满");
                            }
                            else
                            {
                                var list = JsonConvert.DeserializeObject<List<string>>(obj.Data);
                                if (list.Contains(App.GroupName))
                                {
                                    App.notificationManager.SendNotification("垃圾桶", $"有垃圾桶快满");
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //App.LogError(ex);
            }
        }

        public static void OnMqttClientDisConnected(MqttClientDisconnectedEventArgs arg)
        {
            if (!IsConnecting)
            {
                App.Show("服务器", "服务器连接断开");
                App.LoginOut();
            } 
        }

        public static void OnMqttClientConnected(MqttClientConnectedEventArgs arg)
        {
            App.Show("服务器", "服务器已连接");
        }

        public static void Send(string message)
        {
            if (Client.IsConnected)
            {
                var obj = new MqttApplicationMessageBuilder()
                    .WithTopic(SelfClientTopic)
                    .WithPayload(Encoding.UTF8.GetBytes(message))
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();
                Client.PublishAsync(obj);
            }
        }
        public static async Task<bool> Start()
        {
            try
            {
                IsConnecting = true;
                if (Client.IsConnected)
                    await Stop();
                var options = new MqttClientOptions
                {
                    ProtocolVersion = MqttProtocolVersion.V311,
                    ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = App.Config.IP,
                        Port = App.Config.Port
                    }
                };
                if (options.ChannelOptions == null)
                {
                    throw new InvalidOperationException();
                }
                options.Credentials = new MqttClientCredentials
                {
                    Username = App.Config.User
                };

                options.CleanSession = true;
                options.KeepAlivePeriod = TimeSpan.FromSeconds(5);
                options.ClientId = App.Config.User;

                await Client.ConnectAsync(options);
                SelfServerTopic = DataArg.TopicAppServer + "/" + App.Config.User;
                SelfClientTopic = DataArg.TopicAppClient + "/" + App.Config.User;
                await Client.SubscribeAsync(SelfServerTopic);
                await Client.SubscribeAsync(DataArg.TopicAppServer);
                IsConnecting = false;
                return true;
            }
            catch (Exception e)
            {
                string temp = e.ToString();
                //App.LogError(e);
            }
            App.Show("服务器", "服务器连接失败");
            return false;
        }

        public static async Task Stop()
        {
            try
            {
                await Client.DisconnectAsync();
            }
            catch (Exception)
            {
                //App.LogError(e);
            }
        }

        public static void CheckLogin(string token)
        {
            var obj = new DataPackObj
            {
                Token = token,
                Type = DataType.CheckLogin
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public static void Login(string pass)
        {
            var obj = new DataPackObj
            {
                Type = DataType.Login,
                Data = Tools.GenSHA1(pass)
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public static void GetInfo()
        {
            var obj = new DataPackObj
            {
                Token = Token,
                Type = DataType.GetUserGroup,
                Data = App.Config.User
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public static void GetItems()
        {
            var obj = new DataPackObj
            {
                Token = Token,
                Type = DataType.GetUserTask,
                Data = App.Config.User
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public static void Updata(string uuid)
        {
            var obj = new DataPackObj
            {
                Token = Token,
                Type = DataType.UpdataNow,
                Data = uuid
            };
            Send(JsonConvert.SerializeObject(obj));
        }
    }
}
