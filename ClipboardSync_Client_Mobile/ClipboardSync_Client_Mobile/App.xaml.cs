using ClipboardSync_Client_Mobile.Services;
using ClipboardSync_Client_Mobile.Views;
using ClipboardSync_Client_Mobile.Localization;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;
using ClipboardSync.Commom.ViewModels;

namespace ClipboardSync_Client_Mobile
{
    public partial class App : Application
    {
        static ClipboardManagementViewModel clipboardManagementViewModel;
        
        public static ClipboardManagementViewModel ClipboardManagementViewModel
        {
            get
            {
                if (clipboardManagementViewModel == null)
                {
                    clipboardManagementViewModel = new ClipboardManagementViewModel(new XamarinSettingsService());
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
            Localization.Resources.Culture = new CultureInfo(Preferences.Get("Localization", "en"));
            ClipboardSync.Common.Localization.Resources.Culture = new CultureInfo(Preferences.Get("Localization", "en"));
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
