using MQTTnet;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.App
{
    public interface ISelfMqtt
    { 
        bool IsConnected();
        Task PublishAsync(MqttApplicationMessage message);
        Task ConnectAsync(MqttClientOptions options);
        Task SubscribeAsync(string topic);
        Task DisconnectAsync();
    }
}
