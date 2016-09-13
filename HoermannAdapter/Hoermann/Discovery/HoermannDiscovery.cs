using System;
using System.Threading.Tasks;
using System.Text;
using Spark.Universal.Net;


namespace HoermannAdapter.Hoermann.Discovery
{
    internal class HoermannDiscovery : SparkAlljoyn.Discovery.AdapterDiscoveryBase
    {
        public HoermannDiscovery()
        {

        }

        private void SocketDataReceived(object sender, DataReceivedEventArgs e)
        {
            string response = Encoding.UTF8.GetString(e.Data);
            var xmldoc = new System.Xml.XmlDocument();
            xmldoc.LoadXml(response);

            if (xmldoc.DocumentElement.Name != "LogicBox")
            {
                return;
            }

            var protoAttr = xmldoc.DocumentElement.Attributes["protocol"];
            if ((protoAttr == null) || (protoAttr.Value != "MCP V3.0"))
            {
                return;
            }

            var macAttr = xmldoc.DocumentElement.Attributes["mac"];
            if (macAttr == null)
            {
                return;
            }

            var hostName = e.RemoteAddress;
            string mac = macAttr.Value.Replace(":", "");

            if (this.AlreadyDiscovered(mac))
            {
                return;
            }

            var conn = new HoermannConnection(e.RemoteAddress, mac);
            this.AddDevice(mac, conn);         
        }

        protected override void Discover()
        {
            var udpClient = new UdpClient();
            udpClient.DataReceived += SocketDataReceived;
            udpClient.BindSocket("4002");

            Task.Run(async () =>
            {
                await udpClient.Send(UdpClient.BROADCAST_ADDR, "4001", Encoding.UTF8.GetBytes("<Discover target=\"LogicBox\" />"));
                await Task.Delay(5000);

                udpClient.DataReceived -= SocketDataReceived;
                udpClient.Dispose();
                udpClient = null;
            });
        }
    }
}
