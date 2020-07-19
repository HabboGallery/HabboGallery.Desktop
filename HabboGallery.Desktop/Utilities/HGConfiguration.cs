using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace HabboGallery.Desktop.Utilities
{
    public class HGConfiguration
    {
        private readonly Configuration _config;

        public KeyValueConfigurationCollection Settings => _config.AppSettings.Settings;

        public string Email
        {
            get => Get<string>();
            set => Set(value);
        }
        public string Password
        {
            get => Get<string>();
            set => Set(value);
        }
        public bool RememberMe
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsFirstConnect
        {
            get => Get<bool>();
            set => Set(value);
        }
        public IList<string> VisitedHotels
        {
            get => Get<List<string>>();
            set => Set(value);
        }

        public HGConfiguration()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        private T Get<T>([CallerMemberName]string key = "")
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)bool.Parse(Settings[key].Value);

            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(Settings[key].Value);

            if (typeof(T) == typeof(string))
                return (T)(object)Settings[key].Value;

            if (typeof(T).IsAssignableFrom(typeof(IList<string>)))
                return (T)(object)Settings[key].Value
                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            return default;
        }
        private void Set(object value, [CallerMemberName] string key = "")
        {
            Settings[key].Value = value.ToString();
        }
    }
}
