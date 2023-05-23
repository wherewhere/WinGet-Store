using AppInstallerCaller;
using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels
{
    public class VersionsViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        public CatalogPackage CatalogPackage { get; }

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        private ObservableCollection<CatalogPackageVersion> packageVersions = new();
        public ObservableCollection<CatalogPackageVersion> PackageVersions
        {
            get => packageVersions;
            set => SetProperty(ref packageVersions, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property is null ? value is null : property.Equals(value)) { return; }
            property = value;
            _ = Dispatcher.EnqueueAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        public VersionsViewModel(CatalogPackage catalogPackage) => CatalogPackage = catalogPackage;

        public async Task Refresh()
        {
            IsLoading = true;
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                CatalogPackage.AvailableVersions.ToList().ForEach(async (x) =>
                {
                    PackageVersionInfo versionInfo = CatalogPackage.GetPackageVersionInfo(x);
                    CatalogPackageMetadata packageMetadata = versionInfo.GetCatalogPackageMetadata();
                    await Dispatcher.ResumeForegroundAsync();
                    PackageVersions.Add(new(versionInfo.Version, packageMetadata));
                });
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(VersionsViewModel)).Error(ex.ExceptionToMessage());
            }
            IsLoading = false;
        }
    }

    public record class CatalogPackageVersion(string Version, CatalogPackageMetadata PackageMetadata);
}
