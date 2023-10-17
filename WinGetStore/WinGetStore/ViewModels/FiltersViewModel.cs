using Microsoft.Management.Deployment;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.System;
using WinGetStore.Helpers;
using WinGetStore.WinRT;

namespace WinGetStore.ViewModels
{
    public class FiltersViewModel : INotifyPropertyChanged
    {
        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        private ObservableCollection<PackageMatchFilter> packageMatchFilters = [];
        public ObservableCollection<PackageMatchFilter> PackageMatchFilters
        {
            get => packageMatchFilters;
            set => SetProperty(ref packageMatchFilters, value);
        }

        public string Value { get; set; }
        public PackageMatchField Field { get; set; } = PackageMatchField.Id;
        public PackageFieldMatchOption Option { get; set; } = PackageFieldMatchOption.ContainsCaseInsensitive;

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

        public FiltersViewModel(IList<PackageMatchFilter> packageMatchFilters) => PackageMatchFilters = new(packageMatchFilters);

        public void AddField()
        {
            PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
            filter.Field = Field;
            filter.Option = Option;
            filter.Value = Value;
            packageMatchFilters.Add(filter);
        }
    }
}
