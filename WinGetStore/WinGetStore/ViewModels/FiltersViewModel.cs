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
    public enum FilterType
    {
        Selector = 0b01,
        Filter = 0b10,
        Both = Selector | Filter
    }

    public class FiltersViewModel(IList<PackageMatchFilter> selectors, IList<PackageMatchFilter> filters) : INotifyPropertyChanged
    {
        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        private ObservableCollection<PackageMatchFilter> selectors = new(selectors);
        public ObservableCollection<PackageMatchFilter> Selectors
        {
            get => selectors;
            set => SetProperty(ref selectors, value);
        }

        private ObservableCollection<PackageMatchFilter> filters = new(filters);
        public ObservableCollection<PackageMatchFilter> Filters
        {
            get => filters;
            set => SetProperty(ref filters, value);
        }

        private FilterType filterType = FilterType.Both;
        public FilterType FilterType
        {
            get => filterType;
            set => SetProperty(ref filterType, value);
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

        public void AddField()
        {
            PackageMatchFilter filter = WinGetProjectionFactory.TryCreatePackageMatchFilter();
            filter.Field = Field;
            filter.Option = Option;
            filter.Value = Value;
            if (FilterType.HasFlag(FilterType.Selector))
            {
                Selectors.Add(filter);
            }
            if (FilterType.HasFlag(FilterType.Filter))
            {
                Filters.Add(filter);
            }
        }
    }
}
