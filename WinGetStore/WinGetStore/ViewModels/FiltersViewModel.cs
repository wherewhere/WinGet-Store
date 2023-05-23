using AppInstallerCaller;
using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.System;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels
{
    public class FiltersViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        private ObservableCollection<PackageMatchFilter> packageMatchFilter = new();
        public ObservableCollection<PackageMatchFilter> PackageMatchFilters
        {
            get => packageMatchFilter;
            set => SetProperty(ref packageMatchFilter, value);
        }

        public string Value { get; set; }
        public PackageMatchField Field { get; set; } = PackageMatchField.Id;
        public PackageFieldMatchOption Option { get; set; } = PackageFieldMatchOption.ContainsCaseInsensitive;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property is null ? value is null : property.Equals(value)) { return; }
            property = value;
            _ = Dispatcher.EnqueueAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        public FiltersViewModel(IList<PackageMatchFilter> packageMatchFilters) => PackageMatchFilters = new(packageMatchFilters);

        public void AddField()
        {
            PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
            filter.Field = Field;
            filter.Option = Option;
            filter.Value = Value;
            PackageMatchFilters.Add(filter);
        }
    }
}
