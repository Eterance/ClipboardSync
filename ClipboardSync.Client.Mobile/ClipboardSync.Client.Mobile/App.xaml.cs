using ClipboardSync.Client.Mobile.Services;
using ClipboardSync.Client.Mobile.Views;
using ClipboardSync.Client.Mobile.Localization;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;
using ClipboardSync.Common.ViewModels;
using ClipboardSync.Common.Helpers;
using System.Threading.Tasks;
using ClipboardSync.Common.Services;

namespace ClipboardSync.Client.Mobile
{
    public partial class App : Application
    {

        internal static XamarinSettingsService XamarinSettingsService { get; set; } = new(new LocalPinnedListFileHelper("ClipboardSync_Mobile"));
        internal static AuthenticationService AuthenticationService { get; set; } = new(XamarinSettingsService);


        static ClipboardViewModel clipboardManagementViewModel;
        
        public static ClipboardViewModel ClipboardVM
        {
            get
            {
                if (clipboardManagementViewModel == null)
                {
                    clipboardManagementViewModel = new ClipboardViewModel(
                        settingsService: XamarinSettingsService,
                        signalrService: new(),
                        authService: AuthenticationService
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
            if (ClipboardVM.IsConnected == false)
            {
                await ClipboardVM.TryConnectAllUrlAsync();
            }
        }
        
    }
}
