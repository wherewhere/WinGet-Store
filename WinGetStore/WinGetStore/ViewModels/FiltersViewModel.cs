using Microsoft.Management.Deployment;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.System;
using WinGetStore.Common;
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

        private string value;
        public string Value
        {
            get => value;
            set => SetProperty(ref this.value, value);
        }

        private PackageMatchField field = PackageMatchField.Id;
        public PackageMatchField Field
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        private PackageFieldMatchOption option = PackageFieldMatchOption.ContainsCaseInsensitive;
        public PackageFieldMatchOption Option
        {
            get => option;
            set => SetProperty(ref option, value);
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
