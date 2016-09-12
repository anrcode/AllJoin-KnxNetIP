using System;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using KnxNetIPAdapter.KnxNet;
using KnxNetIPAdapter.KnxNet.DPT;


namespace KnxNetIPAdapter
{
    internal class KnxWeather : KnxDevice
    {
        private KnxNetTunnelingConnection _conn;
        public string TemperatureAddr { get; set; }
        public string WindAddr { get; set; }
        public string RainAddr { get; set; }
        public string DawnAddr { get; set; }

        internal KnxWeather(KnxAdapter adapter, KnxNetTunnelingConnection conn, string name, string serialNo, string description) : 
            base(adapter, name, "KNX", "KNX KnxWeather", "1.0", serialNo, description)
        {
            _conn = conn;
            _conn.KnxEvent += HandleKnxEvent;
            _conn.KnxStatus += HandleKnxEvent;
        }

        override protected void CreateProperties()
        {
            this.Properties.Clear();

            var statusProp = new BridgeAdapterProperty<KnxDevice>(this, "Weather", "com.allseen.SmartHome.Weather");
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Temperature", 0.0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Rain", 0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Wind", 0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Dawn", 0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            
            this.Properties.Add(statusProp);
            this.AddChangeOfValueSignal(statusProp);
        }

        override internal async Task<bool> AquireCurrentState()
        {
            await Task.Delay(500);
            _conn.RequestStatus(this.TemperatureAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.RainAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.WindAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.DawnAddr);

            return await base.AquireCurrentState();
        }

        private void HandleKnxEvent(object sender, KnxEventArgs e)
        {
            var statusProp = this.Properties[0];

            if (e.Address == this.TemperatureAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("9.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[0], value);
            }
            else if (e.Address == this.RainAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("1.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[1], value);
            }
            else if (e.Address == this.WindAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("9.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[2], value);
            }
            else if (e.Address == this.DawnAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("9.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[3], value);
            }
        }
    }
}
