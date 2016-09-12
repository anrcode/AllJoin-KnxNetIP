using System;

namespace KnxNetIPAdapter.KnxNet.DPT
{
    internal sealed class DataPointDate : DataPoint
    {
        public override string[] Ids
        {
            get
            {
                return new[] { "11.001" };
            }
        }

        public override object FromASDU(byte[] data)
        {
            if (data == null || data.Length != 4)
            {
                return null;
            }

            return new DateTime((data[3] & 0x7f) + 2000, data[2] & 0xf, data[1] & 0x1f);
        }

        public override byte[] ToASDU(object val)
        {
            throw new NotSupportedException();
        }
    }
}
