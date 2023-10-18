using Newtonsoft.Json;
using System;

namespace WinGetStore.Models
{
    public class UpdateInfo
    {
        [JsonProperty("url")]
        public string ApiUrl { get; set; }
        [JsonProperty("html_url")]
        public string ReleaseUrl { get; set; }
        [JsonProperty("tag_name")]
        public string TagName { get; set; }
        [JsonProperty("prerelease")]
        public bool IsPreRelease { get; set; }
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonProperty("assets")]
        public Asset[] Assets { get; set; }
        [JsonProperty("body")]
        public string Changelog { get; set; }
        [JsonIgnore]
        public bool IsExistNewVersion { get; set; }
        [JsonIgnore]
        public SystemVersionInfo Version { get; set; }
    }

    public class Asset
    {
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("browser_download_url")]
        public string Url { get; set; }
    }
}
