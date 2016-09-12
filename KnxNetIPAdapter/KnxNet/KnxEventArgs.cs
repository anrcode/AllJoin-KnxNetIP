using System;


namespace KnxNetIPAdapter.KnxNet
{
    internal class KnxEventArgs : EventArgs
    {
        public string Address { get; internal set; }
        public byte[] Data { get; internal set; }
    }
}
