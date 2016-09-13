using System.Text.RegularExpressions;


namespace OnkyoAdapter.Onkyo.Command
{
    internal class SubwooferLevel : CommandBase
    {
        public static readonly SubwooferLevel State = new SubwooferLevel()
        {
            CommandMessage = "SWLQSTN"
        };

        public static readonly SubwooferLevel Up = new SubwooferLevel()
        {
            CommandMessage = "SWLUP"
        };

        public static readonly SubwooferLevel Down = new SubwooferLevel()
        {
            CommandMessage = "SWLDOWN"
        };

        #region Constructor / Destructor

        internal SubwooferLevel()
        { }

        #endregion

        public int? Level { get; private set; }

        public string Display { get; private set; }

        public bool CanLevelDown()
        {
            return this.Level.GetValueOrDefault() > -15;
        }

        public bool CanLevelUp()
        {
            return this.Level.GetValueOrDefault() < 12;
        }

        public override bool Match(string psStatusMessage)
        {
            var loMatch = Regex.Match(psStatusMessage, @"!1SWL(.\w)");
            if (loMatch.Success)
            {
                this.Level = loMatch.Groups[1].Value.ConvertDbValueToInt();
                this.Display = this.Level.ConvertDbIntValueToDisplay();
                return true;
            }
            return false;
        }

    }
}
