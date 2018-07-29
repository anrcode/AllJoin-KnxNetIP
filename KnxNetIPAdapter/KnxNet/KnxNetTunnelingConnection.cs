using System;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking;
using Spark.Universal.Net;
using KnxNetIPAdapter.KnxNet.DPT;


namespace KnxNetIPAdapter.KnxNet
{
    internal class KnxNetTunnelingConnection
    {
        private IPEndPoint _localEndpoint;
        private UdpClient _udpClient;

        private string _host;
        private int _port;
        private bool _isConnected = false;

        private byte _messageCode = 0;
        private byte _channelId = 0;
        private byte _sequenceNo = 0;
        private byte _rxSequenceNo = 0;
        private bool _threeLevelGroupAssigning = true;

        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<KnxEventArgs> KnxEvent;
        public event EventHandler<KnxEventArgs> KnxStatus;

        public KnxNetTunnelingConnection()
        {
            var localIP = UdpClient.GetLocalIPAddress();
            _localEndpoint = new IPEndPoint(IPAddress.Parse(localIP), 3671);
        }

        public void Connect(string host, int port = 3671)
        {
            _host = host;
            _port = port;

            _udpClient = new UdpClient();
            _udpClient.DataReceived += UdpClientDataReceived;
            _udpClient.BindSocket(port.ToString());
            _udpClient.Connect(new HostName(host), port.ToString());

            _isConnected = true;

            this.SendConnectRequest();

            // enable state requests, if connected
            Task.Run(async () => {
                while (_isConnected)
                {
                    await Task.Delay(30000);
                    this.SendStateRequest();
                }
            });
        }

        public void Disconnect()
        {
            _isConnected = false;

            this.SendDisconnectRequest();
            this.Disconnected?.Invoke(this, EventArgs.Empty);

            _udpClient?.Dispose();
            _udpClient = null;
        }

        /// <summary>
        ///     Send a byte array value as data to specified address
        /// </summary>
        /// <param name="address">KNX Address</param>
        /// <param name="data">Byte array value</param>
        public async void Action(string address, string type, object value)
        {
            byte[] asdu = DataPointTranslator.Instance.ToASDU(type, value);
            var cemi = KnxCEMI.CreateActionCEMI(_messageCode, address, asdu);
            var cemiBytes = cemi.ToBytes();

            // header
            var header = new byte[10];
            header[00] = 0x06; /* 06 - Header Length */
            header[01] = 0x10; /* 10 - KNXnet version (1.0) */
            header[02] = 0x04; /* 04 - hi-byte Service type descriptor (TUNNELLING_REQUEST) */
            header[03] = 0x20; /* 20 - lo-byte Service type descriptor (TUNNELLING_REQUEST) */
            var totalLength = BitConverter.GetBytes(header.Length + cemiBytes.Length);
            header[04] = totalLength[1];
            header[05] = totalLength[0];
            
            /* Connection Header (4 Bytes) */
            header[06] = 0x04; /* 04 - Structure length */
            header[07] = _channelId;
            header[08] = _sequenceNo++;
            header[09] = 0x00; /* 00 - Reserved */        

            var datagram = new byte[header.Length + cemiBytes.Length];
            Array.Copy(header, datagram, header.Length);
            Array.Copy(cemiBytes, 0, datagram, header.Length, cemiBytes.Length);

            await _udpClient?.Send(datagram);
        }

        // TODO: It would be good to make a type for address, to make sure not any random string can be passed in
        /// <summary>
        ///     Send a request to KNX asking for specified address current status
        /// </summary>
        /// <param name="address"></param>
        public async void RequestStatus(string address)
        {
            var cemi = KnxCEMI.CreateStatusCEMI(_messageCode, address);
            var cemiBytes = cemi.ToBytes();

            // header
            var header = new byte[10];
            header[00] = 0x06; /* 06 - Header Length */
            header[01] = 0x10; /* 10 - KNXnet version (1.0) */
            header[02] = 0x04; /* 04 - hi-byte Service type descriptor (TUNNELLING_REQUEST) */
            header[03] = 0x20; /* 20 - lo-byte Service type descriptor (TUNNELLING_REQUEST) */
            var totalLength = BitConverter.GetBytes(header.Length + cemiBytes.Length);
            header[04] = totalLength[1];
            header[05] = totalLength[0];
            
            /* Connection Header (4 Bytes) */
            header[06] = 0x04; /* 04 - Structure length */
            header[07] = _channelId;
            header[08] = _sequenceNo++;
            header[09] = 0x00; /* 00 - Reserved */       

            var datagram = new byte[header.Length + cemiBytes.Length];
            Array.Copy(header, datagram, header.Length);
            Array.Copy(cemiBytes, 0, datagram, header.Length, cemiBytes.Length);

            await _udpClient?.Send(datagram);
        }

        private void UdpClientDataReceived(object sender, Spark.Universal.Net.DataReceivedEventArgs e)
        {
            // parse datagram 
            byte[] datagram = e.Data;
            var tt = KnxNetTunnelingDatagram.FromBytes(e.Data);

            switch (tt.service_type)
            {
                case (ushort)KnxHelper.SERVICE_TYPE.CONNECT_RESPONSE:
                    this.ProcessConnectResponse(datagram);
                    break;
                case (ushort)KnxHelper.SERVICE_TYPE.CONNECTIONSTATE_RESPONSE:
                    this.ProcessStateResponse(datagram);
                    break;
                case (ushort)KnxHelper.SERVICE_TYPE.TUNNELLING_ACK:
                    this.ProcessTunnelingAck(datagram);
                    break;
                case (ushort)KnxHelper.SERVICE_TYPE.DISCONNECT_REQUEST:
                    this.ProcessDisconnectRequest(datagram);
                    break;
                case (ushort)KnxHelper.SERVICE_TYPE.TUNNELLING_REQUEST:
                    ProcessDatagramHeaders(datagram);
                    break;
            }
        }

        private async void SendConnectRequest()
        {
            /* header */
            var datagram = new byte[26];
            datagram[00] = 0x06; /* 06 - Header Length */
            datagram[01] = 0x10; /* 10 - KNXnet version (1.0) */
            datagram[02] = 0x02; /* 02 - hi-byte Service type descriptor (CONNECTION_REQUEST) */
            datagram[03] = 0x05; /* 05 - lo-byte Service type descriptor (CONNECTION_REQUEST) */
            datagram[04] = 0x00; /* 00 - hi-byte total length */
            datagram[05] = 0x1A; /* 1A - lo-byte total lengt 26 bytes */

            /* Connection HPAI */
            datagram[06] = 0x08; /* 08 - Host Protocol Address Information (HPAI) Lenght */
            datagram[07] = 0x01; /* 01 - Host Protocol Address Information (HPAI) Lenght */
            datagram[08] = _localEndpoint.Address.GetAddressBytes()[0];
            datagram[09] = _localEndpoint.Address.GetAddressBytes()[1];
            datagram[10] = _localEndpoint.Address.GetAddressBytes()[2];
            datagram[11] = _localEndpoint.Address.GetAddressBytes()[3];
            datagram[12] = (byte)(_localEndpoint.Port >> 8);
            datagram[13] = (byte)_localEndpoint.Port;
            
            /* Tunnelling HPAI */
            datagram[14] = 0x08; /* 08 - Host Protocol Address Information (HPAI) Lenght */
            datagram[15] = 0x01; /* 01 - Host Protocol Address Information (HPAI) Lenght */
            datagram[16] = _localEndpoint.Address.GetAddressBytes()[0];
            datagram[17] = _localEndpoint.Address.GetAddressBytes()[1];
            datagram[18] = _localEndpoint.Address.GetAddressBytes()[2];
            datagram[19] = _localEndpoint.Address.GetAddressBytes()[3];
            datagram[20] = (byte)(_localEndpoint.Port >> 8);
            datagram[21] = (byte)_localEndpoint.Port;
            
            /* CRI */
            datagram[22] = 0x04; /* structure len (4 bytes) */
            datagram[23] = 0x04; /* Tunnel Connection */
            datagram[24] = 0x02; /* KNX Layer (Tunnel Link Layer) */
            datagram[25] = 0x00; /* Reserved */

            await _udpClient?.Send(datagram);
        }

        private void ProcessConnectResponse(byte[] datagram)
        {
            // HEADER
            var knxDatagram = KnxNetTunnelingDatagram.FromBytes(datagram);
            if (knxDatagram == null) return;

            if (knxDatagram.channel_id == 0x00 && knxDatagram.status == 0x24) /* TODO: status should be 0 */
            {
                // no more connections available
                return;
            }

            _channelId = knxDatagram.channel_id;
            _sequenceNo = 0;

            this.Connected?.Invoke(this, EventArgs.Empty);
        }

        private async void SendDisconnectRequest()
        {
            // header
            var datagram = new byte[16];
            datagram[00] = 0x06; /* 06 - Header Length */
            datagram[01] = 0x10; /* 10 - KNXnet version (1.0) */
            datagram[02] = 0x02; /* 02 - hi-byte Service type descriptor (DISCONNECT_REQUEST) */
            datagram[03] = 0x09; /* 09 - lo-byte Service type descriptor (DISCONNECT_REQUEST) */
            datagram[04] = 0x00; /* 00 - hi-byte total length */
            datagram[05] = 0x10; /* 10 - lo-byte total lengt 16 Bytes */
            
            /* data (10 Bytes) */
            datagram[06] = _channelId;
            datagram[07] = 0x00;
            datagram[08] = 0x08; /* 08 - Host Protocol Address Information (HPAI) Lenght */
            datagram[09] = 0x01; /* 01 - Host Protocol Code 0x01 -> IPV4_UDP, 0x02 -> IPV6_TCP */
            datagram[10] = _localEndpoint.Address.GetAddressBytes()[0];
            datagram[11] = _localEndpoint.Address.GetAddressBytes()[1];
            datagram[12] = _localEndpoint.Address.GetAddressBytes()[2];
            datagram[13] = _localEndpoint.Address.GetAddressBytes()[3];
            datagram[14] = (byte)(_localEndpoint.Port >> 8);
            datagram[15] = (byte)_localEndpoint.Port;

            await _udpClient?.Send(datagram); // multiple??
        }

        private void ProcessDisconnectRequest(byte[] datagram)
        {
            var knxDatagram = KnxNetTunnelingDatagram.FromBytes(datagram);
            if (knxDatagram == null) return;

            if (knxDatagram.channel_id != _channelId)
            {
                return;
            }

            this.Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private async void SendStateRequest()
        {
            // header
            var datagram = new byte[16];
            datagram[00] = 0x06; /* 06 - Header Length */
            datagram[01] = 0x10; /* 10 - KNXnet version (1.0) */
            datagram[02] = 0x02; /* 02 - hi-byte Service type descriptor (CONNECTIONSTATE_REQUEST) */
            datagram[03] = 0x07; /* 07 - lo-byte Service type descriptor (CONNECTIONSTATE_REQUEST) */
            datagram[04] = 0x00; /* 00 - hi-byte total length */
            datagram[05] = 0x10; /* 10 - lo-byte total lengt 16 bytes */
            
            /* Connection HAPI (10 Bytes) */
            datagram[06] = _channelId;
            datagram[07] = 0x00; /* 00 - */
            datagram[08] = 0x08; /* 08 - Host Protocol Address Information (HPAI) Lenght */
            datagram[09] = 0x01; /* 01 - Host Protocol Code 0x01 -> IPV4_UDP, 0x02 -> IPV6_TCP */
            datagram[10] = _localEndpoint.Address.GetAddressBytes()[0];
            datagram[11] = _localEndpoint.Address.GetAddressBytes()[1];
            datagram[12] = _localEndpoint.Address.GetAddressBytes()[2];
            datagram[13] = _localEndpoint.Address.GetAddressBytes()[3];
            datagram[14] = (byte)(_localEndpoint.Port >> 8);
            datagram[15] = (byte)_localEndpoint.Port;

            await _udpClient?.Send(datagram); // multiple
        }

        private void ProcessStateResponse(byte[] datagram)
        {
            var knxDatagram = KnxNetTunnelingDatagram.FromBytes(datagram);
            if (knxDatagram == null) return;

            if (knxDatagram.status != 0x21) return; /* TODO: status should be 0 */

            this.Disconnect();
        }

        public async void SendTunnelingAck(byte sequenceNumber)
        {
            // header
            var datagram = new byte[10];
            datagram[00] = 0x06; /* 06 - Header Length */
            datagram[01] = 0x10; /* 10 - KNXnet version (1.0) */
            datagram[02] = 0x04; /* 04 - hi-byte Service type descriptor (TUNNELLING_ACK) */
            datagram[03] = 0x21; /* 21 - lo-byte Service type descriptor (TUNNELLING_ACK) */
            datagram[04] = 0x00; /* 00 - hi-byte total length */
            datagram[05] = 0x0A; /* 0A - lo-byte total lengt 10 bytes */
            
            /* ConnectionHeader (4 Bytes) */
            datagram[06] = 0x04; /* 04 - Structure length */
            datagram[07] = _channelId;
            datagram[08] = sequenceNumber;
            datagram[09] = 0x00; /* 00 our error code */

            await _udpClient?.Send(datagram);
        }

        private void ProcessDatagramHeaders(byte[] datagram)
        {
            // HEADER
            // TODO: Might be interesting to take out these magic numbers for the datagram indices
            var knxDatagram = new KnxNetTunnelingDatagram
            {
                header_length = datagram[0],
                protocol_version = datagram[1],
                service_type = (ushort)((datagram[2] << 8) + datagram[3]),
                total_length = datagram[4] + datagram[5]
            };

            var channelId = datagram[7];
            if (channelId != _channelId) return;

            var sequenceNumber = datagram[8];
            var process = sequenceNumber > _rxSequenceNo;
            _rxSequenceNo = sequenceNumber;

            if (process)
            {
                // TODO: Magic number 10, what is it?
                var cemiBytes = new byte[datagram.Length - 10];
                Array.Copy(datagram, 10, cemiBytes, 0, datagram.Length - 10);

                var knxCEMI = KnxCEMI.FromBytes(_threeLevelGroupAssigning, cemiBytes);
                if (knxCEMI.IsEvent)
                {
                    Debug.WriteLine("KNX Event " + knxCEMI.destination_address + "  " + BitConverter.ToString(knxCEMI.apdu));

                    this.KnxEvent?.Invoke(this, new KnxEventArgs()
                    {
                        Address = knxCEMI.destination_address,
                        Data = knxCEMI.apdu
                    });
                }
                else if (knxCEMI.IsStatus)
                {
                    Debug.WriteLine("KNX Status " + knxCEMI.destination_address + "  " + BitConverter.ToString(knxCEMI.apdu));

                    this.KnxStatus?.Invoke(this, new KnxEventArgs()
                    {
                        Address = knxCEMI.destination_address,
                        Data = knxCEMI.apdu
                    });
                }
            }

            this.SendTunnelingAck(sequenceNumber);
        }

        private void ProcessTunnelingAck(byte[] datagram)
        {
            // do nothing
            // byte no. 10 is the status/error number, zero is perfect!
        }
    }
}
