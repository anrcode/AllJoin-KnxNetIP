using System;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using KnxNetIPAdapter.KnxNet;
using KnxNetIPAdapter.KnxNet.DPT;


namespace KnxNetIPAdapter
{
    internal class KnxHvac : KnxDevice
    {
        private KnxNetTunnelingConnection _conn;
        public string TemperatureAddr { get; set; }
        public string CO2Addr { get; set; }
        public string HumidityAddr { get; set; }
        public string OpModeAddr { get; set; }
        public string OpModeStatusAddr { get; set; }
        public string TargetTemperatureAddr { get; set; }
        public string TargetTemperatureStatusAddr { get; set; }

        internal KnxHvac(KnxAdapter adapter, KnxNetTunnelingConnection conn, string name, string serialNo, string description) : 
            base(adapter, name, "KNX", "KNX Hvac", "1.0", serialNo, description)
        {
            _conn = conn;
            _conn.KnxEvent += HandleKnxEvent;
            _conn.KnxStatus += HandleKnxEvent;
        }

        override protected void CreateProperties()
        {
            this.Properties.Clear();

            var statusProp = new BridgeAdapterProperty<KnxDevice>(this, "Hvac", "com.allseen.SmartHome.Hvac");
            statusProp.Attributes.Add(new BridgeAdapterAttribute("OpMode", 0, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });          
            statusProp.Attributes.Add(new BridgeAdapterAttribute("TargetTemperature", 0.0, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Temperature", 0.0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Humidity", 0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("CO2", 0, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });

            this.Properties.Add(statusProp);
            this.AddChangeOfValueSignal(statusProp);
        }

        override public void SendPropertyValue(IAdapterProperty property, IAdapterValue value)
        {
            if (value.Name == "OpMode")
            {
                _conn.Action(this.OpModeAddr, "20.102", (int)value.Data);
            }
            else if (value.Name == "TargetTemperature")
            {
                _conn.Action(this.TargetTemperatureAddr, "9.001", (decimal)value.Data);
            }
        }

        override internal async Task<bool> AquireCurrentState()
        {
            await Task.Delay(500);
            _conn.RequestStatus(this.OpModeStatusAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.TemperatureAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.TargetTemperatureStatusAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.HumidityAddr);
            await Task.Delay(500);
            _conn.RequestStatus(this.CO2Addr);

            return await base.AquireCurrentState();
        }

        private void HandleKnxEvent(object sender, KnxEventArgs e)
        {
            var statusProp = this.Properties[0];

            if (e.Address == this.OpModeStatusAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("20.102", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[0], value);
            }
            else if (e.Address == this.TargetTemperatureStatusAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("9.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[1], value);
            }
            else if (e.Address == this.TemperatureAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("9.001", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[2], value);
            }           
            else if (e.Address == this.HumidityAddr)
            {
                var value = DataPointTranslator.Instance.FromASDU("9.007", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[3], Convert.ToInt32(value));
            }
            else if (e.Address == this.CO2Addr)
            {
                var value = DataPointTranslator.Instance.FromASDU("9.008", e.Data);
                this.UpdatePropertyValue(statusProp, statusProp.Attributes[4], Convert.ToInt32(value));
            }
        }
    }
}
