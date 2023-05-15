using AppInstallerCaller;
using Microsoft.Management.Deployment;
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
        private ObservableCollection<PackageMatchFilter> packageMatchFilter = new();
        public ObservableCollection<PackageMatchFilter> PackageMatchFilters
        {
            get => packageMatchFilter;
            set
            {
                if (packageMatchFilter != value)
                {
                    packageMatchFilter = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public string Value { get; set; }
        public PackageMatchField Field { get; set; } = PackageMatchField.Id;
        public PackageFieldMatchOption Option { get; set; } = PackageFieldMatchOption.ContainsCaseInsensitive;

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

        public FiltersViewModel(IList<PackageMatchFilter> packageMatchFilters)
        {
            PackageMatchFilters = new(packageMatchFilters);
        }

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
