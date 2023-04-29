using ClipboardSync.Common.Exceptions;
using ClipboardSync.Common.Models;
using ClipboardSync.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClipboardSync.Client.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {

        AuthenticationService _authService;
        TaskCompletionSource<bool> _taskCompletionSource;
        public bool ErrorMessageVisible
        {
            get => errorMessageVisible;
            set
            {
                errorMessageVisible = value;
                OnPropertyChanged(nameof(ErrorMessageVisible));
            }
        }
        public string ErrorMessage 
        { 
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        public string UserName
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        public string URL
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged(nameof(URL));
            }
        }
        string _url;
        private bool errorMessageVisible = false;
        private string errorMessage = "";
        private string password = "";
        private string username = "";
        bool _loginResult = false;

        public LoginPage(string url, TaskCompletionSource<bool> taskCompletionSource, AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
            URL = url;
            _taskCompletionSource = taskCompletionSource;
            BindingContext = this;
        }

        private async void Button_Pressed(object sender, EventArgs e)
        {
            _authService.ServerUrl = _url;
            try
            {
                var loginResult = await _authService.LoginAsync(new UserInfo()
                {
                    UserName = username,
                    Password = password,
                });
                _loginResult = true;
                _ = Navigation.PopModalAsync();
            }
            catch (HttpRequestException hrex)
            {
                ErrorMessageVisible = true;
                ErrorMessage = "无法连接到服务器。";
            }
            catch (UserNameOrPasswordWrongException upwe)
            {
                ErrorMessageVisible = true;
                ErrorMessage = "用户名或密码错误。";
            }
        }

        private void Button_Exit_Pressed(object sender, EventArgs e)
        {
            _loginResult = false;
            _ = Navigation.PopModalAsync();
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            _taskCompletionSource.SetResult(_loginResult);
        }
    }
}