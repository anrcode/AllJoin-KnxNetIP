using System;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using KnxNetIPAdapter.KnxNet;
using KnxNetIPAdapter.KnxNet.DPT;


namespace KnxNetIPAdapter
{
    internal class KnxShutterBlinds : KnxDevice
    {
        private KnxNetTunnelingConnection _conn;
        public string PositionAddr { get; set; }
        public string PositionStatusAddr { get; set; }
        public string FlapsAddr { get; set; }
        public string FlapsStatusAddr { get; set; }

        internal KnxShutterBlinds(KnxAdapter adapter, KnxNetTunnelingConnection conn, string name, string serialNo, string description) :
            base(adapter, name, "KNX", "KNX Shutter Blinds", "1.0", serialNo, description)
        {
            _conn = conn;
            _conn.KnxEvent += HandleKnxEvent;
            _conn.KnxStatus += HandleKnxEvent;
        }

        override protected void CreateProperties()
        {
            this.Properties.Clear();

            var statusProp = new BridgeAdapterProperty<KnxDevice>(this, "ShutterBlinds", "com.allseen.SmartHome.ShutterBlinds");
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Position", 0, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Flaps", 0, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });

            this.Properties.Add(statusProp);
            this.AddChangeOfValueSignal(statusProp);
        }

        override public void SendPropertyValue(IAdapterProperty property, IAdapterValue value)
        {
            if (value.Name == "Position")
            {
                _conn.Action(this.PositionAddr, "5.001", (int)value.Data);
            }
            else if (value.Name == "Flaps")
            {
                _conn.Action(this.FlapsAddr, "5.001", (int)value.Data);
            }
        }

        override internal async Task<bool> AquireCurrentState()
        {
            await Task.Delay(500);
            _conn.RequestStatus(this.PositionStatusAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.FlapsStatusAddr);

            return await base.AquireCurrentState();
        }

        private void HandleKnxEvent(object sender, KnxEventArgs e)
        {
            var statusProp = this.Properties[0];

            if (e.Address == this.PositionStatusAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("5.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[0], Convert.ToInt32(value));
            }
            else if (e.Address == this.FlapsStatusAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("5.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[1], Convert.ToInt32(value));
            }
        }
    }
}
