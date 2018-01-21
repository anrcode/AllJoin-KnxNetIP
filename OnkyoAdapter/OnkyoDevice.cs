using System;
using System.Linq;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using OnkyoAdapter.Onkyo;


namespace OnkyoAdapter
{
    class OnkyoDevice : BridgeAdapterDevice<OnkyoAdapter>
    {
        private OnkyoClient _conn;

        internal OnkyoDevice(OnkyoAdapter adapter, OnkyoClient conn, string Name, string VendorName, string Model, string Version, string SerialNumber, string Description) 
            : base(adapter, Name, VendorName, Model, Version, SerialNumber, Description)
        {
            _conn = conn;
            _conn.MessageReceived += OnkyoMessageReceived;
        }        

        override protected void CreateProperties()
        {
            this.Properties.Clear();

            var statusProp = new BridgeAdapterProperty<OnkyoDevice>(this, "Receiver", "com.allseen.SmartHome.Receiver");
            statusProp.Attributes.Add(new BridgeAdapterAttribute("OnOff", false, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("MasterVolume", -1, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Input", "-", E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("Mute", false, E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            statusProp.Attributes.Add(new BridgeAdapterAttribute("ListeningMode", "-", E_ACCESS_TYPE.ACCESS_READWRITE) { COVBehavior = SignalBehavior.Always });
            this.Properties.Add(statusProp);
            this.AddChangeOfValueSignal(statusProp);
        }

        public void SendPropertyValue(IAdapterProperty Property, IAdapterValue Value)
        {
            if (Value.Name == "OnOff")
            {
                if(Convert.ToBoolean(Value.Data))
                {
                    _conn.SendCommand(Onkyo.Command.Power.On());
                }
                else
                {
                    _conn.SendCommand(Onkyo.Command.Power.Off());
                }
                
            }
            else if (Value.Name == "MasterVolume")
            {
                _conn.SendCommand(Onkyo.Command.MasterVolume.SetLevel((int)Value.Data));
            }
            else if (Value.Name == "Input")
            {
                Onkyo.EInputSelector input = Onkyo.EnumExtensions.FromDescription<Onkyo.EInputSelector>(Value.Data as string);
                _conn.SendCommand(Onkyo.Command.InputSelector.Chose(input));
            }
            else if (Value.Name == "Mute")
            {
                _conn.SendCommand(Onkyo.Command.AudioMuting.Chose((bool)Value.Data));
            }
            else if (Value.Name == "ListeningMode")
            {
                Onkyo.EListeningMode mode = Onkyo.EnumExtensions.FromDescription<Onkyo.EListeningMode>(Value.Data as string);
                _conn.SendCommand(Onkyo.Command.ListeningMode.Chose(mode));
            }
        }

        public void CallMethod(IAdapterMethod methodn)
        {

        }

        public async Task<bool> AquireCurrentState()
        {
            await Task.Delay(100);
            _conn.SendCommand(Onkyo.Command.Power.StateCommand());
            await Task.Delay(100);
            _conn.SendCommand(Onkyo.Command.MasterVolume.StateCommand());
            await Task.Delay(100);
            _conn.SendCommand(Onkyo.Command.InputSelector.StateCommand());
            await Task.Delay(100);
            _conn.SendCommand(Onkyo.Command.AudioMuting.StateCommand());
            await Task.Delay(100);
            _conn.SendCommand(Onkyo.Command.ListeningMode.State);
            await Task.Delay(100);
            _conn.SendCommand(Onkyo.Command.NetPlayStatus.State);
            await Task.Delay(500);
            _enableSignals = true;

            return true;
        }

        private void OnkyoMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var statusProp = this.Properties[0];

            foreach (var cmd in Onkyo.Command.CommandBase.CommandList.Where(item => item.Match(e.Message)))
            {
                if (cmd is Onkyo.Command.Power)
                {
                    this.UpdatePropertyValue(statusProp, statusProp.Attributes[0], ((Onkyo.Command.Power)cmd).IsOn);
                }
                else if (cmd is Onkyo.Command.MasterVolume)
                {
                    var newVal = ((Onkyo.Command.MasterVolume)cmd).VolumeLevel;
                    this.UpdatePropertyValue(statusProp, statusProp.Attributes[1], newVal);
                }
                else if (cmd is Onkyo.Command.InputSelector)
                {
                    var newVal = ((Onkyo.Command.InputSelector)cmd).CurrentInputSelector.ToString();
                    this.UpdatePropertyValue(statusProp, statusProp.Attributes[2], newVal);
                }
                else if (cmd is Onkyo.Command.AudioMuting)
                {
                    var newVal = ((Onkyo.Command.AudioMuting)cmd).Mute;
                    this.UpdatePropertyValue(statusProp, statusProp.Attributes[3], newVal);
                }
                else if (cmd is Onkyo.Command.ListeningMode)
                {
                    var newVal = ((Onkyo.Command.ListeningMode)cmd).CurrentListeningMode.ToString();
                    this.UpdatePropertyValue(statusProp, statusProp.Attributes[4], newVal);
                }
                else
                {
                    //logger.Log("Got unknown ISCP Message {0}", e.Message);
                }
            }
        }
    }
}
