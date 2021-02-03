using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ThisMqttServer
    {
        private static MqttServer Server;
        public static async void Start()
        {
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
        }

        private static void OnMessageReceived(MqttApplicationMessageReceivedEventArgs obj)
        {
            
        }

        private static void OnUnsubscribedTopic(MqttServerClientUnsubscribedTopicEventArgs obj)
        {
            
        }

        private static void OnSubscribedTopic(MqttServerClientSubscribedTopicEventArgs obj)
        {
            
        }

        private static void OnDisconnected(MqttServerClientDisconnectedEventArgs obj)
        {
            
        }

        private static void OnConnected(MqttServerClientConnectedEventArgs obj)
        {
            
        }

        public static void Stop()
        {

        }

        public async void Send(string topic, string data)
        {
            var message = new MqttApplicationMessage()
            {
                Topic = topic,
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
    }
}
