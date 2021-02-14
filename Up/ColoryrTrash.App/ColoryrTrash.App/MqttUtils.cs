using Lib;
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
        private readonly MqttClient Client;
        private string SelfServerTopic;
        private string SelfClientTopic;
        private bool IsConnecting;

        public string Token { get; set; }
        public MqttUtils()
        {
            Client = new MqttFactory().CreateMqttClient() as MqttClient;
            Client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnMqttClientConnected);
            Client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnMqttClientDisConnected);
            Client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnSubscriberMessageReceived);
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
                                App.Show("自动登录", "自动登录失败");
                            }
                            else
                            {
                                App.IsLogin = true;
                                App.Show("自动登录", "已自动登录");
                            }
                            break;
                        case DataType.Login:
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
                            break;
                    }
                }
                else if (arg.ApplicationMessage.Topic == DataArg.TopicServer)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    switch (obj.Type)
                    {
                        case DataType.Updata:
                            var obj1 = JsonConvert.DeserializeObject<ItemSaveObj>(obj.Data1);
                            //App.ListWindows_.Updata(obj.Data, obj1);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //App.LogError(ex);
            }
        }

        private void OnMqttClientDisConnected(MqttClientDisconnectedEventArgs arg)
        {
            App.Show("服务器", "服务器连接断开");
            if (!IsConnecting)
                App.LoginOut();
        }

        private void OnMqttClientConnected(MqttClientConnectedEventArgs arg)
        {
            App.Show("服务器", "服务器已连接");
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
                //App.LogError(e);
            }
            App.Show("服务器", "服务器连接失败");
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
                //App.LogError(e);
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
    }
}
