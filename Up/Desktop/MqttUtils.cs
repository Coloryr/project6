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

namespace Desktop
{
    public class MqttUtils
    {
        private readonly MqttClient Client;
        private string SelfTopic;

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
                if (arg.ApplicationMessage.Topic == SelfTopic)
                {
                    string Message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    var obj = JsonConvert.DeserializeObject<DataPackObj>(Message);
                    switch (obj.Type)
                    {
                        case DataType.CheckLogin:
                            if (obj.Res == false)
                            {
                                App.Log("自动登录失败");
                            }
                            break;
                        case DataType.Login:
                            if (obj.Res)
                            {
                                App.Config.Token = (string)obj.Data;
                                App.Save();
                            }
                            App.LoginWindows?.LoginRes(obj.Res);
                            break;
                    }
                }
                else if(arg.ApplicationMessage.Topic == DataArg.Topic)
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
                    .WithTopic(SelfTopic)
                    .WithPayload(Encoding.UTF8.GetBytes(message))
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();
                Client.PublishAsync(obj);
            }
        }

        private bool Check()
        {
            return Client.IsConnected;
        }

        public async Task<bool> Start()
        {
            try
            {
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

                await Client.ConnectAsync(options);
                SelfTopic = DataArg.Topic + "/" + App.Config.User;
                await Client.SubscribeAsync(SelfTopic);
                await Client.SubscribeAsync(DataArg.Topic);
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

        public void CheckLogin(string user, string token)
        {
            var obj = new DataPackObj
            {
                User = user,
                Token = token,
                Type = DataType.CheckLogin
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public void Login(string user, string pass)
        {
            var obj = new DataPackObj
            {
                User = user,
                Type = DataType.Login,
                Data = pass
            };
            Send(JsonConvert.SerializeObject(obj));
        }

        public Dictionary<string, DataSaveObj> GetGroups()
        {
            return null;
        }
    }
}
