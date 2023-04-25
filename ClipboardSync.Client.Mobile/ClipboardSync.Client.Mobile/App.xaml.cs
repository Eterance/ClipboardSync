using ClipboardSync.Client.Mobile.Services;
using ClipboardSync.Client.Mobile.Views;
using ClipboardSync.Client.Mobile.Localization;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;
using ClipboardSync.Common.Services;
using ClipboardSync.Common.Helpers;

namespace ClipboardSync.Client.Mobile
{
    public partial class App : Application
    {

        internal static XamarinSettingsService XamarinSettingsService { get; set; } = new(new LocalPinnedListFileHelper("ClipboardSync_Mobile"));


        static ClipboardManageService clipboardManagementViewModel;
        
        public static ClipboardManageService ClipboardManageService
        {
            get
            {
                if (clipboardManagementViewModel == null)
                {
                    clipboardManagementViewModel = new ClipboardManageService(
                        settingsService: XamarinSettingsService,
                        toast: (e) =>
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    DependencyService.Get<IToast>().ShortAlert(e);
                                });
                            }
                        );
                    clipboardManagementViewModel.Initialize();
                }
                return clipboardManagementViewModel;
            }
        }
        
        public App()
        {
            InitializeComponent();
            Localization.Resources.Culture = new CultureInfo(XamarinSettingsService.Get("Localization", "en"));
            ClipboardSync.Common.Localization.Resources.Culture = new CultureInfo(XamarinSettingsService.Get("Localization", "en"));
            MainPage = new AppShell();
        }
        

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override async void OnResume()
        {
            if (ClipboardManageService.IsConnected == false)
            {
                await ClipboardManageService.ConnectAsync();
            }
        }
        
    }
}
