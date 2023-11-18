using Newtonsoft.Json;
using System;

namespace WinGetStore.Models
{
#if CANARY
    public class ArtifactsInfo
    {
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
        [JsonProperty("artifacts")]
        public Artifact[] Artifacts { get; set; }
    }

    public class Artifact
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("node_id")]
        public string NodeID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("size_in_bytes")]
        public int SizeInBytes { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("archive_download_url")]
        public string ArchiveDownloadUrl { get; set; }
        [JsonProperty("expired")]
        public bool Expired { get; set; }
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
        [JsonProperty("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }
        [JsonProperty("workflow_run")]
        public WorkflowRun WorkflowRun { get; set; }
    }

    public class WorkflowRun
    {
        [JsonProperty("id")]
        public long ID { get; set; }
        [JsonProperty("repository_id")]
        public int RepositoryID { get; set; }
        [JsonProperty("head_repository_id")]
        public int HeadRepositoryID { get; set; }
        [JsonProperty("head_branch")]
        public string HeadBranch { get; set; }
        [JsonProperty("head_sha")]
        public string HeadSHA { get; set; }
    }

    public class RunInfo
    {
        [JsonProperty("id")]
        public long ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("node_id")]
        public string NodeID { get; set; }
        [JsonProperty("head_branch")]
        public string HeadBranch { get; set; }
        [JsonProperty("head_sha")]
        public string HeadSHA { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("display_title")]
        public string DisplayTitle { get; set; }
        [JsonProperty("run_number")]
        public int RunNumber { get; set; }
        [JsonProperty("_event")]
        public string Event { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("conclusion")]
        public string Conclusion { get; set; }
        [JsonProperty("workflow_id")]
        public int WorkflowID { get; set; }
        [JsonProperty("check_suite_id")]
        public long CheckSuiteID { get; set; }
        [JsonProperty("check_suite_node_id")]
        public string CheckSuiteNodeID { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("html_url")]
        public string HTMLUrl { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateInfo
    {
        public string ReleaseUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public Asset[] Assets { get; set; }
        public bool IsExistNewVersion { get; set; }
        public SystemVersionInfo Version { get; set; }
    }

    public class Asset
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public string DownloadUrl { get; set; }
    }
#else
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
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
#endif
}
