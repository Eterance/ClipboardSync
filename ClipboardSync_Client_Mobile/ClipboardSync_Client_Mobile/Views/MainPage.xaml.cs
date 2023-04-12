using ClipboardSync_Client_Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ClipboardSync_Client_Mobile.Localization;

namespace ClipboardSync_Client_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        double editorHeight = 0;
        public MainPage()
        {
            InitializeComponent();
            MainPageViewModel viewModel = new(App.ClipboardManagementViewModel, App.ClipboardManagementViewModel.SettingsService);
            BindingContext = viewModel;
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