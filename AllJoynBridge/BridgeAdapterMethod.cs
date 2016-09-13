using System;
using System.Collections.Generic;
using BridgeRT;


namespace SparkAlljoyn
{
    public class BridgeAdapterMethod<T> : IAdapterMethod
    {
        public string Name { get; }

        public string Description { get; }

        public IList<IAdapterValue> InputParams { get; set; }

        public IList<IAdapterValue> OutputParams { get; }

        public int HResult { get; private set; }

        public T Device { get; private set; }

        public BridgeAdapterMethod(T device, string ObjectName, string Description, int ReturnValue)
        {
            this.Device = device;
            this.Name = ObjectName;
            this.Description = Description;
            this.HResult = ReturnValue;
            this.InputParams = new List<IAdapterValue>();
            this.OutputParams = new List<IAdapterValue>();
        }

        internal void SetResult(int ReturnValue)
        {
            this.HResult = ReturnValue;
        }
    }
}
