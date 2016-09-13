using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using BridgeRT;


namespace SparkAlljoyn
{
    public class BridgeAdapter : IAdapter
    {
        protected const uint ERROR_SUCCESS = 0;
        protected const uint ERROR_INVALID_HANDLE = 6;

        // Device Arrival and Device Removal Signal Indices
        private const int DEVICE_ARRIVAL_SIGNAL_INDEX = 0;
        private const int DEVICE_ARRIVAL_SIGNAL_PARAM_INDEX = 0;
        private const int DEVICE_REMOVAL_SIGNAL_INDEX = 1;
        private const int DEVICE_REMOVAL_SIGNAL_PARAM_INDEX = 0;

        public string Vendor { get; }

        public string AdapterName { get; }

        public string Version { get; }

        public string ExposedAdapterPrefix { get; }

        public string ExposedApplicationName { get; }

        public Guid ExposedApplicationGuid { get; }

        public IList<IAdapterSignal> Signals { get; }

        protected IList<IAdapterDevice> devices;

        private Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>> signalListeners;


        public BridgeAdapter(string adapterName)
        {
            Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
            Windows.ApplicationModel.PackageId packageId = package.Id;
            Windows.ApplicationModel.PackageVersion versionFromPkg = packageId.Version;

            this.Vendor = "win10";
            this.AdapterName = adapterName;

            // the adapter prefix must be something like "com.mycompany" (only alpha num and dots)
            // it is used by the Device System Bridge as root string for all services and interfaces it exposes
            this.ExposedAdapterPrefix = "com." + this.Vendor.ToLower();
            this.ExposedApplicationGuid = Guid.Parse("{0x6516a3e3,0xcc3e,0x4c67,{0xb7,0x5f,0x73,0x75,0x8b,0x09,0x23,0xae}}");

            if (null != package && null != packageId)
            {
                this.ExposedApplicationName = packageId.Name;
                this.Version = versionFromPkg.Major.ToString() + "." +
                               versionFromPkg.Minor.ToString() + "." +
                               versionFromPkg.Revision.ToString() + "." +
                               versionFromPkg.Build.ToString();
            }
            else
            {
                this.ExposedApplicationName = "DeviceSystemBridge";
                this.Version = "0.0.0.0";
            }

            this.Signals = new List<IAdapterSignal>();
            this.devices = new List<IAdapterDevice>();
            this.signalListeners = new Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>>();

            // Device Arrival Signal
            var deviceArrivalSignal = new BridgeAdapterSignal(Constants.DEVICE_ARRIVAL_SIGNAL);
            var deviceHandle_arrival = new BridgeAdapterValue(Constants.DEVICE_ARRIVAL__DEVICE_HANDLE, null);
            deviceArrivalSignal.Params.Add(deviceHandle_arrival);
            this.Signals.Add(deviceArrivalSignal);

            // Device Removal Signal
            var deviceRemovalSignal = new BridgeAdapterSignal(Constants.DEVICE_REMOVAL_SIGNAL);
            var deviceHandle_removal = new BridgeAdapterValue(Constants.DEVICE_REMOVAL__DEVICE_HANDLE, null);
            deviceRemovalSignal.Params.Add(deviceHandle_removal);
            this.Signals.Add(deviceRemovalSignal);
        }

        public uint SetConfiguration([ReadOnlyArray] byte[] ConfigurationData)
        {
            return ERROR_SUCCESS;
        }

        public uint GetConfiguration(out byte[] ConfigurationDataPtr)
        {
            ConfigurationDataPtr = null;

            return ERROR_SUCCESS;
        }

        virtual public uint Initialize()
        {
            return ERROR_SUCCESS;
        }

        virtual public uint Shutdown()
        {
            return ERROR_SUCCESS;
        }

        public uint EnumDevices(ENUM_DEVICES_OPTIONS Options, out IList<IAdapterDevice> DeviceListPtr, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;
            DeviceListPtr = new List<IAdapterDevice>(this.devices);

            return ERROR_SUCCESS;
        }

        public uint GetProperty(IAdapterProperty Property, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        public uint SetProperty(IAdapterProperty Property, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        virtual public uint GetPropertyValue(IAdapterProperty Property, string AttributeName, out IAdapterValue ValuePtr, out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        virtual public uint SetPropertyValue(IAdapterProperty Property, IAdapterValue Value, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        virtual public uint CallMethod(IAdapterMethod Method, out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            return ERROR_SUCCESS;
        }

        public uint RegisterSignalListener(IAdapterSignal Signal, IAdapterSignalListener Listener, object ListenerContext)
        {
            if (Signal == null || Listener == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            int signalHashCode = Signal.GetHashCode();

            SIGNAL_LISTENER_ENTRY newEntry;
            newEntry.Signal = Signal;
            newEntry.Listener = Listener;
            newEntry.Context = ListenerContext;

            lock (this.signalListeners)
            {
                if (this.signalListeners.ContainsKey(signalHashCode))
                {
                    this.signalListeners[signalHashCode].Add(newEntry);
                }
                else
                {
                    IList<SIGNAL_LISTENER_ENTRY> newEntryList = new List<SIGNAL_LISTENER_ENTRY>();
                    newEntryList.Add(newEntry);
                    this.signalListeners.Add(signalHashCode, newEntryList);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint UnregisterSignalListener(IAdapterSignal Signal, IAdapterSignalListener Listener)
        {
            return ERROR_SUCCESS;
        }

        internal uint NotifySignalListener(IAdapterSignal Signal)
        {
            if (Signal == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            int signalHashCode = Signal.GetHashCode();

            lock (this.signalListeners)
            {
                if(!this.signalListeners.ContainsKey(signalHashCode))
                {
                    Debug.WriteLine("Error: Signal not found!");
                    return ERROR_SUCCESS;
                }

                IList<SIGNAL_LISTENER_ENTRY> listenerList = this.signalListeners[signalHashCode];
                foreach (SIGNAL_LISTENER_ENTRY entry in listenerList)
                {
                    IAdapterSignalListener listener = entry.Listener;
                    object listenerContext = entry.Context;
                    listener.AdapterSignalHandler(Signal, listenerContext);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint NotifyDeviceArrival(IAdapterDevice Device)
        {
            if (Device == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            IAdapterSignal deviceArrivalSignal = this.Signals[DEVICE_ARRIVAL_SIGNAL_INDEX];
            deviceArrivalSignal.Params[DEVICE_ARRIVAL_SIGNAL_PARAM_INDEX].Data = Device;
            this.NotifySignalListener(deviceArrivalSignal);

            return ERROR_SUCCESS;
        }

        public uint NotifyDeviceRemoval(IAdapterDevice Device)
        {
            if (Device == null)
            {
                return ERROR_INVALID_HANDLE;
            }

            IAdapterSignal deviceRemovalSignal = this.Signals[DEVICE_REMOVAL_SIGNAL_INDEX];
            deviceRemovalSignal.Params[DEVICE_REMOVAL_SIGNAL_PARAM_INDEX].Data = Device;
            this.NotifySignalListener(deviceRemovalSignal);

            return ERROR_SUCCESS;
        }

        internal struct SIGNAL_LISTENER_ENTRY
        {
            public IAdapterSignal Signal;
            public IAdapterSignalListener Listener;
            public object Context;
        }
    }
}
