namespace OnkyoAdapter.Onkyo
{
    internal class NetListItem
    {

        public NetListItem(int pnLine, string psName)
        {
            this.Line = pnLine;
            this.Name = psName;
        }

        public int Line { get; private set; }
        public string Name { get; private set; }
    }
}
