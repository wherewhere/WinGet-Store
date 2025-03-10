using CommunityToolkit.Common.Helpers;
using MetroLog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using WinGetStore.Models;

namespace WinGetStore.Helpers
{
    public static partial class SettingsHelper
    {
        public const string UpdateDate = nameof(UpdateDate);
        public const string TileUpdateTime = nameof(TileUpdateTime);
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
                LocalObject.Save(UpdateDate, new DateTimeOffset());
            }
            if (!LocalObject.KeyExists(TileUpdateTime))
            {
                LocalObject.Save(TileUpdateTime, 120u);
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

    public static partial class SettingsHelper
    {
        public static readonly ILogManager LogManager = LogManagerFactory.CreateLogManager();
        public static readonly ApplicationDataStorageHelper LocalObject = ApplicationDataStorageHelper.GetCurrent(new SystemTextJsonObjectSerializer());

        static SettingsHelper() => SetDefaultSettings();
    }

    public class SystemTextJsonObjectSerializer : IObjectSerializer
    {
        public string Serialize<T>(T value) => value switch
        {
            uint => JsonSerializer.Serialize(value, SourceGenerationContext.Default.UInt32),
            string => JsonSerializer.Serialize(value, SourceGenerationContext.Default.String),
            ElementTheme => JsonSerializer.Serialize(value, SourceGenerationContext.Default.ElementTheme),
            DateTimeOffset => JsonSerializer.Serialize(value, SourceGenerationContext.Default.DateTimeOffset),
            _ => default
        };

        public T Deserialize<T>([StringSyntax(StringSyntaxAttribute.Json)] string value)
        {
            if (string.IsNullOrEmpty(value)) { return default; }
            Type type = typeof(T);
            return type == typeof(uint) ? Deserialize(value, SourceGenerationContext.Default.UInt32)
                : type == typeof(string) ? Deserialize(value, SourceGenerationContext.Default.String)
                : type == typeof(ElementTheme) ? Deserialize(value, SourceGenerationContext.Default.ElementTheme)
                : type == typeof(DateTimeOffset) ? Deserialize(value, SourceGenerationContext.Default.DateTimeOffset)
                : default;
            static T Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json, JsonTypeInfo<TValue> jsonTypeInfo) => JsonSerializer.Deserialize(json, jsonTypeInfo) is T value ? value : default;
        }
    }

    [JsonSerializable(typeof(uint))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(ElementTheme))]
    [JsonSerializable(typeof(DateTimeOffset))]
#if CANARY
    [JsonSerializable(typeof(ArtifactsInfo))]
    [JsonSerializable(typeof(RunInfo))]
#else
    [JsonSerializable(typeof(UpdateInfo))]
#endif
    public partial class SourceGenerationContext : JsonSerializerContext;
}
