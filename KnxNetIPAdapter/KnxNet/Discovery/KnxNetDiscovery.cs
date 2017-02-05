using System;
using System.Threading.Tasks;
using System.Net;
using Windows.Networking;
using Spark.Universal.Net;
using SparkAlljoyn.Discovery;


namespace KnxNetIPAdapter.KnxNet.Discovery
{
    internal class KnxNetDiscovery : AdapterDiscoveryBase
    {
        private byte[] DISCOVER_REQUEST = new byte[]
            {
                0x06, 0x10, 0x02, 0x01, 0x00, 0x0E, 0x08, 0x01, 0xff, 0xff, 0xff, 0xff, 0x0e, 0x57
            };

        protected override void Discover()
        {
            var localIP = UdpClient.GetLocalIPAddress();
            var localEndpoint = new IPEndPoint(IPAddress.Parse(localIP), 3672);

            DISCOVER_REQUEST[8] = localEndpoint.Address.GetAddressBytes()[0];
            DISCOVER_REQUEST[9] = localEndpoint.Address.GetAddressBytes()[1];
            DISCOVER_REQUEST[10] = localEndpoint.Address.GetAddressBytes()[2];
            DISCOVER_REQUEST[11] = localEndpoint.Address.GetAddressBytes()[3];
            DISCOVER_REQUEST[12] = (byte)(localEndpoint.Port >> 8);
            DISCOVER_REQUEST[13] = (byte)localEndpoint.Port;

            var udpClient = new UdpClient();
            udpClient.BindSocket(localEndpoint.Port.ToString());
            udpClient.DataReceived += SocketDataReceived;
            
            Task.Run(async () =>
            {
                await udpClient.Send(new HostName("224.0.23.12"), "3671", DISCOVER_REQUEST);
                await Task.Delay(5000);

                udpClient.DataReceived -= SocketDataReceived;
                udpClient.Dispose();
            });
        }

        private void SocketDataReceived(object sender, DataReceivedEventArgs e)
        {
            if((e.Data == null) || (e.Data.Length < 14) || (e.Data[5] != e.Data.Length))
            {
                return;
            }

            string ip = e.Data[8] + "." + e.Data[9] + "." + e.Data[10] + "." + e.Data[11];
            string service = Convert.ToString((e.Data[12] << 8) + e.Data[13]);

            if (this.AlreadyDiscovered(ip))
            {
                return;
            }

            var conn = new KnxNetTunnelingConnection();
            conn.Connected += (object s, EventArgs args) =>
            {
                this.AddDevice(ip, conn);
            };
            conn.Disconnected += (object s, EventArgs args) =>
            {
                this.RemoveDevice(ip);
            };
            conn.Connect(ip);
        }
    }
}
