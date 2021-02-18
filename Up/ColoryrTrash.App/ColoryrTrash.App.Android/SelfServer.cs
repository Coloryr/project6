using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(ColoryrTrash.App.Droid.SelfServer))]
namespace ColoryrTrash.App.Droid
{
    public class SelfBinder : Binder
    {
        public SelfBinder(SelfServer service)
        {
            Service = service;
        }

        public SelfServer Service { get; private set; }
    }
    [Service]
    public class SelfServer : Service, ISelfMqtt
    {
        public IBinder Binder { get; private set; }
        private static MqttClient Client;
        public override IBinder OnBind(Intent intent)
        {
            Binder = new SelfBinder(this);
            return Binder;
        }
        public override void OnCreate()
        {
            Client = new MqttFactory().CreateMqttClient() as MqttClient;
            Client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(MqttUtils.OnMqttClientConnected);
            Client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(MqttUtils.OnMqttClientDisConnected);
            Client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(MqttUtils.OnSubscriberMessageReceived);
        }

        public bool IsConnected()
        {
            return Client.IsConnected;
        }

        public Task PublishAsync(MqttApplicationMessage message)
        {
            return Client.PublishAsync(message);
        }

        public Task ConnectAsync(MqttClientOptions options)
        {
            return Client.ConnectAsync(options);
        }

        public Task SubscribeAsync(string topic)
        {
            return Client.SubscribeAsync(topic);
        }

        public Task DisconnectAsync()
        {
            return Client.DisconnectAsync();
        }
    }
}