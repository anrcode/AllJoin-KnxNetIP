using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using KnxNetIPAdapter.KnxNet;
using KnxNetIPAdapter.KnxNet.DPT;


namespace KnxNetIPAdapter
{
    internal class KnxPresence : KnxDevice
    {
        private KnxNetTunnelingConnection _conn;

        public string PresenceStatusAddr { get; set; }

        internal KnxPresence(KnxAdapter adapter, KnxNetTunnelingConnection conn, string name, string serialNo, string description) : 
            base(adapter, name, "KNX", "KNX Presence", "1.0", serialNo, description)
        {
            _conn = conn;
            _conn.KnxEvent += HandleKnxEvent;
            _conn.KnxStatus += HandleKnxEvent;
        }

        override protected void CreateProperties()
        {
            this.Properties.Clear();

            var statusProp = new BridgeAdapterProperty<KnxDevice>(this, "Status", "com.allseen.SmartHome.Presence");
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Detected", false, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });

            this.Properties.Add(statusProp);
            this.AddChangeOfValueSignal(statusProp);
        }

        override internal async Task<bool> AquireCurrentState()
        {
            await Task.Delay(500);
            _conn.RequestStatus(this.PresenceStatusAddr);

            return await base.AquireCurrentState();
        }

        private void HandleKnxEvent(object sender, KnxEventArgs e)
        {
            var statusProp = this.Properties[0];

            if (e.Address == this.PresenceStatusAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("1.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[0], value);

                Task.Run(async () => {
                    await Task.Delay(5000);
                    this.UpdatePropertyValue(statusProp, statusProp.Attributes[0], false);
                });
            }
        }
    }
}
