using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public sealed partial class FiltersViewModel(IList<PackageMatchFilter> selectors, IList<PackageMatchFilter> filters, CoreDispatcher dispatcher) : DispatcherNotifyPropertyChanged(dispatcher)
    {
        public static FilterType[] FilterTypes { get; } = Enum.GetValues<FilterType>();
        public static List<PackageMatchField> PackageMatchFields { get; } = [.. Enum.GetValues<PackageMatchField>()];
        public static List<PackageFieldMatchOption> PackageFieldMatchOptions { get; } = [.. Enum.GetValues<PackageFieldMatchOption>()];
        
        public ObservableCollection<PackageMatchFilter> Selectors { get; set => SetProperty(ref field, value); } = [.. selectors];
        public ObservableCollection<PackageMatchFilter> Filters { get; set => SetProperty(ref field, value); } = [.. filters];
        public FilterType FilterType { get; set => SetProperty(ref field, value); } = FilterType.Both;
        public string Value { get; set => SetProperty(ref field, value); }
        public PackageMatchField Field { get; set => SetProperty(ref field, value); } = PackageMatchField.Id;
        public PackageFieldMatchOption Option { get; set => SetProperty(ref field, value); } = PackageFieldMatchOption.ContainsCaseInsensitive;

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
