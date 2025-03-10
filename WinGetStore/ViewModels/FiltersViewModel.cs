using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using WinGetStore.Common;

namespace WinGetStore.ViewModels
{
    public enum FilterType
    {
        Selector = 0b01,
        Filter = 0b10,
        Both = Selector | Filter
    }

    public partial class FiltersViewModel(IList<PackageMatchFilter> selectors, IList<PackageMatchFilter> filters, CoreDispatcher dispatcher) : INotifyPropertyChanged
    {
        public static FilterType[] FilterTypes { get; } = Enum.GetValues<FilterType>();
        public static List<PackageMatchField> PackageMatchFields { get; } = [.. Enum.GetValues<PackageMatchField>()];
        public static List<PackageFieldMatchOption> PackageFieldMatchOptions { get; } = [.. Enum.GetValues<PackageFieldMatchOption>()];

        public CoreDispatcher Dispatcher => dispatcher;

        private ObservableCollection<PackageMatchFilter> selectors = [.. selectors];
        public ObservableCollection<PackageMatchFilter> Selectors
        {
            get => selectors;
            set => SetProperty(ref selectors, value);
        }

        private ObservableCollection<PackageMatchFilter> filters = [.. filters];
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
            get => this.field;
            set => SetProperty(ref this.field, value);
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
