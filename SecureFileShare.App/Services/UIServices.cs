using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SecureFileShare.App.Services
{
    /// <summary>
    ///     By Shakti Prakash Singh
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class UIServices
    {
        /// <summary>
        ///     A value indicating whether the UI is currently busy
        /// </summary>
        private static bool _isBusy;

        /// <summary>
        ///     Sets the busystate as busy.
        /// </summary>
        public static void SetBusyState()
        {
            SetBusyState(true);
        }

        /// <summary>
        ///     Sets the busystate to busy or not busy.
        /// </summary>
        /// <param name="busy">if set to <c>true</c> the application is now busy.</param>
        private static void SetBusyState(bool busy)
        {
            if (busy != _isBusy)
            {
                _isBusy = busy;
                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if (_isBusy)
                {
                    // ReSharper disable once ObjectCreationAsStatement
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle,
                        dispatcherTimer_Tick, Application.Current.Dispatcher);
                }
            }
        }

        /// <summary>
        ///     Handles the Tick event of the dispatcherTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var dispatcherTimer = sender as DispatcherTimer;
            if (dispatcherTimer != null)
            {
                SetBusyState(false);
                dispatcherTimer.Stop();
            }
        }
    }
}