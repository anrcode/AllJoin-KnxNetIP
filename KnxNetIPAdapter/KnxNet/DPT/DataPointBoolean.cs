namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPointBoolean : DataPoint
    {
        public override string[] Ids
        {
            get
            {
                return new[] { "1.001", "1.002", "1.003", "1.004", "1.005", "1.006", "1.007", "1.008", "1.009", "1.010", "1.011", "1.012" };
            }
        }

        public override object FromASDU(byte[] data)
        {
            if (data == null || data.Length != 1)
            {
                return false;
            }

            return (data[0] & 0x01) != 0;
        }

        public override byte[] ToASDU(object val)
        {
            var dataPoint = new byte[1];
            dataPoint[0] = 0x00;

            if (val is bool)
                dataPoint[0] = ((bool)val) ? (byte)0x01 : (byte)0x00;
            else if (val is int)
                dataPoint[0] = ((int)val) != 0 ? (byte)0x01 : (byte)0x00;
            else
            {
                //Logger.Error("6.xxx", "input value received is not a valid type");
                return dataPoint;
            }

            return dataPoint;
        }
    }
}
