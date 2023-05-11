using AppInstallerCaller;
using Microsoft.Management.Deployment;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;

namespace WinGetStore.ViewModels
{
    public class VersionsViewModel : INotifyPropertyChanged
    {
        public CatalogPackage CatalogPackage { get; }

        private ObservableCollection<CatalogPackageVersion> packageVersions = new();
        public ObservableCollection<CatalogPackageVersion> PackageVersions
        {
            get => packageVersions;
            set
            {
                if (packageVersions != value)
                {
                    packageVersions = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (!Dispatcher.HasThreadAccess)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public VersionsViewModel(CatalogPackage catalogPackage)
        {
            CatalogPackage = catalogPackage;
        }

        public async Task Refresh()
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
    }

    public record class CatalogPackageVersion(string Version, CatalogPackageMetadata PackageMetadata);
}
