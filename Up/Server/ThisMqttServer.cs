using MQTTnet;
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
        private static MqttServer server;
        public static void Start()
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
   .WithDefaultEndpoint().WithDefaultEndpointPort(ServerMain.Config.MQTT.Port).WithConnectionValidator(
       c =>
       {
           var currentUser = config["Users"][0]["UserName"].ToString();
           var currentPWD = config["Users"][0]["Password"].ToString();

           if (currentUser == null || currentPWD == null)
           {
               c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
               return;
           }

           if (c.Username != currentUser)
           {
               c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
               return;
           }

           if (c.Password != currentPWD)
           {
               c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
               return;
           }

           c.ReasonCode = MqttConnectReasonCode.Success;
       }).WithSubscriptionInterceptor(
       c =>
       {
           c.AcceptSubscription = true;
       }).WithApplicationMessageInterceptor(
       c =>
       {
           c.AcceptPublish = true;
       });
            server = new MqttFactory().CreateMqttServer() as MqttServer;
        }
        public static void Stop()
        { 
            
        }
    }
}
