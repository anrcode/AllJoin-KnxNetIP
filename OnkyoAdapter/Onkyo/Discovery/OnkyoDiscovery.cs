using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking;
using Spark.Universal.Net;


namespace OnkyoAdapter.Onkyo.Discovery
{
    internal class OnkyoDiscovery : SparkAlljoyn.Discovery.AdapterDiscoveryBase
    {
        protected override void Discover()
        {
            var udpClient = new UdpClient();
            udpClient.DataReceived += SocketDataReceived;

            Task.Run(async () =>
            {
                var loCommand = "!xECNQSTN".ToISCPCommandMessage(false);
                await udpClient.Send(UdpClient.MULTICAST_ADDR, "60128", loCommand);
                await Task.Delay(5000);

                udpClient.DataReceived -= SocketDataReceived;
                udpClient.Dispose();
            });
        }

        private void SocketDataReceived(object sender, DataReceivedEventArgs e)
        {
            List<DeviceInfo> loDeviceList = new List<DeviceInfo>();
            byte[] loDummyOut;
            foreach (var lsMessage in e.Data.ToISCPStatusMessage(out loDummyOut))
            {
                var loDevice = ExtractDevice(lsMessage, e.RemoteAddress);
                if (loDevice != null)
                {
                    loDeviceList.Add(loDevice);

                    if (this.AlreadyDiscovered(loDevice.MacAddress))
                    {
                        return;
                    }

                    var conn = new OnkyoClient(loDevice.HostName, loDevice.ServiceName);                
                    conn.ConnectionLost += (object s, EventArgs args) =>
                    {
                        this.RemoveDevice(loDevice.MacAddress);
                    };
 
                    this.AddDevice(loDevice.MacAddress, conn);
                }
            }
        }

        private static DeviceInfo ExtractDevice(string psMessage, HostName remoteHost)
        {
            var loMatch = Regex.Match(psMessage, @"!(?<category>\d)ECN(?<model>[^/]*)/(?<port>\d{5})/(?<area>\w{2})/(?<mac>.{0,12})");
            if (!loMatch.Success)
            {
                return null;
            }

            return new DeviceInfo()
            {
                Category = loMatch.Groups["category"].Value,
                Model = loMatch.Groups["model"].Value,
                ServiceName = loMatch.Groups["port"].Value,
                Area = loMatch.Groups["area"].Value,
                MacAddress = loMatch.Groups["mac"].Value,
                HostName = remoteHost
            };
        }
    }
}
