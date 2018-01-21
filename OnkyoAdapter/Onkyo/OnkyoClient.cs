using System;
using Spark.Universal.Net;
using System.Threading.Tasks;
using System.Linq;
using Windows.Networking;

namespace OnkyoAdapter.Onkyo
{
    internal class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string psMessage)
        {
            this.Message = psMessage;
        }
        public string Message { get; private set; }
    }

    internal class OnkyoClient
    {
        private HostName _remoteHost;
        private string _remoteService;
        private TcpClient _socketClient = new TcpClient();

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler ConnectionLost;

        #region Public Methods / Properties

        public OnkyoClient(HostName remoteHost, string remoteService)
        {
            _remoteHost = remoteHost;
            _remoteService = remoteService;

            _socketClient.Connected += (object sender, EventArgs e) => {
                this.Connected?.Invoke(this, EventArgs.Empty);
            };

            _socketClient.Disconnected += (object sender, EventArgs e) => {
                this.Disconnected?.Invoke(this, EventArgs.Empty);
            };

            _socketClient.Error += (object sender, EventArgs e) => {
                this.ConnectionLost?.Invoke(this, EventArgs.Empty);
            };
        }

        public void Connect()
        {
            this.Disconnect();

            this._socketClient.Connect(_remoteHost, _remoteService);

            Task.Run(this.SocketListener);
        }

        public void Disconnect()
        {
            this._socketClient.Disconnect();
        }

        public void SendCommand(Command.CommandBase poCommand)
        {
            this.SendPackage(poCommand.CommandMessage);
        }

        private async void SendPackage(string psMessage)
        {
            var loPackage = psMessage.ToISCPCommandMessage();
            await this._socketClient.Send(loPackage);
        }

        private async Task SocketListener()
        {
            byte[] loNotProcessingBytes = null;
            byte[] loResultBuffer = null;

            while (true)
            {
                byte[] data = await this._socketClient.Read(2048);

                if (loNotProcessingBytes != null && loNotProcessingBytes.Length > 0)
                {
                    loResultBuffer = loNotProcessingBytes.Concat(data).ToArray();
                }
                else
                {
                    loResultBuffer = data;
                }

                foreach (var lsMessage in loResultBuffer.ToISCPStatusMessage(out loNotProcessingBytes))
                {
                    if (lsMessage.IsNotEmpty())
                    {
                        if (this.MessageReceived != null)
                        {
                            this.MessageReceived(this, new MessageReceivedEventArgs(lsMessage));
                        }
                    }
                }
            }
        }

        #endregion
    }
}

