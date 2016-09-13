using Windows.Networking;

namespace OnkyoAdapter.Onkyo
{
    internal class DeviceInfo
    {
        public string Category { get; set; }
        public string Model { get; set; }
        public string ServiceName { get; set; }
        public string Area { get; set; }
        public string MacAddress { get; set; }
        public HostName HostName { get; set; }

        public override string ToString()
        {
            return "{0}: {1}".FormatWith(this.Model, this.HostName);
        }
    }
}
