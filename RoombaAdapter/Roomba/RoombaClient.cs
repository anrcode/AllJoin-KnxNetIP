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
    internal class RoombaClient
    {
        private HostName _remoteHost;
        private string _remoteService;
        private string _user;
        private string _pass;
   
        private MqttClient _client = null;
        private RobotState _state = new RobotState();

        private StringBuilder _log = new StringBuilder();

        public event EventHandler Disconnected;

        public event RobotStateEventHandler StateChanged;
        public event CleanMissionStateEventHandler CleanMissionStateChanged;
        public event PoseChangedEventHandler PoseChanged;


        public RoombaClient(HostName remoteHost, string remoteService, string user, string pass)
        {
            _remoteHost = remoteHost;
            _remoteService = remoteService;
            _user = user;
            _pass = pass;           
        }

        public void Connect()
        {
            _client = new MqttClient(_remoteHost.CanonicalName, MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT, true, MqttSslProtocols.TLSv1_0, (a, b, c, d) => { return true; }, null);

            byte t = _client.Connect(_user, _user, _pass);
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

        public void SendCmd(string command)
        {
            var jsonObject = new JsonObject();
            jsonObject["command"] = JsonValue.CreateStringValue(command); // start, pause, stop, resume, dock
            long secs = DateTimeOffset.Now.ToUnixTimeSeconds();
            jsonObject["time"] = JsonValue.CreateNumberValue(secs);
            jsonObject["initiator"] = JsonValue.CreateStringValue("localApp");
            string jsonString = jsonObject.Stringify();
            ushort t2 = _client.Publish("cmd", Encoding.UTF8.GetBytes(jsonString));
        }

        public void SetEdgeClean(bool value)
        {
            var jsonObject = new JsonObject();
            jsonObject["openOnly"] = JsonValue.CreateBooleanValue(!value);
            this.SetPreference(jsonObject);
        }

        public void SetAlwaysFinish(bool value)
        {
            var jsonObject = new JsonObject();
            jsonObject["binPause"] = JsonValue.CreateBooleanValue(value);
            this.SetPreference(jsonObject);
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

            if (staterep.ContainsKey("bin"))
            {
                var bin = staterep.GetNamedObject("bin");
                _state.Bin.Present = bin.GetNamedBoolean("present");
                _state.Bin.IsFull = bin.GetNamedBoolean("full");
            }

            if (staterep.ContainsKey("cap"))
            {
                var cap = staterep.GetNamedObject("cap");
                _state.Cap.Pose = (uint)cap.GetNamedNumber("pose");
                _state.Cap.Ota = (uint)cap.GetNamedNumber("ota");
                _state.Cap.MultiPass = (uint)cap.GetNamedNumber("multiPass");
                _state.Cap.CarpetBoost = (uint)cap.GetNamedNumber("carpetBoost");
                _state.Cap.PP = (uint)cap.GetNamedNumber("pp");
                _state.Cap.BinFullDetect = (uint)cap.GetNamedNumber("binFullDetect");
                _state.Cap.LangOta = (uint)cap.GetNamedNumber("langOta");
                _state.Cap.Maps = (uint)cap.GetNamedNumber("maps");
                _state.Cap.Edge = (uint)cap.GetNamedNumber("edge");
                _state.Cap.Eco = (uint)cap.GetNamedNumber("eco");
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

            if (staterep.ContainsKey("cleanMissionStatus"))
            {
                var clennMissionState = staterep.GetNamedObject("cleanMissionStatus");

                this.CleanMissionStateChanged?.Invoke(this, new CleanMissionState()
                {
                    Cycle = clennMissionState.GetNamedString("cycle"),
                    Phase = clennMissionState.GetNamedString("phase"),
                    MissionNo = (uint)clennMissionState.GetNamedNumber("nMssn"),
                    MissionTime = TimeSpan.FromMinutes(clennMissionState.GetNamedNumber("mssnM")),
                    Sqft = (uint)clennMissionState.GetNamedNumber("sqft")
                });
            }

            if (staterep.ContainsKey("pose"))
            {
                var pose = staterep.GetNamedObject("pose");
                var point = pose.GetNamedObject("point");

                this.PoseChanged?.Invoke(this, new PoseState()
                {
                    Theta = (int)pose.GetNamedNumber("theta"),
                    X = (int)point.GetNamedNumber("y"),
                    Y = (int)point.GetNamedNumber("x")
                });
            }

            if (staterep.ContainsKey("batPct"))
            {
                _state.BatPct = (uint)staterep.GetNamedNumber("batPct");
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
