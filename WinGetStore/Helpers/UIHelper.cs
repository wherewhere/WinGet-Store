using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinGetStore.Common;

namespace WinGetStore.Helpers
{
    public static class UIHelper
    {
        [SupportedOSPlatformGuard("windows10.0.10240.0")]
        public static bool IsWindows10OrGreater { get; } = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240);

        public static object GetMessage(this Exception ex) => ex.Message is { Length: > 0 } message ? message : ex.GetType();

        public static TResult AwaitByTaskCompleteSource<TResult>(this Task<TResult> function, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Run(async () =>
            {
                try
                {
                    TResult result = await function.ConfigureAwait(false);
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }, cancellationToken);
            TResult taskResult = task.Result;
            return taskResult;
        }

        /// <summary>
        /// Returns a string representation of a version with the format 'Major.Minor.Build.Revision'.
        /// </summary>
        /// <param name="packageVersion">The <see cref="PackageVersion"/> to convert to a string</param>
        /// <param name="significance">The number of version numbers to return, default is 4 for the full version number.</param>
        /// <returns>Version string of the format 'Major.Minor.Build.Revision'</returns>
        /// <example>
        /// Package.Current.Id.Version.ToFormattedString(2); // Returns "7.0" for instance.
        /// </example>
        public static string ToFormattedString(this PackageVersion packageVersion, int significance = 4) => significance switch
        {
            4 => $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}",
            3 => $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}",
            2 => $"{packageVersion.Major}.{packageVersion.Minor}",
            1 => $"{packageVersion.Major}",
            _ => throw new ArgumentOutOfRangeException(nameof(significance), "Value must be a value 1 through 4."),
        };

        public static async Task<T> GetValueAsync<T>(this DependencyObject element, DependencyProperty dp)
        {
            await element.Dispatcher.ResumeForegroundAsync();
            return (T)element.GetValue(dp);
        }

        public static async Task SetValueAsync<T>(this DependencyObject element, DependencyProperty dp, T value)
        {
            await element.Dispatcher.ResumeForegroundAsync();
            element.SetValue(dp, value);
        }

        public static bool TryGetUri(this string url, out Uri uri)
        {
            uri = default;
            if (string.IsNullOrWhiteSpace(url)) { return false; }
            try
            {
                return url.Contains(':')
                    ? Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri)
                    : Uri.TryCreate($"https://{url}", UriKind.RelativeOrAbsolute, out uri);
            }
            catch (FormatException ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger(typeof(UIHelper)).LogWarning(ex, "\"{url}\" is not a URL. {message} (0x{hResult:X})", url, ex.GetMessage(), ex.HResult);
            }
            return false;
        }
    }
}
