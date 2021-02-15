using Lib;
using MQTTnet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Server.Mqtt
{
    class AppServer
    {
        public static async void SendAll(object data)
        {
            var temp = JsonConvert.SerializeObject(data);
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicAppServer,
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
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await ThisMqttServer.PublishAsync(message);
        }
        public static void DesktopReceived(string ClientId, byte[] Payload)
        {
            string data = Encoding.UTF8.GetString(Payload);
            var obj = JsonConvert.DeserializeObject<DataPackObj>(data);
            string User = ClientId;
            if (User == null)
                return;
        }
    }
}
