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
    /// The interface of helper type for switch thread.
    /// </summary>
    public interface IThreadSwitcher : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        void GetResult();

        /// <summary>
        /// Gets an awaiter used to await this <see cref="IThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        IThreadSwitcher GetAwaiter();
    }

    /// <summary>
    /// The interface of helper type for switch thread.
    /// </summary>
    /// <typeparam name="T">The type of the result of <see cref="GetAwaiter"/>.</typeparam>
    public interface IThreadSwitcher<out T> : IThreadSwitcher
    {
        /// <summary>
        /// Gets an awaiter used to await <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A <typeparamref name="T"/> awaiter instance.</returns>
        new T GetAwaiter();
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="CoreDispatcher"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="Dispatcher">A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.</param>
    /// <param name="Priority">Specifies the priority for event dispatch.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct CoreDispatcherThreadSwitcher(CoreDispatcher Dispatcher, CoreDispatcherPriority Priority = CoreDispatcherPriority.Normal) : IThreadSwitcher<CoreDispatcherThreadSwitcher>
    {
        /// <inheritdoc/>
        public bool IsCompleted => Dispatcher?.HasThreadAccess != false;

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public CoreDispatcherThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = Dispatcher.RunAsync(Priority, () => continuation());
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="DispatcherQueue"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="Dispatcher">A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.</param>
    /// <param name="Priority">Specifies the priority for event dispatch.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct DispatcherQueueThreadSwitcher(DispatcherQueue Dispatcher, DispatcherQueuePriority Priority = DispatcherQueuePriority.Normal) : IThreadSwitcher<DispatcherQueueThreadSwitcher>
    {
        /// <inheritdoc/>
        public bool IsCompleted => Dispatcher is not DispatcherQueue dispatcher
            || (ThreadSwitcher.IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess);

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public DispatcherQueueThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = Dispatcher.TryEnqueue(Priority, () => continuation());
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="SynchronizationContext"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="Context">A <see cref="SynchronizationContext"/> whose foreground thread to switch execution to.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct SynchronizationContextThreadSwitcher(SynchronizationContext Context) : IThreadSwitcher<SynchronizationContextThreadSwitcher>
    {
        /// <inheritdoc/>
        public bool IsCompleted => Context is not SynchronizationContext context
            || SynchronizationContext.Current == context;

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public SynchronizationContextThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => Context.Post(_ => continuation(), null);
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="ThreadPool"/>. This type is not intended to be used directly from your code.
    /// </summary>
    /// <param name="Priority">Specifies the priority for event dispatch.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly record struct ThreadPoolThreadSwitcher(WorkItemPriority Priority = WorkItemPriority.Normal) : IThreadSwitcher<ThreadPoolThreadSwitcher>
    {
        /// <inheritdoc/>
        public bool IsCompleted => SynchronizationContext.Current == null;

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public ThreadPoolThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

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
        public static CoreDispatcherThreadSwitcher ResumeForegroundAsync(this CoreDispatcher dispatcher, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) => new(dispatcher, priority);

        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="context">A <see cref="SynchronizationContext"/> whose foreground thread to switch execution to.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static SynchronizationContextThreadSwitcher ResumeForegroundAsync(this SynchronizationContext context) => new(context);

        /// <summary>
        /// A helper function—for use within a coroutine—that returns control to the caller, and then immediately resumes execution on a thread pool thread.
        /// </summary>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static ThreadPoolThreadSwitcher ResumeBackgroundAsync(WorkItemPriority priority = WorkItemPriority.Normal) => new(priority);
    }
}