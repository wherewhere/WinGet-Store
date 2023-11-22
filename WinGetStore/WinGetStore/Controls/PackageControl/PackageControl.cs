using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using WinGetStore.Common;
using WinGetStore.Helpers;
using WinGetStore.WinRT;

namespace WinGetStore.Controls
{
    public class PackageControl : Control
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("PackageControl");

        private const string ActionButtonName = "ActionButton";
        private const string InstallProgressControlName = "InstallProgressControl";

        private ButtonBase ActionButton;
        private ButtonBase InstallProgressControl;

        private IAsyncInfo Progress;

        /// <summary>
        /// Creates a new instance of the <see cref="QRCode"/> class.
        /// </summary>
        public PackageControl()
        {
            DefaultStyleKey = typeof(PackageControl);
            SetValue(TemplateSettingsProperty, new PackageControlTemplateSettings());
        }

        #region Flyout

        /// <summary>
        /// Identifies the <see cref="Flyout"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FlyoutProperty =
            DependencyProperty.Register(
                nameof(Flyout),
                typeof(FlyoutBase),
                typeof(PackageControl),
                null);

        /// <summary>
        /// Gets or sets the Flyout.
        /// </summary>
        public FlyoutBase Flyout
        {
            get => (FlyoutBase)GetValue(FlyoutProperty);
            set => SetValue(FlyoutProperty, value);
        }

        #endregion

        #region CatalogPackage

        /// <summary>
        /// Identifies the <see cref="CatalogPackage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CatalogPackageProperty =
            DependencyProperty.Register(
                nameof(CatalogPackage),
                typeof(CatalogPackage),
                typeof(PackageControl),
                new PropertyMetadata(null, OnCatalogPackagePropertyChanged));

        /// <summary>
        /// Gets or sets the CatalogPackage.
        /// </summary>
        public CatalogPackage CatalogPackage
        {
            get => (CatalogPackage)GetValue(CatalogPackageProperty);
            set => SetValue(CatalogPackageProperty, value);
        }

        private static void OnCatalogPackagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _ = ((PackageControl)d).UpdateCatalogPackageAsync();
        }

        #endregion

        #region TemplateSettings

        /// <summary>
        /// Identifies the <see cref="TemplateSettings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(PackageControlTemplateSettings),
                typeof(PackageControl),
                null);

        /// <summary>
        /// Gets or sets the TemplateSettings.
        /// </summary>
        public PackageControlTemplateSettings TemplateSettings
        {
            get => (PackageControlTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ActionButton != null)
            {
                ActionButton.Click -= ActionButton_Click;
            }

            if (InstallProgressControl != null)
            {
                InstallProgressControl.Click -= ActionButton_Click;
            }

            TemplateSettings.ShouldUpdatePackage -= TemplateSettings_ShouldUpdatePackage;

            ActionButton = GetTemplateChild(ActionButtonName) as ButtonBase;
            InstallProgressControl = GetTemplateChild(InstallProgressControlName) as ButtonBase;

            if (ActionButton != null)
            {
                ActionButton.Click += ActionButton_Click;
            }

            if (InstallProgressControl != null)
            {
                InstallProgressControl.Click += ActionButton_Click;
            }

            TemplateSettings.ShouldUpdatePackage += TemplateSettings_ShouldUpdatePackage;
        }

        private async void TemplateSettings_ShouldUpdatePackage(object sender, PackageState e)
        {
            await Dispatcher.ResumeForegroundAsync();
            CatalogPackage catalogPackage = await GetPackageByIDAsync(CatalogPackage.Id);
            if (catalogPackage != null)
            {
                CatalogPackage = catalogPackage;
            }
        }

        private async Task UpdateCatalogPackageAsync()
        {
            try
            {
                CatalogPackage package = CatalogPackage;

                if (package.DefaultInstallVersion == null)
                {
                    CatalogPackage catalogPackage = await GetPackageByIDAsync(package.Id);
                    if (catalogPackage != null)
                    {
                        CatalogPackage = catalogPackage;
                        return;
                    }
                }

                PackageControlTemplateSettings templateSettings = TemplateSettings;
                templateSettings.PackageState = package.InstalledVersion != null
                    ? package.IsUpdateAvailable ? PackageState.UpdateAvailable : PackageState.Installed
                    : PackageState.Nominal;

                await ThreadSwitcher.ResumeBackgroundAsync();

                InstallOptions installOptions = WinGetProjectionFactory.TryCreateInstallOptions();
                PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();

                PackageInstallerInfo installerInfo = package.DefaultInstallVersion.GetApplicableInstaller(installOptions);

                if (package.DefaultInstallVersion != null)
                {
                    PackageCatalogInfo info = package.DefaultInstallVersion.PackageCatalog.Info;
                    IAsyncOperationWithProgress<InstallResult, InstallProgress> InstallProgress = packageManager.GetInstallProgress(package, info);
                    if (InstallProgress != null)
                    {
                        _ = package.IsUpdateAvailable
                            ? RegisterUpgradeProgressAsync(InstallProgress)
                            : RegisterInstallProgressAsync(InstallProgress);
                        await Dispatcher.ResumeForegroundAsync();
                        templateSettings.PackageState = PackageState.Installing;
                    }
                    else
                    {
                        IAsyncOperationWithProgress<UninstallResult, UninstallProgress> UninstallProgress = packageManager.GetUninstallProgress(package, info);
                        if (UninstallProgress != null)
                        {
                            _ = RegisterUninstallProgressAsync(UninstallProgress);
                            await Dispatcher.ResumeForegroundAsync();
                            templateSettings.PackageState = PackageState.Uninstalling;
                        }
                    }
                }

                await Dispatcher.ResumeForegroundAsync();
                templateSettings.InstallerType = installerInfo.InstallerType;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(PackageControl)).Error(ex.ExceptionToMessage());
#if DEBUG && !DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
                if (System.Diagnostics.Debugger.IsAttached) { System.Diagnostics.Debugger.Break(); }
#endif
            }
        }

        private async Task InstallPackageAsync(CatalogPackage package)
        {
            PackageControlTemplateSettings templateSettings = TemplateSettings;
            templateSettings.PackageState = PackageState.Installing;

            await ThreadSwitcher.ResumeBackgroundAsync();
            IAsyncOperationWithProgress<InstallResult, InstallProgress> progress = GetInstallOperation(package);
            await RegisterInstallProgressAsync(progress).ConfigureAwait(false);
        }

        private async Task RegisterInstallProgressAsync(IAsyncOperationWithProgress<InstallResult, InstallProgress> progress)
        {
            Progress = progress;
            progress.Progress = (sender, args) => _ = Dispatcher.AwaitableRunAsync(() => TemplateSettings.InstallProgress = args);

            long installOperationHr = 0L;
            string errorMessage = _loader.GetString("UnknownError");
            InstallResult installResult = null;
            try
            {
                installResult = await progress;
            }
            catch (OperationCanceledException)
            {
                errorMessage = _loader.GetString("Cancelled");
            }
            catch (Exception ex)
            {
                // Operation failed
                // Example: HRESULT_FROM_WIN32(ERROR_DISK_FULL).
                installOperationHr = ex.HResult;
                // Example: "There is not enough space on the disk."
                errorMessage = ex.Message;
                SettingsHelper.LogManager.GetLogger(nameof(PackageControl)).Warn(ex.ExceptionToMessage());
            }
            finally
            {
                Progress = null;
            }

            // Switch back to ui thread context.
            await Dispatcher.ResumeForegroundAsync();

            PackageControlTemplateSettings templateSettings = TemplateSettings;

            if (progress.Status == AsyncStatus.Canceled)
            {
                templateSettings.ProgressStatusText = _loader.GetString("InstallCancelled");
                templateSettings.PackageState = PackageState.Nominal;
            }
            else if (progress.Status == AsyncStatus.Error || installResult == null)
            {
                templateSettings.ActionButtonText = _loader.GetString("Retry");
                templateSettings.ProgressStatusText = errorMessage;
                templateSettings.PackageState = PackageState.InstallError;
            }
            else if (installResult.Status == InstallResultStatus.Ok)
            {
                templateSettings.PackageState = PackageState.Installed;
            }
            else
            {
                string failText = string.Format(_loader.GetString("InstallFailed"), installResult.Status, installResult.ExtendedErrorCode, installResult.InstallerErrorCode);
                templateSettings.ActionButtonText = _loader.GetString("Retry");
                templateSettings.ProgressStatusText = failText;
                templateSettings.PackageState = PackageState.InstallError;
            }
        }

        private IAsyncOperationWithProgress<InstallResult, InstallProgress> GetInstallOperation(CatalogPackage package)
        {
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            InstallOptions installOptions = WinGetProjectionFactory.TryCreateInstallOptions();

            // Passing PackageInstallScope::User causes the install to fail if there's no installer that supports that.
            installOptions.PackageInstallScope = PackageInstallScope.Any;
            installOptions.PackageInstallMode = PackageInstallMode.Silent;

            return packageManager.InstallPackageAsync(package, installOptions);
        }

        private async Task UpgradePackageAsync(CatalogPackage package)
        {
            PackageControlTemplateSettings templateSettings = TemplateSettings;
            templateSettings.PackageState = PackageState.Installing;

            await ThreadSwitcher.ResumeBackgroundAsync();
            IAsyncOperationWithProgress<InstallResult, InstallProgress> progress = GetUpgradeOperation(package);
            await RegisterUpgradeProgressAsync(progress).ConfigureAwait(false);
        }

        private async Task RegisterUpgradeProgressAsync(IAsyncOperationWithProgress<InstallResult, InstallProgress> progress)
        {
            progress.Progress = (sender, args) => _ = Dispatcher.AwaitableRunAsync(() => TemplateSettings.InstallProgress = args);

            long installOperationHr = 0L;
            string errorMessage = _loader.GetString("UnknownError");
            InstallResult installResult = null;
            try
            {
                Progress = progress;
                installResult = await progress;
            }
            catch (OperationCanceledException)
            {
                errorMessage = _loader.GetString("Cancelled");
            }
            catch (Exception ex)
            {
                // Operation failed
                // Example: HRESULT_FROM_WIN32(ERROR_DISK_FULL).
                installOperationHr = ex.HResult;
                // Example: "There is not enough space on the disk."
                errorMessage = ex.Message;
                SettingsHelper.LogManager.GetLogger(nameof(PackageControl)).Warn(ex.ExceptionToMessage());
            }
            finally
            {
                Progress = null;
            }

            // Switch back to ui thread context.
            await Dispatcher.ResumeForegroundAsync();

            PackageControlTemplateSettings templateSettings = TemplateSettings;

            if (progress.Status == AsyncStatus.Canceled)
            {
                templateSettings.ProgressStatusText = _loader.GetString("UpgradeCancelled");
                templateSettings.PackageState = PackageState.UpdateAvailable;
            }
            else if (progress.Status == AsyncStatus.Error || installResult == null)
            {
                templateSettings.ActionButtonText = _loader.GetString("Retry");
                templateSettings.ProgressStatusText = errorMessage;
                templateSettings.PackageState = PackageState.UpdateError;
            }
            else if (installResult.Status == InstallResultStatus.Ok)
            {
                templateSettings.PackageState = PackageState.Installed;
            }
            else
            {
                string failText = string.Format(_loader.GetString("UpgradeFailed"), installResult.Status, installResult.ExtendedErrorCode, installResult.InstallerErrorCode);
                templateSettings.ActionButtonText = _loader.GetString("Retry");
                templateSettings.ProgressStatusText = failText;
                templateSettings.PackageState = PackageState.UpdateError;
            }
        }

        private IAsyncOperationWithProgress<InstallResult, InstallProgress> GetUpgradeOperation(CatalogPackage package)
        {
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            InstallOptions installOptions = WinGetProjectionFactory.TryCreateInstallOptions();

            // Passing PackageInstallScope::User causes the install to fail if there's no installer that supports that.
            installOptions.PackageInstallScope = PackageInstallScope.Any;
            installOptions.PackageInstallMode = PackageInstallMode.Silent;

            return packageManager.UpgradePackageAsync(package, installOptions);
        }

        private async Task UninstallPackageAsync(CatalogPackage package)
        {
            PackageControlTemplateSettings templateSettings = TemplateSettings;
            templateSettings.PackageState = PackageState.Uninstalling;

            await ThreadSwitcher.ResumeBackgroundAsync();
            IAsyncOperationWithProgress<UninstallResult, UninstallProgress> progress = GetUninstallOperation(package);
            await RegisterUninstallProgressAsync(progress).ConfigureAwait(false);
        }

        private async Task RegisterUninstallProgressAsync(IAsyncOperationWithProgress<UninstallResult, UninstallProgress> progress)
        {
            progress.Progress = (sender, args) => _ = Dispatcher.AwaitableRunAsync(() => TemplateSettings.UninstallProgress = args);

            long installOperationHr = 0L;
            string errorMessage = _loader.GetString("UnknownError");
            UninstallResult installResult = null;
            try
            {
                Progress = progress;
                installResult = await progress;
            }
            catch (OperationCanceledException)
            {
                errorMessage = _loader.GetString("Cancelled");
            }
            catch (Exception ex)
            {
                // Operation failed
                // Example: HRESULT_FROM_WIN32(ERROR_DISK_FULL).
                installOperationHr = ex.HResult;
                // Example: "There is not enough space on the disk."
                errorMessage = ex.Message;
                SettingsHelper.LogManager.GetLogger(nameof(PackageControl)).Warn(ex.ExceptionToMessage());
            }
            finally
            {
                Progress = null;
            }

            // Switch back to ui thread context.
            await Dispatcher.ResumeForegroundAsync();

            PackageControlTemplateSettings templateSettings = TemplateSettings;

            if (progress.Status == AsyncStatus.Canceled)
            {
                templateSettings.ProgressStatusText = _loader.GetString("UninstallCancelled");
                templateSettings.PackageState = CatalogPackage.IsUpdateAvailable
                    ? PackageState.UpdateAvailable
                    : PackageState.Installed;
            }
            else if (progress.Status == AsyncStatus.Error || installResult == null)
            {
                templateSettings.ActionButtonText = _loader.GetString("Retry");
                templateSettings.ProgressStatusText = errorMessage;
                templateSettings.PackageState = PackageState.UninstallError;
            }
            else if (installResult.Status == UninstallResultStatus.Ok)
            {
                templateSettings.PackageState = PackageState.Nominal;
            }
            else
            {
                string failText = string.Format(_loader.GetString("UninstallFailed"), installResult.Status, installResult.ExtendedErrorCode, installResult.UninstallerErrorCode);
                templateSettings.ActionButtonText = _loader.GetString("Retry");
                templateSettings.ProgressStatusText = failText;
                templateSettings.PackageState = PackageState.UninstallError;
            }
        }

        private IAsyncOperationWithProgress<UninstallResult, UninstallProgress> GetUninstallOperation(CatalogPackage package)
        {
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            UninstallOptions uninstallOptions = WinGetProjectionFactory.TryCreateUninstallOptions();

            // Passing PackageInstallScope::User causes the install to fail if there's no installer that supports that.
            uninstallOptions.PackageUninstallScope = PackageUninstallScope.Any;
            uninstallOptions.PackageUninstallMode = PackageUninstallMode.Silent;

            return packageManager.UninstallPackageAsync(package, uninstallOptions);
        }

        private async Task<CatalogPackage> GetPackageByIDAsync(string packageID)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
                PackageCatalogReference[] packageCatalogReferences = packageManager.GetPackageCatalogs()?.ToArray();
                CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
                createCompositePackageCatalogOptions.Catalogs.AddRange(packageCatalogReferences);
                PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                ConnectResult connectResult = await catalogRef.ConnectAsync();
                PackageCatalog catalog = connectResult.PackageCatalog;
                FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
                PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
                filter.Field = PackageMatchField.Id;
                filter.Option = PackageFieldMatchOption.Equals;
                filter.Value = packageID;
                findPackagesOptions.Filters.Add(filter);
                FindPackagesResult packagesResult = await catalog.FindPackagesAsync(findPackagesOptions);
                return packagesResult.Matches.ToArray().FirstOrDefault()?.CatalogPackage;
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(PackageControl)).Error(ex.ExceptionToMessage());
                return null;
            }
        }

        public void CheckToInstall()
        {
            if (CatalogPackage.InstalledVersion != null) { return; }
            _ = InstallPackageAsync(CatalogPackage);
        }

        public void CheckToUpgrade()
        {
            if (CatalogPackage.InstalledVersion == null) { return; }
            _ = UpgradePackageAsync(CatalogPackage);
        }

        public async void CheckToUninstall()
        {
            if (CatalogPackage.InstalledVersion == null) { return; }

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
            ContentDialog dialog = new()
            {
                Title = string.Format(_loader.GetString("UninstallTitle"), CatalogPackage.Name),
                Content = string.Format(_loader.GetString("UninstallContent"), CatalogPackage.Name),
                PrimaryButtonText = loader.GetString("Yes"),
                CloseButtonText = loader.GetString("No"),
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                _ = UninstallPackageAsync(CatalogPackage);
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            PackageControlTemplateSettings templateSettings = TemplateSettings;
            switch (templateSettings.PackageState)
            {
                case PackageState.Nominal:
                case PackageState.InstallError:
                    CheckToInstall();
                    break;
                case PackageState.Installed:
                case PackageState.UninstallError:
                    CheckToUninstall();
                    break;
                case PackageState.UpdateAvailable:
                case PackageState.UpdateError:
                    CheckToUpgrade();
                    break;
                case PackageState.Installing:
                case PackageState.Uninstalling:
                    Progress?.Cancel();
                    break;
                default:
                    break;
            }
        }
    }
}