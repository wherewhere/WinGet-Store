using AppInstallerCaller;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels.SettingsPages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string wingetVersion = "Loading...";
        public string WinGetVersion
        {
            get => wingetVersion;
            set
            {
                if (wingetVersion != value)
                {
                    wingetVersion = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isWinGetInstalled = false;
        public bool IsWinGetInstalled
        {
            get => isWinGetInstalled;
            set
            {
                if (isWinGetInstalled != value)
                {
                    isWinGetInstalled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isWinGetDevInstalled = false;
        public bool IsWinGetDevInstalled
        {
            get => isWinGetDevInstalled;
            set
            {
                if (isWinGetDevInstalled != value)
                {
                    isWinGetDevInstalled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private readonly DispatcherQueue Dispatcher = DispatcherQueue.GetForCurrentThread();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
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
