using Microsoft.Management.Deployment;
using System.Threading.Tasks;
using Windows.UI.Core;
using WinGetStore.Models;

namespace WinGetStore.ViewModels
{
    public sealed partial class VersionsViewModel(CatalogPackage catalogPackage, CoreDispatcher dispatcher) : ViewModelBase(dispatcher)
    {
        public PackageVersionSource PackageVersions { get; set => SetProperty(ref field, value); } = new(catalogPackage, dispatcher);
        public override Task Refresh(bool reset = false) => PackageVersions.Refresh(reset);
    }

    public sealed record CatalogPackageVersion(string Version, CatalogPackageMetadata PackageMetadata);
}
