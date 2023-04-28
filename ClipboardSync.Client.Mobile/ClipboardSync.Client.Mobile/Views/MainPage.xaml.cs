using ClipboardSync.Client.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ClipboardSync.Client.Mobile.Localization;

namespace ClipboardSync.Client.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        double editorHeight = 0;
        public MainPage()
        {
            InitializeComponent();
            MainPageViewModel viewModel = new(App.ClipboardVM, App.ClipboardVM.SettingsService);
            BindingContext = viewModel;
            App.ClipboardVM.LoginMethod = async (url, completionToken) =>
            {
                // https://learn.microsoft.com/zh-cn/xamarin/xamarin-forms/app-fundamentals/navigation/modal
                LoginPage loginPage = new(url, completionToken, App.AuthenticationService);
                await Navigation.PushModalAsync(loginPage);
            };
            if (App.ClipboardVM?.IsConnected == false)
            {
                _ = App.ClipboardVM.TryConnectAllUrlAsync();
            }
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void SendEditor_SizeChanged(object sender, EventArgs e)
        {
            Editor editor = sender as Editor;
            if (editorHeight != editor.Height)
            { 
                editorHeight = editor.Height;
                double stackLayoutHeight = editorHeight + MainStackLayout.Height; // 计算StackLayout的高度

                // 将计算出的StackLayout高度设置为页面布局的高度
                MainStackLayout.HeightRequest = stackLayoutHeight;
            }
        }
    }
}