using System;
using System.Collections.Generic;
using LuckyDrawBot.Localizations;

namespace LuckyDrawBot.Services
{
    public interface ILocalizationFactory
    {
        ILocalization Create(string locale);
    }

    public class LocalizationFactory : ILocalizationFactory
    {
        private const string DefaultLocale = "en-US";

        private readonly Dictionary<string, ILocalization> _localizations = new Dictionary<string, ILocalization>(StringComparer.InvariantCultureIgnoreCase)
        {
            [DefaultLocale] = new LocalizationEnUs(),
            ["zh-CN"] = new LocalizationZhCn(),
        };

        public ILocalization Create(string locale)
        {
            if (_localizations.TryGetValue(locale, out ILocalization localization))
            {
                return localization;
            }
            return _localizations[DefaultLocale];
        }
    }
}