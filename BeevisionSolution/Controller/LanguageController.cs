using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeevisionSolution.Controller
{
    public static class LanguageController
    {
        public enum Language
        {
            English,
            Korean,
            Vietnamese
        }

        private static Language _currentLanguage = Language.English;

        public static Language CurrentLanguage
        {
            get => _currentLanguage;
            private set => _currentLanguage = value;
        }

        public static event EventHandler LanguageChanged;

        /// <summary>
        /// Sets the application language
        /// </summary>
        /// <param name="language">Target language</param>
        public static void SetLanguage(Language language)
        {
            CurrentLanguage = language;

            switch (language)
            {
                case Language.English:
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    break;
                case Language.Korean:
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("ko-KR");
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("ko-KR");
                    break;
                case Language.Vietnamese:
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("vi-VN");
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                    break;
            }

            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Gets a localized string from resources
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public static string GetString(string key)
        {
            try
            {
                return Properties.Resources.ResourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture) ?? key;
            }
            catch
            {
                return key;
            }
        }
    }
}
