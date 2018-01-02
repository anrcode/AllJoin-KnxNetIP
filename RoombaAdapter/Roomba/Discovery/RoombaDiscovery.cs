using System;
using System.Threading.Tasks;
using System.Text;
using Spark.Universal.Net;
using Windows.Data.Json;

namespace RoombaAdapter.Roomba.Discovery
{
    internal class RoombaDiscovery : SparkAlljoyn.Discovery.AdapterDiscoveryBase
    {
        public RoombaDiscovery()
        {

        }

        private void SocketDataReceived(object sender, DataReceivedEventArgs e)
        {
            string response = Encoding.UTF8.GetString(e.Data);

            var device = JsonObject.Parse(response);

            var xmldoc = new System.Xml.XmlDocument();
            xmldoc.LoadXml(response);

            if (xmldoc.DocumentElement.Name != "LogicBox")
            {
                return;
            }

            var protoAttr = device["ver"].GetString();
            if ((protoAttr == null) || (protoAttr != "2"))
            {
                return;
            }

            var macAttr = device["mac"].GetString();
            if (macAttr == null)
            {
                return;
            }

            var hostName = e.RemoteAddress;
            string mac = macAttr.Replace(":", "");

            if (this.AlreadyDiscovered(mac))
            {
                return;
            }

            var conn = new RoombaConnection(e.RemoteAddress, mac);
            this.AddDevice(mac, conn);         
        }

        protected override void Discover()
        {
            var udpClient = new UdpClient();
            udpClient.DataReceived += SocketDataReceived;
            udpClient.BindSocket("5678");

            Task.Run(async () =>
            {
                await udpClient.Send(UdpClient.BROADCAST_ADDR, "5678", Encoding.UTF8.GetBytes("irobotmcs"));
                await Task.Delay(5000);

                udpClient.DataReceived -= SocketDataReceived;
                udpClient.Dispose();
                udpClient = null;
            });
        }
    }
}
