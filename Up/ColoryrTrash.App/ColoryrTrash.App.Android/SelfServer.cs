using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mqtt;
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
    //[Service(Name = "ColoryrTrash.App.SelfServer",
    // Process = ":self",
    // Exported = true)]
    [Service]
    public class SelfServer : Service, ISelfMqtt
    {
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
        public IBinder Binder { get; private set; }
        private IMqttClient Client;
        public override IBinder OnBind(Intent intent)
        {
            Binder = new SelfBinder(this);
            return Binder;
        }

        public override void OnCreate()
        {

        }

        public bool IsConnected()
        {
            return Client?.IsConnected == true;
        }

        public Task PublishAsync(MqttApplicationMessage message)
        {
            return Client.PublishAsync(message, MqttQualityOfService.ExactlyOnce);
        }

        public async Task<SessionState> ConnectAsync(string host, MqttConfiguration options, string id)
        {
            if (Client != null)
            {
                Client.Dispose();
            }
            Client = await MqttClient.CreateAsync(host, options);
            Client.MessageStream.Subscribe(MqttUtils.OnSubscriberMessageReceived);
            Client.Disconnected += MqttUtils.OnMqttClientDisConnected;
            return await Client.ConnectAsync(new MqttClientCredentials(id, id, ""), cleanSession: true);
        }

        public  Task SubscribeAsync(string topic)
        {
            return Client.SubscribeAsync(topic, MqttQualityOfService.ExactlyOnce);
        }

        public Task DisconnectAsync()
        {
            return Client.DisconnectAsync();
        }
    }
}