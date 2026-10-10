using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using WinGetStore.Common;
using WinGetStore.Helpers;

namespace WinGetStore.ViewModels
{
    public abstract partial class DispatcherNotifyPropertyChanged(CoreDispatcher dispatcher) : INotifyPropertyChanged
    {
        public CoreDispatcher Dispatcher => dispatcher;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void PropertyChangedInvoke([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChangedInvoke(name);
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (!property?.Equals(value) ?? (value != null))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        #endregion
    }

    public abstract partial class ViewModelBase(CoreDispatcher dispatcher) : DispatcherNotifyPropertyChanged(dispatcher)
    {
        public abstract Task Refresh(bool reset = false);
    }

    public abstract partial class CachedViewModelBase<TSelf> : ViewModelBase where TSelf : CachedViewModelBase<TSelf>
    {
        protected static readonly ConditionalWeakTable<CoreDispatcher, TSelf> _caches = [];

        #region INotifyPropertyChanged

        protected static new void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, TSelf> cache in _caches)
                {
                    _ = cache.Key.AwaitableRunAsync(() => cache.Value.PropertyChangedInvoke(name));
                }
            }
        }

        protected new void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (!property?.Equals(value) ?? (value != null))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        protected static void RaisePropertyChangedEvent(params string[] names)
        {
            if (names?.Length > 0)
            {
                foreach (KeyValuePair<CoreDispatcher, TSelf> cache in _caches)
                {
                    _ = cache.Key.AwaitableRunAsync(() => names.ForEach(cache.Value.PropertyChangedInvoke));
                }
            }
        }

        #endregion

        public CachedViewModelBase(CoreDispatcher dispatcher) : base(dispatcher) => _caches.AddOrUpdate(dispatcher, this as TSelf);

        public static bool TryGetCache(CoreDispatcher dispatcher, out TSelf cache) => _caches.TryGetValue(dispatcher, out cache);
    }
}
