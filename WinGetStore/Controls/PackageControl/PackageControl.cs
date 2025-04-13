using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using WinGetStore.Common;
using WinGetStore.Helpers;
using WinRT;

namespace WinGetStore.Controls
{
    public partial class PackageControl : Control
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("PackageControl");

        private const string ActionButtonName = "ActionButton";
        private const string InstallProgressControlName = "InstallProgressControl";

        private ButtonBase ActionButton;
        private ButtonBase InstallProgressControl;

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

        #region IsProcessing

        /// <summary>
        /// Identifies the <see cref="IsProcessing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsProcessingProperty =
            DependencyProperty.Register(
                nameof(IsProcessing),
                typeof(bool),
                typeof(PackageControl),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the IsProcessing.
        /// </summary>
        public bool IsProcessing
        {
            get => (bool)GetValue(IsProcessingProperty);
            private set => SetValue(IsProcessingProperty, value);
        }

        #endregion

        #region CatalogPackage

        /// <summary>
        /// Identifies the <see cref="CatalogPackage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CatalogPackageProperty =
            DependencyProperty.Register(
                nameof(CatalogPackage),
                typeof(BindableCatalogPackage),
                typeof(PackageControl),
                new PropertyMetadata(null, OnCatalogPackagePropertyChanged));

        /// <summary>
        /// Gets or sets the CatalogPackage.
        /// </summary>
        public BindableCatalogPackage CatalogPackage
        {
            get => (BindableCatalogPackage)GetValue(CatalogPackageProperty);
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

        private IAsyncInfo progress;
        public IAsyncInfo Progress
        {
            get => progress;
            set
            {
                if (progress != value)
                {
                    progress = value;
                    _ = this.SetValueAsync(IsProcessingProperty, progress != null);
                }
            }
        }

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
                PackageControlTemplateSettings templateSettings = TemplateSettings;

                if (package is { DefaultInstallVersion: null, InstalledVersion: null })
                {
                    CatalogPackage catalogPackage = await GetPackageByIDAsync(package.Id);
                    if (catalogPackage?.DefaultInstallVersion != null)
                    {
                        CatalogPackage = catalogPackage;
                        return;
                    }
                }

                templateSettings.PackageState = package.InstalledVersion != null
                    ? package.IsUpdateAvailable ? PackageState.UpdateAvailable : PackageState.Installed
                    : PackageState.Nominal;

                if (package.DefaultInstallVersion is PackageVersionInfo version)
                {
                    await ThreadSwitcher.ResumeBackgroundAsync();

                    InstallOptions installOptions = WinGetProjectionFactory.TryCreateInstallOptions();
                    PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();

                    PackageInstallerInfo installerInfo = version.GetApplicableInstaller(installOptions);
                    await templateSettings.SetValueAsync(PackageControlTemplateSettings.InstallerTypeProperty, installerInfo.InstallerType);

                    PackageCatalogInfo info = version.PackageCatalog.Info;
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
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<PackageControl>().LogError(ex, "Failed to update package info. {message} (0x{hResult:X})", ex.GetMessage(), ex.HResult);
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
            progress.Progress = (sender, args) => _ = Dispatcher.AwaitableRunAsync((Action)(() => TemplateSettings.InstallProgress = args));

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
                string id = await this.GetValueAsync<BindableCatalogPackage>(CatalogPackageProperty).ContinueWith(x => x.Result.Id);
                SettingsHelper.LoggerFactory.CreateLogger<PackageControl>().LogWarning(ex, "Failed to install app \"{id}\". {message} (0x{hResult:X})", id, ex.GetMessage(), ex.HResult);
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

        private static IAsyncOperationWithProgress<InstallResult, InstallProgress> GetInstallOperation(CatalogPackage package)
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
            progress.Progress = (sender, args) => _ = Dispatcher.AwaitableRunAsync((Action)(() => TemplateSettings.InstallProgress = args));

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
                string id = await this.GetValueAsync<BindableCatalogPackage>(CatalogPackageProperty).ContinueWith(x => x.Result.Id);
                SettingsHelper.LoggerFactory.CreateLogger<PackageControl>().LogWarning(ex, "Failed to upgrade app \"{id}\". {message} (0x{hResult:X})", id, ex.GetMessage(), ex.HResult);
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

        private static IAsyncOperationWithProgress<InstallResult, InstallProgress> GetUpgradeOperation(CatalogPackage package)
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
            progress.Progress = (sender, args) => _ = Dispatcher.AwaitableRunAsync((Action)(() => TemplateSettings.UninstallProgress = args));

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
                string id = await this.GetValueAsync<BindableCatalogPackage>(CatalogPackageProperty).ContinueWith(x => x.Result.Id);
                SettingsHelper.LoggerFactory.CreateLogger<PackageControl>().LogWarning(ex, "Failed to uninstall app \"{id}\". {message} (0x{hResult:X})", id, ex.GetMessage(), ex.HResult);
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

        private static IAsyncOperationWithProgress<UninstallResult, UninstallProgress> GetUninstallOperation(CatalogPackage package)
        {
            PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
            UninstallOptions uninstallOptions = WinGetProjectionFactory.TryCreateUninstallOptions();

            // Passing PackageInstallScope::User causes the install to fail if there's no installer that supports that.
            uninstallOptions.PackageUninstallScope = PackageUninstallScope.Any;
            uninstallOptions.PackageUninstallMode = PackageUninstallMode.Silent;

            return packageManager.UninstallPackageAsync(package, uninstallOptions);
        }

        private static async Task<CatalogPackage> GetPackageByIDAsync(string packageID)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                PackageManager packageManager = WinGetProjectionFactory.TryCreatePackageManager();
                IReadOnlyList<PackageCatalogReference> packageCatalogReferences = packageManager.GetPackageCatalogs();
                CreateCompositePackageCatalogOptions createCompositePackageCatalogOptions = WinGetProjectionFactory.TryCreateCreateCompositePackageCatalogOptions();
                createCompositePackageCatalogOptions.Catalogs.AddRange(packageCatalogReferences.AsReader());
                PackageCatalogReference catalogRef = packageManager.CreateCompositePackageCatalog(createCompositePackageCatalogOptions);
                ConnectResult connectResult = await catalogRef.ConnectAsync();
                PackageCatalog catalog = connectResult.PackageCatalog;
                FindPackagesOptions findPackagesOptions = WinGetProjectionFactory.TryCreateFindPackagesOptions();
                PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
                filter.Field = PackageMatchField.Id;
                filter.Option = PackageFieldMatchOption.Equals;
                filter.Value = packageID;
                findPackagesOptions.Selectors.Add(filter);
                FindPackagesResult packagesResult = await catalog.FindPackagesAsync(findPackagesOptions);
                return packagesResult.Matches.AsReader() is [MatchResult result, ..] ? result.CatalogPackage : default;
            }
            catch (Exception ex)
            {
                SettingsHelper.LoggerFactory.CreateLogger<PackageControl>().LogError(ex, "Failed to get package \"{id}\". {message} (0x{hResult:X})", packageID, ex.GetMessage(), ex.HResult);
                return null;
            }
        }

        public void CheckToInstall()
        {
            if (Progress != null || CatalogPackage.InstalledVersion != null) { return; }
            _ = InstallPackageAsync(CatalogPackage);
        }

        public void CheckToUpgrade()
        {
            if (Progress != null || CatalogPackage.InstalledVersion == null) { return; }
            _ = UpgradePackageAsync(CatalogPackage);
        }

        public async void CheckToUninstall()
        {
            if (Progress != null || CatalogPackage.InstalledVersion == null) { return; }

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

    [GeneratedBindableCustomProperty]
    public sealed partial class BindableCatalogPackage(CatalogPackage value)
    {
        private readonly CatalogPackage value = value;

        public bool IsUpdateAvailable => value != null && value.IsUpdateAvailable;
        public string Name => value?.Name;
        public string Id => value?.Id;
        public BindablePackageVersionInfo InstalledVersion => value?.InstalledVersion;
        public BindablePackageVersionInfo DefaultInstallVersion => value == null ? default : value.DefaultInstallVersion ?? value.InstalledVersion;

        public static implicit operator CatalogPackage(BindableCatalogPackage host) => host?.value;
        public static implicit operator BindableCatalogPackage(CatalogPackage value) => value == null ? null : new(value);
    }

    [GeneratedBindableCustomProperty]
    public sealed partial class BindablePackageVersionInfo(PackageVersionInfo value)
    {
        private readonly PackageVersionInfo value = value;

        public string Version => value?.Version;

        public static implicit operator PackageVersionInfo(BindablePackageVersionInfo host) => host?.value;
        public static implicit operator BindablePackageVersionInfo(PackageVersionInfo value) => value == null ? null : new(value);
    }
}