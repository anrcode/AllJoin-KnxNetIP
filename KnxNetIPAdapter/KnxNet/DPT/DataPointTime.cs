using System;

namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPointTime : DataPoint
    {
        public override string[] Ids
        {
            get
            {
                return new[] { "10.001" };
            }
        }

        public override object FromASDU(byte[] data)
        {
            if (data == null || data.Length != 4)
            {
                return null;
            }

            return new TimeSpan(data[1] & 0x1f, data[2] & 0x3f, data[3] & 0x3f);
        }

        public override byte[] ToASDU(object val)
        {
            throw new NotSupportedException();
        }
    }
}
