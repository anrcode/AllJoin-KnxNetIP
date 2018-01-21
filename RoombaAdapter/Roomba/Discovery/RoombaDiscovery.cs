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

        protected override void Discover()
        {
            var udpClient = new UdpClient();
            udpClient.DataReceived += SocketDataReceived;

            Task.Run(async () =>
            {
                await udpClient.Send(UdpClient.BROADCAST_ADDR, "5678", Encoding.UTF8.GetBytes("irobotmcs"));
                await Task.Delay(5000);

                udpClient.DataReceived -= SocketDataReceived;
                udpClient.Dispose();
                udpClient = null;
            });
        }

        private void SocketDataReceived(object sender, DataReceivedEventArgs e)
        {
            string response = Encoding.UTF8.GetString(e.Data);
            var device = JsonObject.Parse(response);

            var hostName = device.GetNamedString("hostname");
            if(hostName.Split('-')[0] != "Roomba")
            {
                return;
            }

            var macAddr = device.GetNamedString("mac");
            string mac = macAddr.Replace(":", "");
            if (this.AlreadyDiscovered(mac))
            {
                return;
            }

            var blid = hostName.Split('-')[1];
            var pass = ":1:1515862749:YYxDNy5nydFOrI88";
            var conn = new RoombaClient(e.RemoteAddress, "8883", blid, pass);
            conn.ConnectionLost += (object s, EventArgs args) =>
            {
                this.RemoveDevice(mac);
            };
            this.AddDevice(mac, conn);         
        }
    }
}
