using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class AudioMuting : CommandBase
    {
        public static AudioMuting StateCommand()
        {
            string lsCommandMessage = "AMTQSTN";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZMTQSTN";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "MT3QSTN";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "MT4QSTN";
                    break;
            }
            return new AudioMuting()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static AudioMuting Chose(bool pbMute)
        {
            string lsCommandMessage = "AMT{0}";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZMT{0}";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "MT3{0}";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "MT4{0}";
                    break;
            }
            return new AudioMuting()
            {
                CommandMessage = lsCommandMessage.FormatWith(pbMute ? "01" : "00")
            };
        }

        #region Constructor / Destructor

        internal AudioMuting()
        { }

        #endregion

        public bool Mute { get; private set; }

        public override bool Match(string psStatusMessage)
        {
            string lsMatchToken = "AMT";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsMatchToken = "ZMT";
                    break;
                case EZone.Zone3:
                    lsMatchToken = "MT3";
                    break;
                case EZone.Zone4:
                    lsMatchToken = "MT4";
                    break;
            }
            var loMatch = Regex.Match(psStatusMessage, @"!1{0}(\w\w)".FormatWith(lsMatchToken));
            if (loMatch.Success)
            {
                this.Mute = loMatch.Groups[1].Value == "01";
                return true;
            }
            return false;
        }
    }
}
