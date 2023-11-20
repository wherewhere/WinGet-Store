using Microsoft.Management.Deployment;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;
using WinGetStore.Common;
using WinGetStore.Models;

namespace WinGetStore.ViewModels
{
    public class VersionsViewModel(CatalogPackage catalogPackage) : INotifyPropertyChanged
    {
        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        private PackageVersionSource packageVersions = new(catalogPackage);
        public PackageVersionSource PackageVersions
        {
            get => packageVersions;
            set => SetProperty(ref packageVersions, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
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

        public Task Refresh(bool reset = false) => PackageVersions.Refresh(reset);
    }

    public record class CatalogPackageVersion(string Version, CatalogPackageMetadata PackageMetadata);
}
