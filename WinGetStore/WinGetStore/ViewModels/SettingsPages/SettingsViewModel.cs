using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml;
using WinGetStore.Helpers;
using WinGetStore.Models;
using WinGetStore.WinRT;

namespace WinGetStore.ViewModels.SettingsPages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public static SettingsViewModel Caches;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("SettingsPage");

        public static string DeviceFamily => AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public static string ToolkitVersion => Assembly.GetAssembly(typeof(HsvColor)).GetName().Version.ToString(3);

        public DispatcherQueue Dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        public string Title => _loader.GetString("Title");

        public DateTime UpdateDate
        {
            get => SettingsHelper.Get<DateTime>(SettingsHelper.UpdateDate);
            set
            {
                if (UpdateDate != value)
                {
                    SettingsHelper.Set(SettingsHelper.UpdateDate, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int SelectedTheme
        {
            get => 2 - (int)ThemeHelper.ActualTheme;
            set
            {
                if (SelectedTheme != value)
                {
                    ThemeHelper.RootTheme = (ElementTheme)(2 - value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _wingetVersion = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Loading");
        public string WinGetVersion
        {
            get => _wingetVersion;
            set => SetProperty(ref _wingetVersion, value);
        }

        private bool _isWinGetInstalled = false;
        public bool IsWinGetInstalled
        {
            get => _isWinGetInstalled;
            set => SetProperty(ref _isWinGetInstalled, value);
        }

        private bool _isWinGetDevInstalled = false;
        public bool IsWinGetDevInstalled
        {
            get => _isWinGetDevInstalled;
            set => SetProperty(ref _isWinGetDevInstalled, value);
        }

        private bool _checkingUpdate;
        public bool CheckingUpdate
        {
            get => _checkingUpdate;
            set => SetProperty(ref _checkingUpdate, value);
        }

        private string _gotoUpdateTag;
        public string GotoUpdateTag
        {
            get => _gotoUpdateTag;
            set => SetProperty(ref _gotoUpdateTag, value);
        }

        private Visibility _gotoUpdateVisibility;
        public Visibility GotoUpdateVisibility
        {
            get => _gotoUpdateVisibility;
            set => SetProperty(ref _gotoUpdateVisibility, value);
        }

        private bool _updateStateIsOpen;
        public bool UpdateStateIsOpen
        {
            get => _updateStateIsOpen;
            set => SetProperty(ref _updateStateIsOpen, value);
        }

        private string _updateStateMessage;
        public string UpdateStateMessage
        {
            get => _updateStateMessage;
            set => SetProperty(ref _updateStateMessage, value);
        }

        private InfoBarSeverity _updateStateSeverity;
        public InfoBarSeverity UpdateStateSeverity
        {
            get => _updateStateSeverity;
            set => SetProperty(ref _updateStateSeverity, value);
        }

        private string _updateStateTitle;
        public string UpdateStateTitle
        {
            get => _updateStateTitle;
            set => SetProperty(ref _updateStateTitle, value);
        }

        private string _aboutTextBlockText;
        public string AboutTextBlockText
        {
            get => _aboutTextBlockText;
            set => SetProperty(ref _aboutTextBlockText, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (name == null || property is null ? value is null : property.Equals(value)) { return; }
            property = value;
            _ = Dispatcher.EnqueueAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public string VersionTextBlockText
        {
            get
            {
                string ver = Package.Current.Id.Version.ToFormattedString(3);
                string name = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? Package.Current.DisplayName;
                GetAboutTextBlockText();
                return $"{name} v{ver}";
            }
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

        public async void CheckUpdate()
        {
            CheckingUpdate = true;
            UpdateInfo info = null;
            try
            {
                info = await UpdateHelper.CheckUpdateAsync("wherewhere", "WinGet-Store");
            }
            catch (Exception ex)
            {
                UpdateStateIsOpen = true;
                UpdateStateMessage = ex.Message;
                UpdateStateSeverity = InfoBarSeverity.Error;
                GotoUpdateVisibility = Visibility.Collapsed;
                UpdateStateTitle = _loader.GetString("CheckFailed");
            }
            if (info != null)
            {
                if (info.IsExistNewVersion)
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateTag = info.ReleaseUrl;
                    GotoUpdateVisibility = Visibility.Visible;
                    UpdateStateSeverity = InfoBarSeverity.Warning;
                    UpdateStateTitle = _loader.GetString("FindUpdate");
                    UpdateStateMessage = $"{VersionTextBlockText} -> {info.TagName}";
                }
                else
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateVisibility = Visibility.Collapsed;
                    UpdateStateSeverity = InfoBarSeverity.Success;
                    UpdateStateTitle = _loader.GetString("UpToDate");
                }
            }
            UpdateDate = DateTime.Now;
            CheckingUpdate = false;
        }

        private async void GetAboutTextBlockText()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            string langCode = LanguageHelper.GetPrimaryLanguage();
            Uri dataUri = new($"ms-appx:///Assets/About/About.{langCode}.md");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            if (file != null)
            {
                string markdown = await FileIO.ReadTextAsync(file);
                AboutTextBlockText = markdown;
            }
        }
    }
}
