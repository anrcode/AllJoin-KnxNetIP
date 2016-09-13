using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class Tone : CommandBase
    {

        public static Tone StateCommand()
        {
            string lsCommandMessage = "TFRQSTN";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZTNQSTN";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "TN3QSTN";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "TN4QSTN";
                    break;
            }
            return new Tone()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static Tone TrebleUpCommand()
        {
            string lsCommandMessage = "TFRTUP";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZTNTUP";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "TN3TUP";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "TN4TUP";
                    break;
            }
            return new Tone()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static Tone TrebleDownCommand()
        {
            string lsCommandMessage = "TFRTDOWN";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZTNTDOWN";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "TN3TDOWN";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "TN4TDOWN";
                    break;
            }
            return new Tone()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static Tone BassUpCommand()
        {
            string lsCommandMessage = "TFRBUP";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZTNBUP";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "TN3BUP";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "TN4BUP";
                    break;
            }
            return new Tone()
            {
                CommandMessage = lsCommandMessage
            };
        }

        public static Tone BassDownCommand()
        {
            string lsCommandMessage = "TFRBDOWN";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsCommandMessage = "ZTNBDOWN";
                    break;
                case EZone.Zone3:
                    lsCommandMessage = "TN3BDOWN";
                    break;
                case EZone.Zone4:
                    lsCommandMessage = "TN4BDOWN";
                    break;
            }
            return new Tone()
            {
                CommandMessage = lsCommandMessage
            };
        }

        #region Constructor / Destructor

        internal Tone()
        { }

        #endregion

        public int? TrebleLevel { get; private set; }

        public int? BassLevel { get; private set; }

        public string TrebleDisplay { get; private set; }

        public string BassDisplay { get; private set; }

        public bool CanTrebleDown()
        {
            return this.TrebleLevel.GetValueOrDefault() > -10;
        }

        public bool CanTrebleUp()
        {
            return this.TrebleLevel.GetValueOrDefault() < 10;
        }

        public bool CanBassDown()
        {
            return this.BassLevel.GetValueOrDefault() > -10;
        }

        public bool CanBassUp()
        {
            return this.BassLevel.GetValueOrDefault() < 10;
        }

        public override bool Match(string psStatusMessage)
        {
            string lsMatchToken = "TFR";
            switch (Zone.CurrentZone)
            {
                case EZone.Zone2:
                    lsMatchToken = "ZTN";
                    break;
                case EZone.Zone3:
                    lsMatchToken = "TN3";
                    break;
                case EZone.Zone4:
                    lsMatchToken = "TN4";
                    break;
            }
            var loMatch = Regex.Match(psStatusMessage, @"!1{0}B(.\w)T(.\w)".FormatWith(lsMatchToken));
            if (loMatch.Success)
            {
                this.BassLevel = loMatch.Groups[1].Value.ConvertDbValueToInt();
                this.TrebleLevel = loMatch.Groups[2].Value.ConvertDbValueToInt();
                this.BassDisplay = this.BassLevel.ConvertDbIntValueToDisplay();
                this.TrebleDisplay = this.TrebleLevel.ConvertDbIntValueToDisplay();
                return true;
            }
            return false;
        }

    }
}
