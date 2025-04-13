using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Security.Authorization.AppCapabilityAccess;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinGetStore.Common;
using WinGetStore.Helpers;
using WinGetStore.Pages;

namespace WinGetStore
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        [SupportedOSPlatformGuard("Windows10.0.18362.0")]
        public static bool IsAppCapabilitySupported { get; } = UIHelper.IsWindows10OrGreater && ApiInformation.IsTypePresent("Windows.Security.Authorization.AppCapabilityAccess.AppCapability");

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            UnhandledException += Application_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox") { FocusVisualKind = FocusVisualKind.Reveal; }
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            if (SynchronizationContext.Current == null)
            {
                DispatcherQueueSynchronizationContext context = new(args.Window.CoreWindow.DispatcherQueue);
                SynchronizationContext.SetSynchronizationContext(context);
            }
            base.OnWindowCreated(args);
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureWindow(e);
        }

        #region OnActivated

        protected override void OnActivated(IActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnActivated(e);
        }

        protected override void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnCachedFileUpdaterActivated(e);
        }

        protected override void OnFileActivated(FileActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnFileActivated(e);
        }

        protected override void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnFileOpenPickerActivated(e);
        }

        protected override void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnFileSavePickerActivated(e);
        }

        protected override void OnSearchActivated(SearchActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnSearchActivated(e);
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs e)
        {
            EnsureWindow(e);
            base.OnShareTargetActivated(e);
        }

        #endregion

        private void EnsureWindow(IActivatedEventArgs e)
        {
            RegisterExceptionHandlingSynchronizationContext();

            if (!isLoaded)
            {
                _ = RequestPackageManagementAsync();
                _ = RegisterBackgroundTaskAsync();
                CheckWinGetDev();
                isLoaded = true;
            }

            Window window = Window.Current;
            WindowHelper.TrackWindow(window);

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (window.Content is not Frame rootFrame)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                window.Content = rootFrame;

                ThemeHelper.Initialize();
            }

            if (e is LaunchActivatedEventArgs args)
            {
                if (!args.PrelaunchActivated)
                {
                    CoreApplication.EnablePrelaunch(true);
                }
                else { return; }
            }

            if (rootFrame.Content == null)
            {
                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                _ = rootFrame.Navigate(typeof(MainPage), e, new DrillInNavigationTransitionInfo());
            }

            // 确保当前窗口处于活动状态
            window.Activate();
        }

        /// <summary>
        /// 导航到特定页失败时调用。
        /// </summary>
        ///<param name="sender">导航失败的框架。</param>
        ///<param name="e">有关导航失败的详细信息。</param>
        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private static void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private static void CheckWinGetDev()
        {
            if (!WinGetProjectionFactory.IsWinGetInstalled && WinGetProjectionFactory.IsWinGetDevInstalled)
            {
                WinGetProjectionFactory.IsUseDev = true;
            }
        }

        private static async Task RequestPackageManagementAsync()
        {
            if (IsAppCapabilitySupported)
            {
                AppCapability PackageManagement = AppCapability.Create("packageManagement");
                switch (PackageManagement.CheckAccess())
                {
                    case AppCapabilityAccessStatus.DeniedByUser:
                    case AppCapabilityAccessStatus.DeniedBySystem:
                        // Do something
                        _ = await PackageManagement.RequestAccessAsync();
                        break;
                }
            }
        }

        private static void Application_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger("Unhandled Exception - Application").LogError(ex, "Unhandled exception. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
            e.Handled = true;
        }

        private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger("Unhandled Exception - CurrentDomain").LogError(ex, "Unhandled exception. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
        }

        /// <summary>
        /// Should be called from OnActivated and OnLaunched.
        /// </summary>
        private static void RegisterExceptionHandlingSynchronizationContext()
        {
            if (ExceptionHandlingSynchronizationContext.TryRegister(out ExceptionHandlingSynchronizationContext context))
            {
                context.UnhandledException += SynchronizationContext_UnhandledException;
            }
        }

        private static void SynchronizationContext_UnhandledException(object sender, Common.UnhandledExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger("Unhandled Exception - SynchronizationContext").LogError(ex, "Unhandled exception. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
            }
            e.Handled = true;
        }

        private static async Task RegisterBackgroundTaskAsync()
        {
            if (!WinGetProjectionFactory.IsWinGetInstalled && !WinGetProjectionFactory.IsWinGetDevInstalled)
            { return; }

            // Check for background access (optional)
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            if (status is not BackgroundAccessStatus.Unspecified
                and not BackgroundAccessStatus.Denied
                and not BackgroundAccessStatus.DeniedByUser)
            { RegisterLiveTileTask(); }

            #region LiveTileTask

            const string LiveTileTask = "LiveTileTask";

            static void RegisterLiveTileTask()
            {
                uint time = SettingsHelper.Get<uint>(SettingsHelper.TileUpdateTime);
                if (time < 15)
                {
                    UnregisterLiveTileTask();
                    return;
                }

                // If background task is already registered, do nothing
                if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(LiveTileTask)))
                { return; }

                // Register (Single Process)
                BackgroundTaskRegistration _LiveTileTask = BackgroundTaskHelper.Register(LiveTileTask, new TimeTrigger(time, false), true);
            }

            static void UnregisterLiveTileTask()
            {
                // If background task is not registered, do nothing
                if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(LiveTileTask)))
                { return; }

                // Unregister (Single Process)
                BackgroundTaskHelper.Unregister(LiveTileTask);
            }

            #endregion
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case "LiveTileTask":
                    await TilesHelper.UpdateAvailablePackageAsync();
                    break;

                default:
                    break;
            }

            deferral.Complete();
        }

        private bool isLoaded;
    }
}
