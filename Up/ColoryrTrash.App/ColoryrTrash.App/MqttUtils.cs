using Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Mqtt;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ColoryrTrash.App
{
    public class MqttUtils
    {
        private static string SelfServerTopic;
        private static string SelfClientTopic;
        private static bool IsConnecting;

        public static string Token { get; set; }
        public static ISelfMqtt Client;
        public static void Init()
        {
            Client = DependencyService.Get<ISelfMqtt>();
        }

        public static void OnSubscriberMessageReceived(MqttApplicationMessage arg)
        {
            try
            {
                if (arg.Topic == SelfServerTopic)
                {
                    string Message = Encoding.UTF8.GetString(arg.Payload);
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
                else if (arg.Topic == DataArg.TopicAppServer)
                {
                    string Message = Encoding.UTF8.GetString(arg.Payload);
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

        public static void OnMqttClientDisConnected(object sender, MqttEndpointDisconnected arg)
        {
            if (IsConnecting)
                return;
            if (App.Config.AutoLogin)
            {
                Task.Run(async () =>
                {
                    if (!await Start())
                        CheckLogin(App.Config.Token);
                    else
                    {
                        IsConnecting = true;
                        App.Config.AutoLogin = false;
                        App.Show("服务器", "服务器连接断开");
                        App.LoginOut();
                    }
                });
            }
            else if (!IsConnecting)
            {
                App.Show("服务器", "服务器连接断开");
                App.LoginOut();
            } 
        }

        //public static void OnMqttClientConnected(MqttClientConnectedEventArgs arg)
        //{
        //    App.Show("服务器", "服务器已连接");
        //}

        public static void Send(string message)
        {
            if (Client.IsConnected())
            {
                var obj = new MqttApplicationMessage(SelfClientTopic, Encoding.UTF8.GetBytes(message));
                Client.PublishAsync(obj);
            }
        }
        public static async Task<bool> Start()
        {
            try
            {
                IsConnecting = true;
                if (Client.IsConnected())
                    await Stop();
                var configuration = new MqttConfiguration
                {
                    BufferSize = 128 * 1024,
                    Port = App.Config.Port,
                    KeepAliveSecs = 10,
                    WaitTimeoutSecs = 60,
                    MaximumQualityOfService = MqttQualityOfService.ExactlyOnce,
                    AllowWildcardsInTopicFilters = true
                };

                await Client.ConnectAsync(App.Config.IP, configuration, App.Config.User);
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
