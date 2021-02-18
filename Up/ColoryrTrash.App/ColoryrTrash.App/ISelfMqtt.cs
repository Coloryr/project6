using System;
using System.Collections.Generic;
using System.Net.Mqtt;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.App
{
    public interface ISelfMqtt
    { 
        bool IsConnected();
        Task PublishAsync(MqttApplicationMessage message);
        Task<SessionState> ConnectAsync(string host, MqttConfiguration options, string id);
        Task SubscribeAsync(string topic);
        Task DisconnectAsync();
    }
}
