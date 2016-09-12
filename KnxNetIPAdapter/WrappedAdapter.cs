using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using BridgeRT;


namespace KnxNetIPAdapter
{
    public sealed class WrappedAdapter : IAdapter
    {
        private IAdapter _adapter = new KnxAdapter();

        public string AdapterName
        {
            get
            {
                return _adapter.AdapterName;
            }
        }

        public string ExposedAdapterPrefix
        {
            get
            {
                return _adapter.ExposedAdapterPrefix;
            }
        }

        public Guid ExposedApplicationGuid
        {
            get
            {
                return _adapter.ExposedApplicationGuid;
            }
        }

        public string ExposedApplicationName
        {
            get
            {
                return _adapter.ExposedApplicationName;
            }
        }

        public IList<IAdapterSignal> Signals
        {
            get
            {
                return _adapter.Signals;
            }
        }

        public string Vendor
        {
            get
            {
                return _adapter.Vendor;
            }
        }

        public string Version
        {
            get
            {
                return _adapter.Version;
            }
        }

        public WrappedAdapter()
        {

        }

        public uint CallMethod(IAdapterMethod Method, out IAdapterIoRequest RequestPtr)
        {
            return _adapter.CallMethod(Method, out RequestPtr);
        }

        public uint EnumDevices(ENUM_DEVICES_OPTIONS Options, out IList<IAdapterDevice> DeviceListPtr, out IAdapterIoRequest RequestPtr)
        {
            return _adapter.EnumDevices(Options, out DeviceListPtr, out RequestPtr);
        }

        public uint GetConfiguration(out byte[] ConfigurationDataPtr)
        {
            return _adapter.GetConfiguration(out ConfigurationDataPtr);
        }

        public uint GetProperty(IAdapterProperty Property, out IAdapterIoRequest RequestPtr)
        {
            return _adapter.GetProperty(Property, out RequestPtr);
        }

        public uint GetPropertyValue(IAdapterProperty Property, string AttributeName, out IAdapterValue ValuePtr, out IAdapterIoRequest RequestPtr)
        {
            return _adapter.GetPropertyValue(Property, AttributeName, out ValuePtr, out RequestPtr);
        }

        public uint Initialize()
        {
            return _adapter.Initialize();
        }

        public uint RegisterSignalListener(IAdapterSignal Signal, IAdapterSignalListener Listener, object ListenerContext)
        {
            return _adapter.RegisterSignalListener(Signal, Listener, ListenerContext);
        }

        public uint SetConfiguration([ReadOnlyArray]byte[] ConfigurationData)
        {
            return _adapter.SetConfiguration(ConfigurationData);
        }

        public uint SetProperty(IAdapterProperty Property, out IAdapterIoRequest RequestPtr)
        {
            return _adapter.SetProperty(Property, out RequestPtr);
        }

        public uint SetPropertyValue(IAdapterProperty Property, IAdapterValue Value, out IAdapterIoRequest RequestPtr)
        {
            return _adapter.SetPropertyValue(Property, Value, out RequestPtr);
        }

        public uint Shutdown()
        {
            return _adapter.Shutdown();
        }

        public uint UnregisterSignalListener(IAdapterSignal Signal, IAdapterSignalListener Listener)
        {
            return _adapter.UnregisterSignalListener(Signal, Listener);
        }
    }
}
