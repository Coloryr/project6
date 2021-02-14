using Lib;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;

namespace ColoryrTrash.Server.Mqtt
{
    class ThisMqttServer
    {
        public static MqttServer Server { get; private set; }
        public static bool IsRun { get; set; }//是否在运行

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

        public static async Task PublishAsync(MqttApplicationMessage message)
        {
            await Server.PublishAsync(message);
        }

        private static void OnMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                if (arg.ApplicationMessage.Topic.StartsWith(DataArg.TopicDesktopClient))
                {
                    DesktopServer.DesktopReceived(arg.ClientId, arg.ApplicationMessage.Payload);
                }
                else if (arg.ApplicationMessage.Topic.StartsWith(DataArg.TopicAppClient))
                {

                }
                else if (arg.ApplicationMessage.Topic.StartsWith(DataArg.TopicTrashClient))
                {

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
    }
}
