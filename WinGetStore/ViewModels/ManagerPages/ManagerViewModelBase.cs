using Microsoft.Management.Deployment;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using WinGetStore.Common;

namespace WinGetStore.ViewModels.ManagerPages
{
    public abstract partial class ManagerViewModelBase(CoreDispatcher dispatcher) : ViewModelBase(dispatcher)
    {
        protected static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("MainPage");

        protected bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        public string WaitProgressText { get; set => SetProperty(ref field, value); } = _loader.GetString("Loading");

        protected bool isError = false;
        public bool IsError
        {
            get => isError;
            set => SetProperty(ref isError, value);
        }

        public string ErrorDescription { get; set => SetProperty(ref field, value); }
        public string ErrorLongDescription { get; set => SetProperty(ref field, value); }
        public string ErrorCode { get; set => SetProperty(ref field, value); }

        protected ObservableCollection<CatalogPackage> matchResults = [];
        public ObservableCollection<CatalogPackage> MatchResults
        {
            get => matchResults;
            set => SetProperty(ref matchResults, value);
        }

        protected async void SetError(string title, string description, string code = "")
        {
            if (isError) { return; }
            await Dispatcher.ResumeForegroundAsync();
            IsError = true;
            IsLoading = false;
            ErrorDescription = title;
            ErrorLongDescription = description;
            ErrorCode = code;
            matchResults.Clear();
        }

        protected void RemoveError()
        {
            if (!isError) { return; }
            IsError = false;
            ErrorDescription = string.Empty;
            ErrorLongDescription = string.Empty;
            ErrorCode = string.Empty;
        }
    }
}
