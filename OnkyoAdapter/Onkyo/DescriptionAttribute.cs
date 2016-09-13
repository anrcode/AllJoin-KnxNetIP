using System;

namespace OnkyoAdapter.Onkyo
{
    internal class DescriptionAttribute : Attribute
    {
        private string _message = null;

        public string Description
        {
            get { return _message; }
        }

        internal DescriptionAttribute(string message)
        {
            _message = message;
        }
    }
}