using Microsoft.Management.Deployment;
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
        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

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

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher is DispatcherQueue dispatcher
                    && !(ThreadSwitcher.IsHasThreadAccessPropertyAvailable
                    && dispatcher.HasThreadAccess != false))
                {
                    await dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
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
