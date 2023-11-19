﻿using Microsoft.Toolkit.Uwp;
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
using WinGetStore.Common;
using WinGetStore.Helpers;
using WinGetStore.Models;
using WinGetStore.WinRT;

namespace WinGetStore.ViewModels.SettingsPages
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("SettingsPage");

        public static Dictionary<DispatcherQueue, SettingsViewModel> Caches { get; } = [];

        public static string DeviceFamily => AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public static string ToolkitVersion => Assembly.GetAssembly(typeof(HsvColor)).GetName().Version.ToString(3);

        public static string VersionTextBlockText { get; } = $"{ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? Package.Current.DisplayName} v{Package.Current.Id.Version.ToFormattedString(3)}";

        public DispatcherQueue Dispatcher { get; }

        public string Title { get; } = _loader.GetString("Title");

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

        private static string _wingetVersion = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Loading");
        public string WinGetVersion
        {
            get => _wingetVersion;
            set => SetProperty(ref _wingetVersion, value);
        }

        private static bool _isWinGetInstalled = false;
        public bool IsWinGetInstalled
        {
            get => _isWinGetInstalled;
            set => SetProperty(ref _isWinGetInstalled, value);
        }

        private static bool _isWinGetDevInstalled = false;
        public bool IsWinGetDevInstalled
        {
            get => _isWinGetDevInstalled;
            set => SetProperty(ref _isWinGetDevInstalled, value);
        }

        private static bool _checkingUpdate;
        public bool CheckingUpdate
        {
            get => _checkingUpdate;
            set => SetProperty(ref _checkingUpdate, value);
        }

        private static string _gotoUpdateTag;
        public string GotoUpdateTag
        {
            get => _gotoUpdateTag;
            set => SetProperty(ref _gotoUpdateTag, value);
        }

        private static Visibility _gotoUpdateVisibility;
        public Visibility GotoUpdateVisibility
        {
            get => _gotoUpdateVisibility;
            set => SetProperty(ref _gotoUpdateVisibility, value);
        }

        private static bool _updateStateIsOpen;
        public bool UpdateStateIsOpen
        {
            get => _updateStateIsOpen;
            set => SetProperty(ref _updateStateIsOpen, value);
        }

        private static string _updateStateMessage;
        public string UpdateStateMessage
        {
            get => _updateStateMessage;
            set => SetProperty(ref _updateStateMessage, value);
        }

        private static InfoBarSeverity _updateStateSeverity;
        public InfoBarSeverity UpdateStateSeverity
        {
            get => _updateStateSeverity;
            set => SetProperty(ref _updateStateSeverity, value);
        }

        private static string _updateStateTitle;
        public string UpdateStateTitle
        {
            get => _updateStateTitle;
            set => SetProperty(ref _updateStateTitle, value);
        }

        private static string _aboutTextBlockText;
        public string AboutTextBlockText
        {
            get => _aboutTextBlockText;
            set => SetProperty(ref _aboutTextBlockText, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<DispatcherQueue, SettingsViewModel> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name));
                }
            }
        }

        protected static async void RaisePropertyChangedEvent(params string[] names)
        {
            if (names?.Any() == true)
            {
                foreach (KeyValuePair<DispatcherQueue, SettingsViewModel> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    names.ForEach(name => cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name)));
                }
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

        public SettingsViewModel(DispatcherQueue dispatcher)
        {
            Dispatcher = dispatcher ?? DispatcherQueue.GetForCurrentThread();
            Caches[dispatcher] = this;
        }

        public async Task UpdateWinGetVersionAsync()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            IEnumerable<Package> packages = await PackageHelper.FindPackagesByName("Microsoft.DesktopAppInstaller");
            if (packages.Any())
            {
                string wingetVersion = packages.FirstOrDefault().Id.Version.ToFormattedString();
                bool isWinGetInstalled = WinGetProjectionFactory.IsWinGetInstalled;
                bool isWinGetDevInstalled = WinGetProjectionFactory.IsWinGetDevInstalled;
                WinGetVersion = wingetVersion;
                IsWinGetInstalled = isWinGetInstalled;
                IsWinGetDevInstalled = isWinGetDevInstalled;
            }
            else
            {
                WinGetVersion = "Not Installed";
            }
        }

        public async void CheckUpdate()
        {
            CheckingUpdate = true;
            try
            {
                UpdateInfo info = null;
                try
                {
                    info = await UpdateHelper.CheckUpdateAsync("wherewhere", "WinGet-Store").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    UpdateStateIsOpen = true;
                    UpdateStateMessage = ex.Message;
                    UpdateStateSeverity = InfoBarSeverity.Error;
                    GotoUpdateVisibility = Visibility.Collapsed;
                    UpdateStateTitle = _loader.GetString("CheckFailed");
                }
                if (info?.IsExistNewVersion == true)
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateTag = info.ReleaseUrl;
                    GotoUpdateVisibility = Visibility.Visible;
                    UpdateStateSeverity = InfoBarSeverity.Warning;
                    UpdateStateTitle = _loader.GetString("FindUpdate");
                    UpdateStateMessage = $"{VersionTextBlockText} -> {info.Version.ToString(3)}";
                }
                else
                {
                    UpdateStateIsOpen = true;
                    UpdateStateMessage = string.Empty;
                    GotoUpdateVisibility = Visibility.Collapsed;
                    UpdateStateSeverity = InfoBarSeverity.Success;
                    UpdateStateTitle = _loader.GetString("UpToDate");
                }
                UpdateDate = DateTime.Now;
            }
            finally
            {
                CheckingUpdate = false;
            }
        }

        private async Task GetAboutTextBlockTextAsync(bool reset)
        {
            if (reset || string.IsNullOrWhiteSpace(_aboutTextBlockText))
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

        public async Task Refresh(bool reset)
        {
            if (reset)
            {
                RaisePropertyChangedEvent(
                    nameof(UpdateDate),
                    nameof(SelectedTheme));
            }
            await GetAboutTextBlockTextAsync(reset);
            await UpdateWinGetVersionAsync();
        }
    }
}
