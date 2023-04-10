using ClipboardSync_Client_Mobile.Services;
using ClipboardSync_Client_Mobile.Views;
using ClipboardSync_Client_Mobile.Localization;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;
using ClipboardSync.Common.ViewModels;

namespace ClipboardSync_Client_Mobile
{
    public partial class App : Application
    {

        internal static XamarinSettingsService XamarinSettingsService { get; set; } = new();


        static ClipboardManagementViewModel clipboardManagementViewModel;
        
        public static ClipboardManagementViewModel ClipboardManagementViewModel
        {
            get
            {
                if (clipboardManagementViewModel == null)
                {
                    clipboardManagementViewModel = new ClipboardManagementViewModel(XamarinSettingsService);
                    clipboardManagementViewModel.ToastMessage += (sender, e) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            DependencyService.Get<IToast>().ShortAlert(e);
                        });
                    };
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
            if (ClipboardManagementViewModel.IsConnected == false)
            {
                await ClipboardManagementViewModel.ConnectAsync();
            }
        }
        
    }
}
