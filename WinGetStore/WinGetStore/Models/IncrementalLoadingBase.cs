﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using WinGetStore.Common;

namespace WinGetStore.Models
{
    /// <summary>
    /// A incremental loading class base on the data binding sample on
    /// <see cref="MSDN" href="https://code.msdn.microsoft.com/windowsapps/Data-Binding-7b1d67b5/"/>
    /// , but using ObservableCollection to contain data and notify changes. <br/>
    /// If you want to use incremental loading in MVVM pattern, you can use this as a collection,
    /// and add a constructor with a delegate to load data,
    /// so that you can load different data in your view model, refer this blog for detail
    /// <see href="http://blogs.msdn.com/b/devosaure/archive/2012/10/15/isupportincrementalloading-loading-a-subsets-of-data.aspx"/>
    /// </summary>
    public abstract class IncrementalLoadingBase<T>(CoreDispatcher dispatcher) : ObservableCollection<T>, ISupportIncrementalLoading
    {
        #region ISupportIncrementalLoading

        public bool HasMoreItems => HasMoreItemsOverride();

        /// <summary>
        /// Load more items, this is invoked by Controls like ListView.
        /// </summary>
        /// <param name="count">How many new items want to load.</param>
        /// <returns>Item count actually loaded.</returns>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                return AsyncInfo.Run(_ => Task.FromResult(new LoadMoreItemsResult { Count = 0 }));
            }

            _busy = true;

            // We need to use AsyncInfo.Run to invoke async operation, as this method cannot return a Task.
            return AsyncInfo.Run(cancellation => LoadMoreItemsAsync(count, cancellation));
        }

        #endregion

        public CoreDispatcher Dispatcher => dispatcher;

        private bool any = false;
        public bool Any
        {
            get => any;
            set => SetProperty(ref any, value);
        }

        private bool isLoading = false;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        protected override event PropertyChangedEventHandler PropertyChanged;

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

        /// <summary>
        /// We use this method to load data and add to self.
        /// </summary>
        /// <param name="count">How many want to load.</param>
        /// <param name="cancellation">Cancellation Token</param>
        /// <returns>Item count actually loaded.</returns>
        protected async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count, CancellationToken cancellation)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();

                // We are going to load more.
                IsLoading = true;
                LoadMoreStarted?.Invoke();

                // Data loading will different for sub-class.
                uint loaded = await LoadMoreItemsOverrideAsync(count, cancellation).ConfigureAwait(false);

                // We finished loading operation.
                IsLoading = false;
                LoadMoreCompleted?.Invoke();

                await Dispatcher.ResumeForegroundAsync();
                return new LoadMoreItemsResult { Count = loaded };
            }
            finally
            {
                _busy = false;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            Any = Count > 0;
        }

        public delegate void EventHandler();
        public delegate void EventHandler<in TEventArgs>(TEventArgs e);

        public event EventHandler LoadMoreStarted;
        public event EventHandler LoadMoreCompleted;

        #region Overridable methods

        public virtual async Task AddAsync(T item)
        {
            await Dispatcher.ResumeForegroundAsync();
            Add(item);
        }

        public virtual async Task RemoveAsync(T item)
        {
            await Dispatcher.ResumeForegroundAsync();
            Remove(item);
        }

        public virtual async Task ClearAsync()
        {
            await Dispatcher.ResumeForegroundAsync();
            Clear();
        }

        protected abstract Task<uint> LoadMoreItemsOverrideAsync(uint count, CancellationToken cancellationToken);

        protected abstract bool HasMoreItemsOverride();

        #endregion

        protected bool _busy = false;
    }
}
