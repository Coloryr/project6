using Lib;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Desktop
{
    public class MqttUtils
    {
        private readonly MqttClient Client;
        private string SelfServerTopic;
        private string SelfClientTopic;
        private bool IsConnecting;

        public bool isok;
        public MqttUtils()
        {
            try
            {
                Client = new MqttFactory().CreateMqttClient() as MqttClient;
                Client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnMqttClientConnected);
                Client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnMqttClientDisConnected);
                Client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnSubscriberMessageReceived);
                isok = true;
            }
            catch
            {
                isok = false;
            }
        }

        private void OnSubscriberMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                if (arg.ApplicationMessage.Topic == SelfServerTopic)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    switch (obj.Type)
                    {
                        case DataType.CheckLogin:
                            if (obj.Res == false)
                            {
                                App.ShowB("自动登录", "自动登录失败");
                            }
                            else
                            {
                                App.Start();
                                App.IsLogin = true;
                                App.LoginWindows.LoginClose();
                                App.ShowB("自动登录", "已自动登录");
                            }
                            break;
                        case DataType.Login:
                            if (obj.Res)
                            {
                                App.Config.Token = obj.Data as string;
                                App.Save();
                                App.Start();
                                App.IsLogin = true;
                                App.LoginWindows.LoginClose();
                                App.ShowA("登录", "登录成功");
                            }
                            else
                            {
                                App.IsLogin = false;
                                App.ShowB("登录", obj.Data as string);
                            }
                            break;
                        case DataType.GetGroups:
                            if (obj.Res == true)
                            {
                                var list = JsonConvert.DeserializeObject<List<string>>(obj.Data as string);
                                App.ListWindows_?.SetList(list);
                            }
                            else 
                            {
                                App.IsLogin = false;
                                App.ShowB("获取群组错误", obj.Data as string);
                                App.Login();
                            }
                            break;
                        case DataType.GetGroupInfo:
                            if (obj.Res == true)
                            {
                                if (obj.Data == null)
                                {
                                    App.ShowB("获取群组内容错误", obj.Data1 as string);
                                }
                                else
                                {
                                    var list = JsonConvert.DeserializeObject<DataSaveObj>(obj.Data as string);
                                    App.ListWindows_?.SetInfo(list);
                                }
                            }
                            else
                            {
                                App.IsLogin = false;
                                App.ShowB("获取群组错误", obj.Data as string);
                                App.Login();
                            }
                            break;
                    }
                }
                else if (arg.ApplicationMessage.Topic == DataArg.TopicServer)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    switch (obj.Type)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        private void OnMqttClientDisConnected(MqttClientDisconnectedEventArgs arg)
        {
            App.Log("服务器连接断开");
            if (!IsConnecting)
                App.DisConnect();
        }

        private void OnMqttClientConnected(MqttClientConnectedEventArgs arg)
        {
            App.Log("服务器已连接");
        }

        private void Send(string message)
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

        public bool Check()
        {
            return Client.IsConnected;
        }

        public async Task<bool> Start()
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
                SelfServerTopic = DataArg.TopicServer + "/" + App.Config.User;
                SelfClientTopic = DataArg.TopicClient + "/" + App.Config.User;
                await Client.SubscribeAsync(SelfServerTopic);
                await Client.SubscribeAsync(DataArg.TopicServer);
                IsConnecting = false;
                return true;
            }
            catch (Exception e)
            {
                App.LogError(e);
            }
            return false;
        }

        public async Task Stop()
        {
            try
            {
                await Client.DisconnectAsync();
            }
            catch (Exception e)
            {
                App.LogError(e);
            }
        }

        public void CheckLogin(string token)
        {
            var obj = new DataPackObj
            {
                Token = token,
                Type = DataType.CheckLogin
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public void Login(string pass)
        {
            var obj = new DataPackObj
            {
                Type = DataType.Login,
                Data = Tools.GenSHA1(pass)
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public void GetGroups()
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.GetGroups
            };
            Send(JsonConvert.SerializeObject(obj));
        }
        public void GetGroupInfo(string group)
        {
            var obj = new DataPackObj
            {
                Token = App.Config.Token,
                Type = DataType.GetGroupInfo,
                Data = group
            };
            Send(JsonConvert.SerializeObject(obj));
        }
    }
}
