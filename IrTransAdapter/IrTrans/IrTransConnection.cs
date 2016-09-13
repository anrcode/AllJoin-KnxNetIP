using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Spark.Universal.Net;


namespace IrTransAdapter.IrTrans
{
    internal class IrTransConnection
    {
        private HostName _hostname;
        private TcpClient _connection = new TcpClient();
        private bool _isConnected = false;

        public IrTransConnection(HostName hostname)
        {
            _hostname = hostname;

            _connection.Error += (object sender, EventArgs e) =>
            {

            };
        }

        public void Connect()
        {
            _connection.Connect(_hostname, "21000");
            _isConnected = true;
        }

        public void Disconnect()
        {
            _connection.Disconnect();
            _isConnected = false;
        }

        private void EnsureConnected()
        {
            if (_isConnected) return;

        }

        public async void GetVersion()
        {
            this.EnsureConnected();

            byte[] verCmd = Encoding.ASCII.GetBytes("Aver\n");
            await _connection.Send(verCmd);
            string response = await this.ExpectResponse("VERSION");
        }

        public async void SendCommand(string remote, string command)
        {
            this.EnsureConnected();

            byte[] sndCmd = Encoding.ASCII.GetBytes(string.Format("Asnd {0},{1}\n", remote, command));
            await _connection.Send(sndCmd);
            string response = await this.ExpectResponse("RESULT");
        }

        public async void SendCommandHex(string command)
        {
            this.EnsureConnected();

            byte[] sndHexCmd = Encoding.ASCII.GetBytes(string.Format("Asndhex LB H{0}\n", command));
            await _connection.Send(sndHexCmd);
            string response = await this.ExpectResponse("RESULT");
        }

        public async Task<string> LearnCommand()
        {
            this.EnsureConnected();

            byte[] learnCmd = Encoding.ASCII.GetBytes("Alearn\n");
            await _connection.Send(learnCmd);
            string response = await this.ExpectResponse("LEARN");
            return response;
        }

        private async Task<string> ExpectResponse(string command)
        {
            byte[] data = await this._connection.Read(2048);
            if ((data == null) || (data.Length < 8))
            {
                throw new Exception("invalid response - data length");
            }

            var response = Encoding.ASCII.GetString(data);
            if(!response.StartsWith("**"))
            {
                throw new Exception("invalid response - header");
            }

            int length = 0;
            if(!int.TryParse(response.Substring(3, 5), out length))
            {
                throw new Exception("invalid response - length format");
            }

            if(length != data.Length)
            {
                throw new Exception("invalid response - length");
            }

            if(response.IndexOf(command) != 8)
            {
                throw new Exception("invalid response - command mismatch");
            }

            return response.Substring(response.IndexOf(' ', 9)).TrimEnd('\n');
        }
    }
}
