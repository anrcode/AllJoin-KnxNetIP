namespace OnkyoAdapter.Onkyo.Command
{
    internal class NetTune : CommandBase
    {
        public static NetTune Chose(ENetTuneOperation peOperation, DeviceInfo poDevice)
        {
            string lsCommandMessage = "NTC{0}";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "NTZ{0}";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "NT3{0}";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "NT4{0}";
                    break;
            }
            return new NetTune()
            {
                CommandMessage = lsCommandMessage.FormatWith(peOperation.ToDescription())
            };
        }

        #region Constructor / Destructor

        internal NetTune()
        { }

        #endregion

        public override bool Match(string psStatusMessage)
        {
            return false;
        }
    }
}
