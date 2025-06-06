﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Globalization;
using Windows.System.UserProfile;

namespace WinGetStore.Helpers
{
    public static class LanguageHelper
    {
        public const string AutoLanguageCode = "auto";
        public const string FallbackLanguageCode = "en-US";

        public static readonly string[] SupportLanguages =
        [
            "en-US",
            "zh-CN",
            "zh-TW"
        ];

        private static readonly string[][] SupportLanguageCodes =
        [
            ["en", "en-au", "en-ca", "en-gb", "en-ie", "en-in", "en-nz", "en-sg", "en-us", "en-za", "en-bz", "en-hk", "en-id", "en-jm", "en-kz", "en-mt", "en-my", "en-ph", "en-pk", "en-tt", "en-vn", "en-zw", "en-053", "en-021", "en-029", "en-011", "en-018", "en-014"],
            ["zh-Hans", "zh-cn", "zh-hans-cn", "zh-sg", "zh-hans-sg"],
            ["zh-Hant", "zh-hk", "zh-mo", "zh-tw", "zh-hant-hk", "zh-hant-mo", "zh-hant-tw"]
        ];

        public static CultureInfo[] SupportCultures { get; } = [.. SupportLanguages.Select(x => new CultureInfo(x))];

        public static int FindIndexFromSupportLanguageCodes(string language) => Array.FindIndex(SupportLanguageCodes, codes => Array.Exists(codes, x => x.Equals(language, StringComparison.OrdinalIgnoreCase)));

        public static string GetCurrentLanguage()
        {
            IReadOnlyList<string> languages = GlobalizationPreferences.Languages;
            foreach (string language in languages)
            {
                int temp = FindIndexFromSupportLanguageCodes(language);
                if (temp != -1)
                {
                    return SupportLanguages[temp];
                }
            }
            return FallbackLanguageCode;
        }

        public static string GetPrimaryLanguage()
        {
            string language = ApplicationLanguages.PrimaryLanguageOverride;
            if (string.IsNullOrWhiteSpace(language)) { return GetCurrentLanguage(); }
            int temp = FindIndexFromSupportLanguageCodes(language);
            return temp == -1 ? FallbackLanguageCode : SupportLanguages[temp];
        }
    }
}
