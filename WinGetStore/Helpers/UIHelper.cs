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

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new StringBuilder().AppendLine();
            if (!string.IsNullOrWhiteSpace(ex.Message)) { _ = builder.AppendLine($"Message: {ex.Message}"); }
            _ = builder.AppendLine($"HResult: {ex.HResult} (0x{ex.HResult:X})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { _ = builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { _ = builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }

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
        /// Extension method for <see cref="CoreDispatcher"/>. Offering an actual awaitable <see cref="Task{T}"/> with optional result that will be executed on the given dispatcher.
        /// </summary>
        /// <typeparam name="T">Returned data type of the function.</typeparam>
        /// <param name="dispatcher">Dispatcher of a thread to run <paramref name="function"/>.</param>
        /// <param name="function"> Function to be executed on the given dispatcher.</param>
        /// <param name="priority">Dispatcher execution priority, default is normal.</param>
        /// <returns>An awaitable <see cref="Task{T}"/> for the operation.</returns>
        /// <remarks>If the current thread has UI access, <paramref name="function"/> will be invoked directly.</remarks>
        public static Task<T> AwaitableRunAsync<T>(this CoreDispatcher dispatcher, Func<T> function, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            ArgumentNullException.ThrowIfNull(function);

            // Skip the dispatch, if possible
            if (dispatcher.HasThreadAccess)
            {
                try
                {
                    return Task.FromResult(function());
                }
                catch (Exception e)
                {
                    return Task.FromException<T>(e);
                }
            }

            TaskCompletionSource<T> taskCompletionSource = new();

            _ = dispatcher.RunAsync(priority, () =>
            {
                try
                {
                    taskCompletionSource.SetResult(function());
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            });

            return taskCompletionSource.Task;
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
                SettingsHelper.LogManager.CreateLogger(nameof(UIHelper)).LogWarning(ex, "{message} (0x{hResult:X})", ex.Message, ex.HResult);
            }
            return false;
        }
    }
}
