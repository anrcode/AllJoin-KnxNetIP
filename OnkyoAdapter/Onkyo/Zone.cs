namespace OnkyoAdapter.Onkyo
{
    public static class Zone
    {
        private static EZone meCurrentZone = EZone.Zone1;
        public static EZone CurrentZone
        {
            get { return meCurrentZone; }
            set { meCurrentZone = value; }
        }
    }
}
