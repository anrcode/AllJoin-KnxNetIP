using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class Power : CommandBase
    {

        public static Power StateCommand()
        {
            string lsCommandMessage = "PWRQSTN";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZPWQSTN";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "PW3QSTN";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "PW4QSTN";
                    break;
            }
            return new Power()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static Power On()
        {
            string lsCommandMessage = "PWR01";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZPW01";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "PW301";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "PW401";
                    break;
            }
            return new Power()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static Power Off()
        {
            string lsCommandMessage = "PWR00";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZPW00";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "PW300";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "PW400";
                    break;
            }
            return new Power()
            {
                CommandMessage = lsCommandMessage
            };
        }

        #region Constructor / Destructor

        internal Power()
        { }

        #endregion

        public bool IsOn { get; private set; }

        public override bool Match(string psStatusMessage)
        {
            string lsMatchToken = "PWR";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsMatchToken = "ZPW";
                    break;
                case EZone.Zone3:
                    lsMatchToken = "PW3";
                    break;
                case EZone.Zone4:
                    lsMatchToken = "PW4";
                    break;
            }
            var loMatch = Regex.Match(psStatusMessage, @"!1{0}(.*)".FormatWith(lsMatchToken));
            if (loMatch.Success)
            {
                this.IsOn = loMatch.Groups[1].Value == "01";
                return true;
            }
            return false;
        }
    }

}
