using System;
using System.Linq;
using System.Threading.Tasks;
using BridgeRT;
using SparkAlljoyn;
using KnxNetIPAdapter.KnxNet;
using KnxNetIPAdapter.KnxNet.Discovery;


namespace KnxNetIPAdapter
{
    internal class KnxAdapter : BridgeAdapter
    {
        private KnxNetDiscovery _discovery;

        public KnxAdapter() : base("KnxNetIP")
        {
            Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
            Windows.ApplicationModel.PackageId packageId = package.Id;
            Windows.ApplicationModel.PackageVersion versionFromPkg = packageId.Version;
        }

        override public uint Initialize()
        {
            KnxNetDiscovery.DeviceDiscovered += KnxDeviceDiscovery_DeviceDiscovered;
            KnxNetDiscovery.DeviceRemoved += KnxNetDiscovery_DeviceRemoved;
            _discovery = new KnxNetDiscovery();

            return ERROR_SUCCESS;
        }

        private void KnxDeviceDiscovery_DeviceDiscovered(object sender, SparkAlljoyn.Discovery.AdapterDiscoveryEventArgs e)
        {
            var conn = e.Device as KnxNetTunnelingConnection;

            if (devices.Count > 0) return;

            Task.Factory.StartNew(async () =>
            {
                // TEST
                //var t = new OpenWeather();
                //this.NotifyDeviceArrival(t);
                //await t.AquireCurrentState();

                var storageFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Data");
                var storageFile = await storageFolder.GetFileAsync("Visu.xml");
                var visuXml = await Windows.Data.Xml.Dom.XmlDocument.LoadFromFileAsync(storageFile);

                var deviceNodes = visuXml.SelectNodes("//device[alljoyn[@bridge='knxnetip']]");
                foreach (Windows.Data.Xml.Dom.XmlElement deviceNode in deviceNodes)
                {
                    var alljoynNode = (Windows.Data.Xml.Dom.XmlElement)deviceNode.SelectSingleNode("alljoyn");
                    KnxDevice device = null;

                    if (deviceNode.GetAttribute("type") == "switch")
                    {
                        device = new KnxSwitch(this, conn, "Switch Device", deviceNode.GetAttribute("id"), "Knx Switch")
                        {
                            SwitchAddr = alljoynNode.GetAttribute("knxaddr"),
                            SwitchStatusAddr = alljoynNode.GetAttribute("knxsaddr")
                        };
                    }
                    else if (deviceNode.GetAttribute("type") == "presence")
                    {
                        device = new KnxPresence(this, conn, "Presence Device", deviceNode.GetAttribute("id"), "Knx Presence")
                        {
                            PresenceStatusAddr = alljoynNode.GetAttribute("knxsaddr")
                        };
                    }
                    else if (deviceNode.GetAttribute("type") == "hvac")
                    {
                        device = new KnxHvac(this, conn, "Hvac Device", deviceNode.GetAttribute("id"), "Knx Hvac")
                        {
                            OpModeAddr = alljoynNode.GetAttribute("knxmodeaddr"),
                            OpModeStatusAddr = alljoynNode.GetAttribute("knxmodesaddr"),
                            TargetTemperatureAddr = alljoynNode.GetAttribute("knxtargettempaddr"),
                            TargetTemperatureStatusAddr = alljoynNode.GetAttribute("knxtargettempsaddr"),
                            TemperatureAddr = alljoynNode.GetAttribute("knxtempaddr"),
                            HumidityAddr = alljoynNode.GetAttribute("knxhumaddr"),
                            CO2Addr = alljoynNode.GetAttribute("knxco2addr")
                        };
                    }
                    else if (deviceNode.GetAttribute("type") == "shutterblinds")
                    {
                        device = new KnxShutterBlinds(this, conn, "Jalousie Device", deviceNode.GetAttribute("id"), "Knx Jalousie")
                        {
                            PositionAddr = alljoynNode.GetAttribute("knxposaddr"),
                            PositionStatusAddr = alljoynNode.GetAttribute("knxpossaddr"),
                            FlapsAddr = alljoynNode.GetAttribute("knxflapaddr"),
                            FlapsStatusAddr = alljoynNode.GetAttribute("knxflapsaddr")
                        };
                    }
                    else if (deviceNode.GetAttribute("type") == "weather")
                    {
                        device = new KnxWeather(this, conn, "Weather Device", deviceNode.GetAttribute("id"), "Knx Weather")
                        {
                            TemperatureAddr = alljoynNode.GetAttribute("knxtempaddr"),
                            RainAddr = alljoynNode.GetAttribute("knxrainaddr"),
                            WindAddr = alljoynNode.GetAttribute("knxwindaddr"),
                            DawnAddr = alljoynNode.GetAttribute("knxdawnaddr")
                        };
                    }

                    if (device != null)
                    {
                        devices.Add(device);
                        conn.Disconnected += (object s, EventArgs args) =>
                        {
                            this.NotifyDeviceRemoval(device);
                            devices.Remove(device);
                        };

                        await device.AquireCurrentState();
                        this.NotifyDeviceArrival(device);
                    }
                }
            });
        }

        private void KnxNetDiscovery_DeviceRemoved(object sender, SparkAlljoyn.Discovery.AdapterDiscoveryEventArgs e)
        {
            var matchingDevices = devices.Where(d => d.SerialNumber == e.DeviceId).ToList();
            foreach (var device in matchingDevices)
            {
                this.NotifyDeviceRemoval(device);
                devices.Remove(device);
            }
        }

        override public uint GetPropertyValue(IAdapterProperty Property, string AttributeName, out IAdapterValue ValuePtr, out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            ValuePtr = ((BridgeAdapterProperty<KnxDevice>)Property).Device.GetPropertyValue(Property, AttributeName);

            return ERROR_SUCCESS;
        }

        override public uint SetPropertyValue(IAdapterProperty Property, IAdapterValue Value, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            ((BridgeAdapterProperty<KnxDevice>)Property).Device.SendPropertyValue(Property, Value);

            return ERROR_SUCCESS;
        }

        override public uint CallMethod(IAdapterMethod Method, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            ((BridgeAdapterMethod<KnxDevice>)Method).Device.CallMethod(Method);

            return ERROR_SUCCESS;
        }
    }
}
