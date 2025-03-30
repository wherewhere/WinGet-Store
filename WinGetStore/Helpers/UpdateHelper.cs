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
        private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/actions/artifacts";
        private const string GITHUB_RUNS_API = "https://api.github.com/repos/{0}/{1}/actions/runs/{2}";

        public static Task<UpdateInfo> CheckUpdateAsync(string username, string repository)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(username, repository, currentVersion);
        }

        public static async Task<UpdateInfo> CheckUpdateAsync(string username, string repository, PackageVersion currentVersion)
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
            response.EnsureSuccessStatusCode();
            if (response.StatusCode != HttpStatusCode.OK) { return null; }
            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            ArtifactsInfo result = JsonSerializer.Deserialize(responseBody, SourceGenerationContext.Default.ArtifactsInfo);

            if (result != null)
            {
                Artifact artifact = result.Artifacts.FirstOrDefault(x => x.WorkflowRun.HeadBranch == "main");

                if (artifact != null)
                {
                    UpdateInfo updateInfo = new()
                    {
                        CreatedAt = artifact.CreatedAt,
                        PublishedAt = artifact.UpdatedAt,
                        Assets =
                        [
                            new Asset
                            {
                                Url = artifact.Url,
                                Name = artifact.Name,
                                Size = artifact.SizeInBytes,
                                CreatedAt = artifact.CreatedAt,
                                UpdatedAt = artifact.UpdatedAt,
                                ExpiresAt = artifact.ExpiresAt,
                                DownloadUrl = artifact.ArchiveDownloadUrl
                            }
                        ]
                    };

                    try
                    {
                        url = string.Format(GITHUB_RUNS_API, username, repository, artifact.WorkflowRun.ID);
                        response = await client.GetAsync(url).ConfigureAwait(false);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            RunInfo run = JsonSerializer.Deserialize(responseBody, SourceGenerationContext.Default.RunInfo);

                            SystemVersionInfo newVersionInfo = GetAsVersionInfo(artifact.CreatedAt, (int)run.RunNumber);
                            updateInfo.ReleaseUrl = run.HTMLUrl;
                            updateInfo.IsExistNewVersion = newVersionInfo > currentVersion;
                            updateInfo.Version = newVersionInfo;
                            updateInfo.Assets[0].DownloadUrl = $"https://github.com/{username}/{repository}/suites/{run.CheckSuiteID}/artifacts/{artifact.ID}";
                        }
                    }
                    catch (Exception ex)
                    {
                        Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(SettingsHelper.LogManager.CreateLogger(nameof(UpdateHelper)), ex, "{message} (0x{hResult:X})", ex.Message, ex.HResult);
                    }

                    return updateInfo;
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

        public static Task<UpdateInfo> CheckUpdateAsync(string username, string repository)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(username, repository, currentVersion);
        }

        public static async Task<UpdateInfo> CheckUpdateAsync(string username, string repository, PackageVersion currentVersion)
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
            response.EnsureSuccessStatusCode();
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
            int[] numbs = [.. GetVersionNumbers(version).Split('.').Select(int.Parse)];
            return numbs.Length <= 1
                ? new SystemVersionInfo(numbs[0], 0, 0, 0)
                : numbs.Length <= 2
                    ? new SystemVersionInfo(numbs[0], numbs[1], 0, 0)
                    : numbs.Length <= 3
                        ? new SystemVersionInfo(numbs[0], numbs[1], numbs[2], 0)
                        : new SystemVersionInfo(numbs[0], numbs[1], numbs[2], numbs[3]);
        }

        private static string GetVersionNumbers(string version)
        {
            string allowedChars = "01234567890.";
            return new string([.. version.Where(allowedChars.Contains)]);
        }
#endif
    }
}
