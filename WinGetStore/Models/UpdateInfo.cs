using System;
using System.Text.Json.Serialization;

namespace WinGetStore.Models
{
#if CANARY
    public class WorkflowRunsInfo
    {
        [JsonPropertyName("total_count")]
        public long TotalCount { get; init; }
        [JsonPropertyName("workflow_runs")]
        public WorkflowRun[] WorkflowRuns { get; init; }
    }

    public class WorkflowRun
    {
        [JsonPropertyName("id")]
        public long ID { get; init; }
        [JsonPropertyName("name")]
        public string Name { get; init; }
        [JsonPropertyName("node_id")]
        public string NodeID { get; init; }
        [JsonPropertyName("head_branch")]
        public string HeadBranch { get; init; }
        [JsonPropertyName("head_sha")]
        public string HeadSHA { get; init; }
        [JsonPropertyName("path")]
        public string Path { get; init; }
        [JsonPropertyName("display_title")]
        public string DisplayTitle { get; init; }
        [JsonPropertyName("run_number")]
        public int RunNumber { get; init; }
        [JsonPropertyName("event")]
        public string Event { get; init; }
        [JsonPropertyName("status")]
        public string Status { get; init; }
        [JsonPropertyName("conclusion")]
        public string Conclusion { get; init; }
        [JsonPropertyName("workflow_id")]
        public int WorkflowID { get; init; }
        [JsonPropertyName("check_suite_id")]
        public long CheckSuiteID { get; init; }
        [JsonPropertyName("check_suite_node_id")]
        public string CheckSuiteNodeID { get; init; }
        [JsonPropertyName("url")]
        public string Url { get; init; }
        [JsonPropertyName("html_url")]
        public string HTMLUrl { get; init; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }
        [JsonPropertyName("run_started_at")]
        public DateTimeOffset RunStartedAt { get; init; }
        [JsonPropertyName("artifacts_url")]
        public string ArtifactsUrl { get; init; }
    }

    public class ArtifactsInfo
    {
        [JsonPropertyName("total_count")]
        public long TotalCount { get; init; }
        [JsonPropertyName("artifacts")]
        public Artifact[] Artifacts { get; init; }
    }

    public class Artifact
    {
        [JsonPropertyName("id")]
        public long ID { get; init; }
        [JsonPropertyName("node_id")]
        public string NodeID { get; init; }
        [JsonPropertyName("name")]
        public string Name { get; init; }
        [JsonPropertyName("size_in_bytes")]
        public long SizeInBytes { get; init; }
        [JsonPropertyName("url")]
        public string Url { get; init; }
        [JsonPropertyName("archive_download_url")]
        public string ArchiveDownloadUrl { get; init; }
        [JsonPropertyName("expired")]
        public bool Expired { get; init; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }
        [JsonPropertyName("expires_at")]
        public DateTimeOffset ExpiresAt { get; init; }
    }

    public class UpdateInfo
    {
        public string ReleaseUrl { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset PublishedAt { get; init; }
        public Asset[] Assets { get; init; }
        public bool IsExistNewVersion { get; init; }
        public SystemVersionInfo Version { get; init; }
    }

    public class Asset
    {
        public string Url { get; init; }
        public string Name { get; init; }
        public long Size { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
        public string DownloadUrl { get; init; }
    }
#else
    public class UpdateInfo
    {
        [JsonPropertyName("url")]
        public string ApiUrl { get; init; }
        [JsonPropertyName("html_url")]
        public string ReleaseUrl { get; init; }
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; }
        [JsonPropertyName("prerelease")]
        public bool IsPreRelease { get; init; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }
        [JsonPropertyName("published_at")]
        public DateTimeOffset PublishedAt { get; init; }
        [JsonPropertyName("assets")]
        public Asset[] Assets { get; init; }
        [JsonPropertyName("body")]
        public string Changelog { get; init; }
        [JsonIgnore]
        public bool IsExistNewVersion { get; set; }
        [JsonIgnore]
        public SystemVersionInfo Version { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("url")]
        public string Url { get; init; }
        [JsonPropertyName("name")]
        public string Name { get; init; }
        [JsonPropertyName("size")]
        public long Size { get; init; }
        [JsonPropertyName("download_count")]
        public long DownloadCount { get; init; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }
        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; init; }
    }
#endif
}
