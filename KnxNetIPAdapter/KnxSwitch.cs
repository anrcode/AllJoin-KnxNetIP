using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using KnxNetIPAdapter.KnxNet;
using KnxNetIPAdapter.KnxNet.DPT;


namespace KnxNetIPAdapter
{
    internal class KnxSwitch : KnxDevice
    {
        private KnxNetTunnelingConnection _conn;
        public string SwitchAddr { get; set; }
        public string SwitchStatusAddr { get; set; }


        internal KnxSwitch(KnxAdapter adapter, KnxNetTunnelingConnection conn, string name, string serialNo, string description) : 
            base(adapter, name, "KNX", "KNX Switch", "1.0", serialNo, description)
        {
            _conn = conn;
            _conn.KnxEvent += HandleKnxEvent;
            _conn.KnxStatus += HandleKnxEvent;
        }

        override protected void CreateProperties()
        {
            this.Properties.Clear();

            var statusProp = new BridgeAdapterProperty<KnxDevice>(this, "Status", "com.allseen.SmartHome.Switch");
            statusProp.Attributes.Add(new BridgeAdapterAttribute("OnOff", true, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            
            this.Properties.Add(statusProp);
            this.AddChangeOfValueSignal(statusProp);
        }

        override public void SendPropertyValue(IAdapterProperty property, IAdapterValue value)
        {
            if (value.Name == "OnOff")
            {
                _conn.Action(this.SwitchAddr, "1.001", (bool)value.Data);               
            }
        }

        override internal async Task<bool> AquireCurrentState()
        {
            await Task.Delay(500);
            _conn.RequestStatus(this.SwitchStatusAddr);

            return await base.AquireCurrentState();
        }

        private void HandleKnxEvent(object sender, KnxEventArgs e)
        {
            var statusProp = this.Properties[0];

            if (e.Address == this.SwitchStatusAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("1.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[0], value);
            }
        }
    }
}
