using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinGetStore.Helpers;
using WinGetStore.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace WinGetStore.Controls.Dialogs
{
    public sealed partial class VersionsDialog : ContentDialog
    {
        private readonly VersionsViewModel Provider;

        public VersionsDialog(VersionsViewModel provider)
        {
            InitializeComponent();
            Provider = provider;
            DataContext = Provider;
            _ = Provider.Refresh();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Uri url = element.Tag?.ToString().ValidateAndGetUri();
            if (url != null)
            {
                _ = Launcher.LaunchUriAsync(url);
            }
        }
    }
}
