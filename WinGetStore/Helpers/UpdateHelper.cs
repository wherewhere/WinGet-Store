using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using WinGetStore.Models;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace WinGetStore.Helpers
{
    public static class UpdateHelper
    {
#if CANARY
        private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/actions/runs";

        public static ValueTask<UpdateInfo> CheckUpdateAsync(string username, string repository)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(username, repository, currentVersion);
        }

        public static async ValueTask<UpdateInfo> CheckUpdateAsync(string username, string repository, PackageVersion currentVersion)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentNullException(nameof(repository));
            }

            using HttpClientHandler clientHandler = new();
            using HttpClient client = new(clientHandler);

            using (HttpBaseProtocolFilter filter = new())
            {
                Uri host = new("https://api.github.com");
                HttpCookieManager cookieManager = filter.CookieManager;
                foreach (HttpCookie item in cookieManager.GetCookies(host))
                {
                    clientHandler.CookieContainer.SetCookies(host, item.ToString());
                }
            }

            client.DefaultRequestHeaders.Add("User-Agent", username);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string url = string.Format(GITHUB_API, username, repository);
            HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            if (response.StatusCode != HttpStatusCode.OK) { return null; }
            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            WorkflowRunsInfo result = JsonSerializer.Deserialize(responseBody, SourceGenerationContext.Default.WorkflowRunsInfo);

            if (result != null)
            {
                WorkflowRun run = result.WorkflowRuns.FirstOrDefault(x => x is { HeadBranch: "main", Event: "push", Conclusion: "success" });
                if (run != null)
                {
                    try
                    {
                        response = await client.GetAsync(run.ArtifactsUrl).ConfigureAwait(false);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            ArtifactsInfo artifacts = JsonSerializer.Deserialize(responseBody, SourceGenerationContext.Default.ArtifactsInfo);
                            if (artifacts != null)
                            {
                                SystemVersionInfo newVersionInfo = GetAsVersionInfo(run.CreatedAt, run.RunNumber);
                                return new UpdateInfo()
                                {
                                    ReleaseUrl = run.HTMLUrl,
                                    CreatedAt = run.CreatedAt,
                                    PublishedAt = run.UpdatedAt,
                                    Assets =
                                    [
                                        .. artifacts.Artifacts.Select(artifact => new Asset
                                        {
                                            Url = artifact.Url,
                                            Name = artifact.Name,
                                            Size = artifact.SizeInBytes,
                                            CreatedAt = artifact.CreatedAt,
                                            UpdatedAt = artifact.UpdatedAt,
                                            ExpiresAt = artifact.ExpiresAt,
                                            DownloadUrl = $"https://github.com/{username}/{repository}/suites/{run.CheckSuiteID}/artifacts/{artifact.ID}"
                                        })
                                    ],
                                    IsExistNewVersion = newVersionInfo > currentVersion,
                                    Version = newVersionInfo
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(SettingsHelper.LoggerFactory.CreateLogger(nameof(UpdateHelper)), ex, "{message} (0x{hResult:X})", ex.Message, ex.HResult);
                    }
                }
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(DateTimeOffset dateTimeOffset, int id)
        {
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            int major = int.Parse(dateTime.ToString("yy"));
            int minor = int.Parse(dateTime.ToString("MMdd"));
            return new SystemVersionInfo(major, minor, id);
        }
#else
        private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases/latest";

        public static ValueTask<UpdateInfo> CheckUpdateAsync(string username, string repository)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(username, repository, currentVersion);
        }

        public static async ValueTask<UpdateInfo> CheckUpdateAsync(string username, string repository, PackageVersion currentVersion)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentNullException(nameof(repository));
            }

            using HttpClientHandler clientHandler = new();
            using HttpClient client = new(clientHandler);

            using (HttpBaseProtocolFilter filter = new())
            {
                Uri host = new("https://api.github.com");
                HttpCookieManager cookieManager = filter.CookieManager;
                foreach (HttpCookie item in cookieManager.GetCookies(host))
                {
                    clientHandler.CookieContainer.SetCookies(host, item.ToString());
                }
            }

            client.DefaultRequestHeaders.Add("User-Agent", username);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string url = string.Format(GITHUB_API, username, repository);
            HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            if (response.StatusCode != HttpStatusCode.OK) { return null; }
            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            UpdateInfo result = JsonSerializer.Deserialize(responseBody, SourceGenerationContext.Default.UpdateInfo);

            if (result != null)
            {
                SystemVersionInfo newVersionInfo = GetAsVersionInfo(result.TagName);
                result.IsExistNewVersion = newVersionInfo > currentVersion;
                result.Version = newVersionInfo;
                return result;
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(string version)
        {
            ReadOnlySpan<int> numbs = [.. GetVersionNumbers(version).Split('.', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)];
            return numbs switch
            {
                { Length: 0 } => new SystemVersionInfo(0, 0, 0, 0),
                { Length: 1 } => new SystemVersionInfo(numbs[0], 0, 0, 0),
                { Length: 2 } => new SystemVersionInfo(numbs[0], numbs[1], 0, 0),
                { Length: 3 } => new SystemVersionInfo(numbs[0], numbs[1], numbs[2], 0),
                { Length: >= 4 } => new SystemVersionInfo(numbs[0], numbs[1], numbs[2], numbs[3]),
            };
        }

        private static string GetVersionNumbers(string version)
        {
            const string allowedChars = "01234567890.";
            return new string([.. version.Where(allowedChars.Contains)]);
        }
#endif
    }
}
