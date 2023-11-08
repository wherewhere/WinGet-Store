using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace WinGetStore.Common
{
    /// <summary>
    /// A helper type for switch thread by <see cref="CoreDispatcher"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="Dispatcher">A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.</param>
    /// <param name="Priority">Specifies the priority for event dispatch.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct DispatcherThreadSwitcher(CoreDispatcher Dispatcher, CoreDispatcherPriority Priority = CoreDispatcherPriority.Normal) : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted => Dispatcher.HasThreadAccess;

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="DispatcherThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public DispatcherThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = Dispatcher.RunAsync(Priority, () => continuation());
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="DispatcherQueue"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="Dispatcher">A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.</param>
    /// <param name="Priority">Specifies the priority for event dispatch.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct DispatcherQueueThreadSwitcher(DispatcherQueue Dispatcher, DispatcherQueuePriority Priority = DispatcherQueuePriority.Normal) : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted => ThreadSwitcher.IsHasThreadAccessPropertyAvailable && Dispatcher.HasThreadAccess;

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="DispatcherThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public DispatcherQueueThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = Dispatcher.TryEnqueue(Priority, () => continuation());
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="ThreadPool"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="Priority">Specifies the priority for event dispatch.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct ThreadPoolThreadSwitcher(WorkItemPriority Priority = WorkItemPriority.Normal) : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted => SynchronizationContext.Current == null;

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="DispatcherThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public ThreadPoolThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = ThreadPool.RunAsync(_ => continuation(), Priority);
    }

    /// <summary>
    /// The extensions for switching threads.
    /// </summary>
    public static class ThreadSwitcher
    {
        /// <summary>
        /// Gets is <see cref="DispatcherQueue.HasThreadAccess"/> supported.
        /// </summary>
        public static bool IsHasThreadAccessPropertyAvailable { get; } = ApiInformation.IsMethodPresent("Windows.System.DispatcherQueue", "HasThreadAccess");

        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="dispatcher">A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static DispatcherQueueThreadSwitcher ResumeForegroundAsync(this DispatcherQueue dispatcher, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) => new(dispatcher, priority);

        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="dispatcher">A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static DispatcherThreadSwitcher ResumeForegroundAsync(this CoreDispatcher dispatcher, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) => new(dispatcher, priority);

        /// <summary>
        /// A helper function—for use within a coroutine—that returns control to the caller, and then immediately resumes execution on a thread pool thread.
        /// </summary>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static ThreadPoolThreadSwitcher ResumeBackgroundAsync(WorkItemPriority priority = WorkItemPriority.Normal) => new(priority);
    }
}