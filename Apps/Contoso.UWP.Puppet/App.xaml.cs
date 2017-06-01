﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Push;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;

namespace Contoso.UWP.Puppet
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            MobileCenter.LogLevel = LogLevel.Verbose;
            MobileCenter.SetLogUrl("https://in-integration.dev.avalanch.es");
            Push.PushNotificationReceived += PushNotificationReceivedHandler;
            MobileCenter.Start("42f4a839-c54c-44da-8072-a2f2a61751b2", typeof(Analytics), typeof(Crashes), typeof(Push));
            Push.CheckLaunchedFromNotification(e);

            Frame rootFrame = Window.Current.Content as Frame;
            List<Task> tasks = new List<Task>();
            for (var i = 0; i < 100; ++i)
            {
                var iter = i;
                tasks.Add(Task.Run(() =>
                {
                    Analytics.TrackEvent($"task number {iter}");

                    if (iter % 2 == 0)
                    {
                        Analytics.Enabled = true;
                    }
                    if (iter == 2)
                    {
                        Analytics.Enabled = false;
                    }
                }));
            }

            Task.WhenAll(tasks).ContinueWith(t => Analytics.Enabled = true);
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void PushNotificationReceivedHandler(object sender, PushNotificationReceivedEventArgs args)
        {
            string title = args.Title;
            string message = args.Message;
            var customData = args.CustomData;

            string customDataString = string.Empty;
            foreach (var pair in customData)
            {
                customDataString += $"key='{pair.Key}', value='{pair.Value}'";
            }

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(message))
            {
                MobileCenterLog.Debug(MobileCenterLog.LogTag, $"PushNotificationReceivedHandler received title:'{title}', message:'{message}', customData:{customDataString}");
            }
            else
            {
                MobileCenterLog.Debug(MobileCenterLog.LogTag, $"PushNotificationReceivedHandler received customData:{customDataString}");
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
