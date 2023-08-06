using MetroLog;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using IObjectSerializer = Microsoft.Toolkit.Helpers.IObjectSerializer;

namespace WinGetStore.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string UpdateDate = nameof(UpdateDate);
        public const string CurrentLanguage = nameof(CurrentLanguage);
        public const string SelectedAppTheme = nameof(SelectedAppTheme);

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set<Type>(string key, Type value) => LocalObject.Save(key, value);
        public static Task<Type> GetFile<Type>(string key) => LocalObject.ReadFileAsync<Type>($"Settings/{key}");
        public static Task SetFile<Type>(string key, Type value) => LocalObject.CreateFileAsync($"Settings/{key}", value);

        public static void SetDefaultSettings()
        {
            if (!LocalObject.KeyExists(UpdateDate))
            {
                LocalObject.Save(UpdateDate, new DateTime());
            }
            if (!LocalObject.KeyExists(CurrentLanguage))
            {
                LocalObject.Save(CurrentLanguage, LanguageHelper.AutoLanguageCode);
            }
            if (!LocalObject.KeyExists(SelectedAppTheme))
            {
                LocalObject.Save(SelectedAppTheme, ElementTheme.Default);
            }
        }
    }

    internal static partial class SettingsHelper
    {
        public static readonly ILogManager LogManager = LogManagerFactory.CreateLogManager();
        public static readonly ApplicationDataStorageHelper LocalObject = ApplicationDataStorageHelper.GetCurrent(new SystemTextJsonObjectSerializer());

        static SettingsHelper() => SetDefaultSettings();
    }

    public class SystemTextJsonObjectSerializer : IObjectSerializer
    {
        // Specify your serialization settings
        private readonly JsonSerializerSettings settings = new() { DefaultValueHandling = DefaultValueHandling.Ignore };

        string IObjectSerializer.Serialize<T>(T value) => JsonConvert.SerializeObject(value, typeof(T), Formatting.Indented, settings);

        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, settings);
    }
}
