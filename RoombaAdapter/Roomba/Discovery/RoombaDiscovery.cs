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

            Task.Delay(50000);
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

            var mac = device.GetNamedString("mac");
            if (this.AlreadyDiscovered(mac))
            {
                return;
            }

            var blid = hostName.Split('-')[1];
            var pass = ":1:1515862749:YYxDNy5nydFOrI88";
            var conn = new RoombaConnection(e.RemoteAddress, blid, pass);
            conn.ConnectionLost += (object s, EventArgs args) =>
             {
                 this.RemoveDevice(mac);
             };
            conn.Connect();
            this.AddDevice(mac, conn);         
        }
    }
}
