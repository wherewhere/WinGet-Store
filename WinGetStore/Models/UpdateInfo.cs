using System;
using System.Text.Json.Serialization;

namespace WinGetStore.Models
{
#if CANARY
    public class ArtifactsInfo
    {
        [JsonPropertyName("total_count")]
        public long TotalCount { get; set; }
        [JsonPropertyName("artifacts")]
        public Artifact[] Artifacts { get; set; }
    }

    public class Artifact
    {
        [JsonPropertyName("id")]
        public long ID { get; set; }
        [JsonPropertyName("node_id")]
        public string NodeID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("size_in_bytes")]
        public long SizeInBytes { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("archive_download_url")]
        public string ArchiveDownloadUrl { get; set; }
        [JsonPropertyName("expired")]
        public bool Expired { get; set; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
        [JsonPropertyName("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }
        [JsonPropertyName("workflow_run")]
        public WorkflowRun WorkflowRun { get; set; }
    }

    public class WorkflowRun
    {
        [JsonPropertyName("id")]
        public long ID { get; set; }
        [JsonPropertyName("repository_id")]
        public long RepositoryID { get; set; }
        [JsonPropertyName("head_repository_id")]
        public long HeadRepositoryID { get; set; }
        [JsonPropertyName("head_branch")]
        public string HeadBranch { get; set; }
        [JsonPropertyName("head_sha")]
        public string HeadSHA { get; set; }
    }

    public class RunInfo
    {
        [JsonPropertyName("id")]
        public long ID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("node_id")]
        public string NodeID { get; set; }
        [JsonPropertyName("head_branch")]
        public string HeadBranch { get; set; }
        [JsonPropertyName("head_sha")]
        public string HeadSHA { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("display_title")]
        public string DisplayTitle { get; set; }
        [JsonPropertyName("run_number")]
        public long RunNumber { get; set; }
        [JsonPropertyName("_event")]
        public string Event { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("conclusion")]
        public string Conclusion { get; set; }
        [JsonPropertyName("workflow_id")]
        public long WorkflowID { get; set; }
        [JsonPropertyName("check_suite_id")]
        public long CheckSuiteID { get; set; }
        [JsonPropertyName("check_suite_node_id")]
        public string CheckSuiteNodeID { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("html_url")]
        public string HTMLUrl { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
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
        public long Size { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public string DownloadUrl { get; set; }
    }
#else
    public class UpdateInfo
    {
        [JsonPropertyName("url")]
        public string ApiUrl { get; set; }
        [JsonPropertyName("html_url")]
        public string ReleaseUrl { get; set; }
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }
        [JsonPropertyName("prerelease")]
        public bool IsPreRelease { get; set; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonPropertyName("published_at")]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonPropertyName("assets")]
        public Asset[] Assets { get; set; }
        [JsonPropertyName("body")]
        public string Changelog { get; set; }
        [JsonIgnore]
        public bool IsExistNewVersion { get; set; }
        [JsonIgnore]
        public SystemVersionInfo Version { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
        [JsonPropertyName("download_count")]
        public long DownloadCount { get; set; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
#endif
}
