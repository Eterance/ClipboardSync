using ClipboardSync_Client_Mobile.Services;
using ClipboardSync_Client_Mobile.ViewModels;
using ClipboardSync_Client_Mobile.Views;
using ClipboardSync_Client_Mobile.Localization;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;

namespace ClipboardSync_Client_Mobile
{
    public partial class App : Application
    {
        static ClipboardManagementViewModel viewModel;
        
        public static ClipboardManagementViewModel ViewModel
        {
            get
            {
                if (viewModel == null)
                {
                    viewModel = new ClipboardManagementViewModel();
                    viewModel.ToastMessage += (sender, e) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            DependencyService.Get<IToast>().ShortAlert(e);
                        });
                    };
                    viewModel.Initialize();
                }
                return viewModel;
            }
        }
        
        public App()
        {
            InitializeComponent();
            Localization.Resources.Culture = new CultureInfo(Preferences.Get("Localization", "en"));
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
            if (ViewModel.IsConnected == false)
            {
                await ViewModel.ConnectAsync();
            }
        }
        
    }
}
