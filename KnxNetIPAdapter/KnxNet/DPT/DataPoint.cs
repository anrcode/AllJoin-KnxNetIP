namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal abstract class DataPoint
    {
        public abstract string[] Ids { get; }

        public abstract object FromASDU(byte[] data);

        public abstract byte[] ToASDU(object value);
    }
}
