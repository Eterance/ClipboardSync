using ClipboardSync.Common.Exceptions;
using ClipboardSync.Common.Models;
using ClipboardSync.Common.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClipboardSync.Client.Windows.Views
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        UserInfo _userInfo { get; set; } = new UserInfo()
        {
            Password = string.Empty,
            UserName = string.Empty,
        };
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

        public LoginWindow(string url, TaskCompletionSource<bool> taskCompletionSource, AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
            URL = url;
            _taskCompletionSource = taskCompletionSource;
            DataContext = this;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
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
                Close();
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

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            _loginResult = false;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _taskCompletionSource.SetResult(_loginResult);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
