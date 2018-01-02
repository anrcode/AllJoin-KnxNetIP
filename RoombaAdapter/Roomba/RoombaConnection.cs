using System;
using System.Text;
using Windows.Networking;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet;
using Windows.Data.Json;

namespace RoombaAdapter.Roomba
{
    internal class RoombaConnection
    {
        private HostName _hostname;
        private string _gatewayMac;
        private IManagedMqttClient _client = null;

        public event TransitionEventHandler OnTransition;
        public event EventHandler OnError;


        public RoombaConnection(HostName hostname, string gatewayMac)
        {
            _hostname = hostname;
            _gatewayMac = gatewayMac;

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Client1")
                    .WithTcpServer("broker.hivemq.com", 8883)
                    .WithCredentials("username", "password")
                    .WithTls().Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            _client.StartAsync(options);
            _client.ApplicationMessageReceived += Device_ApplicationMessageReceived;
        }

        private void Device_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            string response = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var jsonMsg = JsonObject.Parse(response);
        }

        public void Connect()
        {       
            
        }

        public async void SendMessage(MCP msg)
        {
            // see: https://github.com/koalazak/dorita980/blob/master/lib/v2/local.js
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("MyTopic")
                .WithPayload("Hello World")
                .Build();

            await _client.PublishAsync(message);    
        }
    }
}
