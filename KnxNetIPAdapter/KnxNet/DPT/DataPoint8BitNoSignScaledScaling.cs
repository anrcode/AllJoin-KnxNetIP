namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPoint8BitNoSignScaledScaling : DataPoint
    {
        public override string[] Ids
        {
            get { return new[] { "5.001" }; }
        }

        public override object FromASDU(byte[] data)
        {
            if (data == null || data.Length != 2)
            {
                return null;
            }

            var value = (int) data[1];

            decimal result = value * 100;
            result = result / 255;

            return result;
        }

        public override byte[] ToASDU(object val)
        {
            var dataPoint = new byte[] { 0x00, 0x00 };

            decimal input = 0;
            if (val is int)
                input = (decimal) ((int) val);
            else if (val is float)
                input = (decimal) ((float) val);
            else if (val is long)
                input = (decimal) ((long) val);
            else if (val is double)
                input = (decimal) ((double) val);
            else if (val is decimal)
                input = (decimal) val;
            else
            {
                //Logger.Error("5.001", "input value received is not a valid type");
                return dataPoint;
            }

            if (input < 0 || input > 100)
            {
                //Logger.Error("5.001", "input value received is not in a valid range");
                return dataPoint;
            }

            input = input * 255;
            input = input / 100;

            dataPoint[1] = (byte) (input);

            return dataPoint;
        }
    }
}