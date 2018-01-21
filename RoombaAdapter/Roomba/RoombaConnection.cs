using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using Windows.Data.Json;
using Windows.Networking;


namespace RoombaAdapter.Roomba
{
    internal class RoombaConnection
    {
        private HostName _hostname;
        private string _blid;
        private string _pass;
        private StringBuilder _log = new StringBuilder();

        private MqttClient _client = null;
        private RoombaState _state = new RoombaState();

        public event RoombaStateEventHandler StateChanged;
        public event EventHandler Disconnected;
        public event EventHandler ConnectionLost;


        public RoombaConnection(HostName hostname, string blid, string pass)
        {
            _hostname = hostname;
            _blid = blid;
            _pass = pass;           
        }

        public void Connect()
        {
            _client = new MqttClient(_hostname.CanonicalName, MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT, true, MqttSslProtocols.TLSv1_0, (a, b, c, d) => { return true; }, null);

            byte t = _client.Connect(_blid, _blid, _pass);
            _client.MqttMsgPublishReceived += Roomba_MessageReceived;
            _client.ConnectionClosed += Roomba_ConnectionClosed;
            ushort x3 = _client.Subscribe(new string[] { "#" }, new byte[] { 1 });

            var state = new JsonObject();
            state["binPause"] = JsonValue.CreateBooleanValue(true);
            //this.SetPreference(state);
        }

        public void Disconnect()
        {
            if(_client == null) return;

            try
            {
                _client.MqttMsgPublishReceived -= Roomba_MessageReceived;
                _client.ConnectionClosed -= Roomba_ConnectionClosed;
                _client.Disconnect();
            }
            catch(Exception) { }
            finally
            {
                _client = null;
            }
        }

        private void SendCmd(string command)
        {
            var jsonObject = new JsonObject();
            jsonObject["command"] = JsonValue.CreateStringValue(command); // start, pause, stop, resume, dock
            long secs = DateTimeOffset.Now.ToUnixTimeSeconds();
            jsonObject["time"] = JsonValue.CreateNumberValue(secs);
            jsonObject["initiator"] = JsonValue.CreateStringValue("localApp");
            string jsonString = jsonObject.Stringify();
            ushort t2 = _client.Publish("cmd", Encoding.UTF8.GetBytes(jsonString));
        }

        private void SetPreference(JsonObject state)
        {
            var jsonObject = new JsonObject();
            jsonObject["state"] = state;
            string jsonString = jsonObject.Stringify();
            ushort t2 = _client.Publish("delta", Encoding.UTF8.GetBytes(jsonString));
        }

        private void Roomba_MessageReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            string response = Encoding.UTF8.GetString(e.Message);
            var jsonMsg = JsonObject.Parse(response);

            var staterep = jsonMsg.GetNamedObject("state").GetNamedObject("reported");
            if(staterep.ContainsKey("country"))
            {
                _state.Country = staterep.GetNamedString("country");
            }

            if (staterep.ContainsKey("mapUploadAllowed"))
            {
                _state.MapUploadAllowed = staterep.GetNamedBoolean("mapUploadAllowed");
            }

            if (staterep.ContainsKey("batPct"))
            {
                _state.BatPct = (uint)staterep.GetNamedNumber("batPct");
            }

            if (staterep.ContainsKey("bin"))
            {
                var bin = staterep.GetNamedObject("bin");
                _state.Bin.Present = bin.GetNamedBoolean("present");
                _state.Bin.IsFull = bin.GetNamedBoolean("full");
            }

            if (staterep.ContainsKey("cleanMissionStatus"))
            {
                var bin = staterep.GetNamedObject("cleanMissionStatus");
                _state.CleanMission.Cycle = bin.GetNamedString("cycle");
                _state.CleanMission.Phase = bin.GetNamedString("phase");
                _state.CleanMission.Sqft = (uint)bin.GetNamedNumber("sqft");
            }

            if (staterep.ContainsKey("cap"))
            {
                var bin = staterep.GetNamedObject("cap");
                _state.Cap.Pose = (uint)bin.GetNamedNumber("pose");
                _state.Cap.Ota = (uint)bin.GetNamedNumber("ota");
                _state.Cap.MultiPass = (uint)bin.GetNamedNumber("multiPass");
                _state.Cap.CarpetBoost = (uint)bin.GetNamedNumber("carpetBoost");
                _state.Cap.PP = (uint)bin.GetNamedNumber("pp");
                _state.Cap.BinFullDetect = (uint)bin.GetNamedNumber("binFullDetect");
                _state.Cap.LangOta = (uint)bin.GetNamedNumber("langOta");
                _state.Cap.Maps = (uint)bin.GetNamedNumber("maps");
                _state.Cap.Edge = (uint)bin.GetNamedNumber("edge");
                _state.Cap.Eco = (uint)bin.GetNamedNumber("eco");
            }

            if (staterep.ContainsKey("vacHigh"))
            {
                _state.VacHigh = staterep.GetNamedBoolean("vacHigh");
            }

            if (staterep.ContainsKey("binPause"))
            {
                _state.BinPause = staterep.GetNamedBoolean("binPause");
            }

            if (staterep.ContainsKey("carpetBoost"))
            {
                _state.CarpetBoost = staterep.GetNamedBoolean("carpetBoost");
            }

            if (staterep.ContainsKey("openOnly"))
            {
                _state.OpenOnly = staterep.GetNamedBoolean("openOnly");
            }

            if (staterep.ContainsKey("twoPass"))
            {
                _state.TwoPass = staterep.GetNamedBoolean("twoPass");
            }

            if (staterep.ContainsKey("schedHold"))
            {
                _state.SchedHold = staterep.GetNamedBoolean("schedHold");
            }

            if (staterep.ContainsKey("pose"))
            {
                var pose = staterep.GetNamedObject("pose");
                _state.Pose.Theta = (int)pose.GetNamedNumber("theta");
                var point = pose.GetNamedObject("point");
                _state.Pose.X = (int)point.GetNamedNumber("y");
                _state.Pose.Y = (int)point.GetNamedNumber("x");
            }

            this.StateChanged?.Invoke(this, _state);

            _log.AppendLine("--" + e.Topic + "--" + staterep.Stringify());
        }

        private void Roomba_ConnectionClosed(object sender, EventArgs e)
        {
            this.Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private static async Task<string> GetPassword(HostName hostname)
        {
            var _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(hostname.CanonicalName, MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT).ConfigureAwait(false);
            var _sslStream = new SslStream(new NetworkStream(_socket, true), false, (a,b,c,d) => { return true; });
            try
            {
                await _sslStream.AuthenticateAsClientAsync(hostname.CanonicalName, null, SslProtocols.Tls, true).ConfigureAwait(false);
                byte[] pwdreq = new byte[] { 0xf0, 0x05, 0xef, 0xcc, 0x3b, 0x29, 0x00 };
                _sslStream.Write(pwdreq);
                byte[] buffer = new byte[1024];
                int length = _sslStream.Read(buffer, 0, buffer.Length);
                if((length != 2) || (buffer[0] != pwdreq[0]))
                {
                    return null;
                }

                length = _sslStream.Read(buffer, 0, buffer.Length);
                if((length > 6) && (buffer[0] == pwdreq[2]) && 
                    (buffer[1] == pwdreq[3]) && (buffer[2] == pwdreq[4]) && 
                    (buffer[3] == pwdreq[5]) && (buffer[4] == pwdreq[6]))
                {
                    return Encoding.UTF8.GetString(buffer, 5, length - 5);
                }

                return null;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}
