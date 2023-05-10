using System.Collections.Generic;
using Windows.UI.Xaml;

namespace WinGetStore.Helpers
{
    // Helpers class to allow the app to find the Window that contains an
    // arbitrary UIElement (GetWindowForElement).  To do this, we keep track
    // of all active Windows.  The app code must call WindowHelper.CreateWindow
    // rather than "new Window" so we can keep track of all the relevant
    // windows.  In the future, we would like to support this in platform APIs.
    public static class WindowHelper
    {
        public static void TrackWindow(this Window window)
        {
            window.Closed += (sender, args) =>
            {
                ActiveWindows?.Remove(window);
            };
            ActiveWindows?.Add(window);
        }

        public static List<Window> ActiveWindows { get; } = new List<Window>();
    }
}
