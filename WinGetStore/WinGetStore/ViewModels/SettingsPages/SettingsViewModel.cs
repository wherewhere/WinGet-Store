using AppInstallerCaller;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.SettingsPages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        private string wingetVersion = "Loading...";
        public string WinGetVersion
        {
            get => wingetVersion;
            set => SetProperty(ref wingetVersion, value);
        }

        private bool isWinGetInstalled = false;
        public bool IsWinGetInstalled
        {
            get => isWinGetInstalled;
            set => SetProperty(ref isWinGetInstalled, value);
        }

        private bool isWinGetDevInstalled = false;
        public bool IsWinGetDevInstalled
        {
            get => isWinGetDevInstalled;
            set => SetProperty(ref isWinGetDevInstalled, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property is null ? value is null : property.Equals(value)) { return; }
            property = value;
            _ = Dispatcher.EnqueueAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        public async Task Refresh()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            IEnumerable<Package> packages = await PackageHelper.FindPackagesByName("Microsoft.DesktopAppInstaller");
            if (packages.Any())
            {
                string wingetVersion = packages.FirstOrDefault().Id.Version.ToFormattedString();
                bool isWinGetInstalled = WinGetProjectionFactory.IsWinGetInstalled;
                bool isWinGetDevInstalled = WinGetProjectionFactory.IsWinGetDevInstalled;
                await Dispatcher.ResumeForegroundAsync();
                WinGetVersion = wingetVersion;
                IsWinGetInstalled = isWinGetInstalled;
                IsWinGetDevInstalled = isWinGetDevInstalled;
            }
            else
            {
                await Dispatcher.ResumeForegroundAsync();
                WinGetVersion = "Not Installed";
            }
        }
    }
}
