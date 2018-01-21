using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Networking;
using Spark.Universal.Net;


namespace HoermannAdapter.Hoermann
{
    internal class HoermannClient
    {
        private HostName _remoteHost;
        private string _remoteService;
        private string _gatewayMac;
        private TcpClient _client = new TcpClient();
        private string _clientId = "000000000000";
        private uint _token = 0;

        public event EventHandler OnConnected;
        public event EventHandler OnLogin;
        public event EventHandler OnLogout;
        public event JmcpEventHandler OnJmcpMessage;
        public event TransitionEventHandler OnTransition;
        public event EventHandler OnError;


        public HoermannClient(HostName remoteHost, string remoteService, string gatewayMac)
        {
            _remoteHost = remoteHost;
            _remoteService = remoteService;
            _gatewayMac = gatewayMac;
        }

        public void Connect()
        {       
            _client.Connect(_remoteHost, _remoteService);
            Task.Run(SocketReader);    
        }

        public async void SendMessage(MCP msg)
        {
            msg.Token = this._token;
            var tpMsg = this._clientId + this._gatewayMac + Helpers.ToHex(msg.ToByteArray());
            tpMsg = tpMsg.ToUpper();
            int checksum = 0;
            foreach(char c in tpMsg)
            {
                checksum += c;
            }
            checksum = checksum & 0xff;
            tpMsg += checksum.ToString("X2");

            var msgBytes = Encoding.UTF8.GetBytes(tpMsg);
            await _client.Send(msgBytes);        
        }

        private async Task SocketReader()
        {
            byte[] buffer = new byte[2048];
            int offset = 0;

            while(true)
            {
                try
                {
                    offset += await _client.Read(buffer, (uint)offset, (uint)(buffer.Length - offset));
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Ex " + ex.StackTrace);
                    throw ex;
                }
            
                if(offset < (MCP.ADDRESS_SIZE * 2) + MCP.LENGTH_SIZE)
                {
                    continue;                  
                }

                var tp = Encoding.UTF8.GetString(buffer, 0, offset);
                if (!tp.StartsWith(this._gatewayMac))
                {
                    //not good!
                    continue;
                }

                byte[] tpBytes = Helpers.HexToBytes(tp);
                int length = (tpBytes[12] << 8) + tpBytes[12 + 1];

                if (tpBytes.Length - 12 - length - 1 < 0)
                {
                    continue;                   
                }

                byte[] tpmsgbytes = new byte[length];
                Array.Copy(tpBytes, 12, tpmsgbytes, 0, length);
                var test = MCP.FromByteArray(tpmsgbytes);
                Array.Copy(buffer, (12 + length + 1) * 2, buffer, 0, offset - (12 + length + 1) * 2);
                offset = offset - (12 + length + 1) * 2;

                //
                if(test.Command == McpCommand.Login)
                {
                    // remember access token to be used on consecutive calls
                    this._token = BitConverter.ToUInt32(test.Payload, 1);

                    this.OnLogin?.Invoke(this, EventArgs.Empty);

                    //var msg = MCP.CreateJmcpGetValuesCmd();
                    //var msg = MCP.CreateGetGroupsCmd();
                    //var msg = MCP.CreateGetUsersCmd();
                    //var msg = MCP.CreateGetPortsCmd();
                    //var msg = MCP.CreateGetWifiStateCmd();
                    var msg = MCP.CreateGetTransitionCmd(0);
                    this.SendMessage(msg);
                }
                else if (test.Command == McpCommand.Logout)
                {
                    this.OnLogout?.Invoke(this, EventArgs.Empty);
                }
                else if(test.Command == McpCommand.Jmcp)
                {
                    this.OnJmcpMessage?.Invoke(this, new JmcpEventArgs(test));
                }
                else if (test.Command == McpCommand.HmGetTransition)
                {
                    this.OnTransition?.Invoke(this, new TransitionEventArgs(test));
                }
                else if (test.Command == McpCommand.Error)
                {
                    this.OnError?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
