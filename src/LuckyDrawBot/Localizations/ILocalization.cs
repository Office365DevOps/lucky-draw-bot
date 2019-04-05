using System;
using System.Collections.Generic;

namespace LuckyDrawBot.Localizations
{
    public interface ILocalization
    {
        string this[string name, params object[] arguments] { get; }
    }

    public class BaseLocalization : ILocalization
    {
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        protected BaseLocalization(Dictionary<string, string> strings)
        {
            _strings = strings;
        }

        public string this[string name, params object[] arguments]
        {
            get
            {
                try
                {
                    var format = _strings[name];
                    return string.Format(format, arguments);
                }
                catch(Exception ex)
                {
                    throw new Exception($"Failed to localize '{name}'.", ex);
                }
            }
        }
    }
}