using BridgeRT;


namespace SparkAlljoyn
{
    public class BridgeAdapterValue : IAdapterValue
    {
        // public properties
        public string Name { get; }
        public object Data { get; set; }

        public BridgeAdapterValue(string ObjectName, object DefaultData)
        {
            this.Name = ObjectName;
            this.Data = DefaultData;
        }
    }
}
