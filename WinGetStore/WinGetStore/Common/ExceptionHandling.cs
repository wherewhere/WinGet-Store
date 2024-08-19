using System;
using System.Threading;
using Windows.UI.Xaml.Controls;

namespace WinGetStore.Common
{
    /// <summary>
    /// Wrapper around a standard synchronization context, that catches any unhandled exceptions.
    /// Acts as a façade passing calls to the original SynchronizationContext.
    /// </summary>
    /// <param name="syncContext">The synchronization context to wrap.</param>
    /// <example>
    /// Set this up inside your App.xaml.cs file as follows:
    /// <code>
    /// protected override void OnActivated(IActivatedEventArgs args)
    /// {
    ///     EnsureSyncContext();
    ///     ...
    /// }
    /// 
    /// protected override void OnLaunched(LaunchActivatedEventArgs args)
    /// {
    ///     EnsureSyncContext();
    ///     ...
    /// }
    /// 
    /// private void EnsureSyncContext()
    /// {
    ///     var exceptionHandlingSynchronizationContext = ExceptionHandlingSynchronizationContext.Register();
    ///     exceptionHandlingSynchronizationContext.UnhandledException += OnSynchronizationContextUnhandledException;
    /// }
    /// 
    /// private void OnSynchronizationContextUnhandledException(object sender, UnhandledExceptionEventArgs args)
    /// {
    ///     args.Handled = true;
    /// }
    /// </code>
    /// </example>
    public class ExceptionHandlingSynchronizationContext(SynchronizationContext syncContext) : SynchronizationContext
    {
        /// <summary>
        /// Registration method. Call this from OnLaunched and OnActivated inside the App.xaml.cs.
        /// </summary>
        /// <returns>The <see cref="ExceptionHandlingSynchronizationContext"/> which registered.</returns>
        public static ExceptionHandlingSynchronizationContext Register()
        {
            SynchronizationContext syncContext = Current ?? throw new InvalidOperationException("Ensure a synchronization context exists before calling this method.");

            if (syncContext is not ExceptionHandlingSynchronizationContext customSynchronizationContext)
            {
                customSynchronizationContext = new ExceptionHandlingSynchronizationContext(syncContext);
                SetSynchronizationContext(customSynchronizationContext);
            }

            return customSynchronizationContext;
        }

        /// <summary>
        /// Links the synchronization context to the specified frame
        /// and ensures that it is still in use after each navigation event.
        /// </summary>
        /// <param name="rootFrame">The frame to link the synchronization context to.</param>
        /// <returns>The <see cref="ExceptionHandlingSynchronizationContext"/> which registered.</returns>
        public static ExceptionHandlingSynchronizationContext Register(Frame rootFrame)
        {
            if (rootFrame == null) { throw new ArgumentNullException(nameof(rootFrame)); }

            ExceptionHandlingSynchronizationContext synchronizationContext = Register();

            rootFrame.Navigating += (sender, args) => EnsureContext(synchronizationContext);
            rootFrame.Loaded += (sender, args) => EnsureContext(synchronizationContext);

            return synchronizationContext;
        }

        /// <summary>
        /// Ensures that the specified synchronization context is the current one.
        /// </summary>
        /// <param name="context">The <see cref="SynchronizationContext"/> to ensure.</param>
        private static void EnsureContext(SynchronizationContext context)
        {
            if (Current != context) { SetSynchronizationContext(context); }
        }

        /// <inheritdoc/>
        public override SynchronizationContext CreateCopy() => new ExceptionHandlingSynchronizationContext(syncContext.CreateCopy());

        /// <inheritdoc/>
        public override void OperationCompleted() => syncContext.OperationCompleted();

        /// <inheritdoc/>
        public override void OperationStarted() => syncContext.OperationStarted();

        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object state) => syncContext.Post(WrapCallback(d), state);

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object state) => syncContext.Send(d, state);

        /// <summary>
        /// Pack the callback in a try-catch block to catch any unhandled exceptions.
        /// </summary>
        /// <param name="sendOrPostCallback">The callback to wrap.</param>
        /// <returns>The wrapped callback.</returns>
        private SendOrPostCallback WrapCallback(SendOrPostCallback sendOrPostCallback) =>
            state =>
            {
                try
                {
                    sendOrPostCallback(state);
                }
                catch (Exception ex)
                {
                    if (!HandleException(ex)) { throw; }
                }
            };

        /// <summary>
        /// Handles the exception by raising the UnhandledException event.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <returns><see langword="true"/> if the exception was handled; otherwise, <see langword="false"/>.</returns>
        private bool HandleException(Exception exception)
        {
            if (UnhandledException == null) { return false; }

            UnhandledExceptionEventArgs exWrapper = new(exception);

            UnhandledException(this, exWrapper);

#if DEBUG && !DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
            if (System.Diagnostics.Debugger.IsAttached) { System.Diagnostics.Debugger.Break(); }
#endif

            return exWrapper.Handled;
        }


        /// <summary>
        /// Listen to this event to catch any unhandled exceptions and allow for handling them
        /// so they don't crash your application.
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;
    }

    /// <summary>
    /// Provides data for the UnhandledException event.
    /// </summary>
    /// <param name="exception">The exception that was not handled.</param>
    public class UnhandledExceptionEventArgs(Exception exception) : EventArgs
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the exception is handled.
        /// </summary>
        /// <value><see langword="true"/> to mark the exception as handled, which indicates that the event system should not process it further; otherwise, <see langword="false"/>.</value>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the <b>HRESULT</b> code associated with the unhandled exception.
        /// </summary>
        /// <value>The <b>HRESULT</b> code (for Visual C++ component extensions (C++/CX)), or a mapped common language runtime (CLR) <see cref="System.Exception"/>.</value>
        public Exception Exception => exception;

        /// <summary>
        /// Gets the message string as passed by the originating unhandled exception.
        /// </summary>
        /// <value>The message string, which may be useful for debugging.</value>
        public string Message => Exception?.Message;
    }
}
