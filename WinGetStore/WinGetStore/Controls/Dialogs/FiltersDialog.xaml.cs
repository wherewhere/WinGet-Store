using Microsoft.Management.Deployment;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinGetStore.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace WinGetStore.Controls.Dialogs
{
    public sealed partial class FiltersDialog : ContentDialog
    {
        private readonly FiltersViewModel Provider;

        public ObservableCollection<PackageMatchFilter> PackageMatchFilters => Provider.PackageMatchFilters;

        public FiltersDialog(FiltersViewModel provider)
        {
            InitializeComponent();
            Provider = provider;
            DataContext = Provider;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement BackgroundElement = this.FindDescendant("BackgroundElement");
            if (BackgroundElement != null)
            {
                BackgroundElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "Add":
                    if (Pivot.SelectedIndex == 0)
                    {
                        Pivot.SelectedIndex = 1;
                    }
                    else if (!string.IsNullOrWhiteSpace(Provider?.Value))
                    {
                        Pivot.SelectedIndex = 0;
                        Provider?.AddField();
                    }
                    break;
                case "Delete":
                    Provider?.PackageMatchFilters.Remove((PackageMatchFilter)element.Tag);
                    break;
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (Pivot.SelectedIndex == 1 && !string.IsNullOrWhiteSpace(Provider?.Value))
            {
                args.Cancel = true;
                Pivot.SelectedIndex = 0;
                Provider?.AddField();
            }
            else
            {
                args.Cancel = Provider?.PackageMatchFilters.Any() != true;
            }
        }
    }
}
