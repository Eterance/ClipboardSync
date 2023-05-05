using ClipboardSync.Common.Models;
using ClipboardSync.Common.ViewModels;
using ClipboardSync.Common.Services;
using ClipboardSync.Client.Mobile.Models;
using ClipboardSync.Client.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using ClipboardSync.Client.Mobile.Localization;

namespace ClipboardSync.Client.Mobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public string ConnectionStatusInstruction
        {
            get => _connectionStatusInstruction;
            private set
            {
                SetValue(ref _connectionStatusInstruction, value);
            }
        }
        public ClipboardViewModel SubViewModel { get; set; }
        public LocalizationModel SelectedLanguage
        {
            get 
            {
                if (selectedLanguage == null)
                {
                    return SearchLanguage(settings.Get(localizationSettingName, "en"));
                }
                return selectedLanguage; 
            }
            set
            {
                var before = selectedLanguage;
                if (selectedLanguage != value)
                {
                    SetValue(ref selectedLanguage, value);
                    settings.Set(localizationSettingName, selectedLanguage.LanguageID);
                    if (before != null)
                    { 
                        DependencyService.Get<IToast>().LongAlert(Resources.LanguageDescription);
                    }
                }
            }
        }
        public ICommand SendPlaygroundTextCommand { get; set; }
        public ICommand ClearPlaygroundTextCommand { get; set; }
        public ICommand SendClipboardTextAsyncCommand { get; set; }

        public ObservableCollection<LocalizationModel> LanguageList { get; set; }

        public string PlaygroundText
        {
            get { return _playgroundText; }
            set
            {
                SetValue(ref _playgroundText, value);
            }
        }

        
        private string _playgroundText = "";
        private LocalizationModel? selectedLanguage;
        private readonly string localizationSettingName = "Localization";
        private ISettingsService settings;
        private string _connectionStatusInstruction;

        private void Toast(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToast>().ShortAlert(message);
            });
        }

        public MainPageViewModel(ClipboardViewModel viewModel, ISettingsService _settings)
        {
            SubViewModel = viewModel;

            SubViewModel.HistoryListCapacityUpdated += (sender, newCapacity) =>
            {
                if (newCapacity <= 0)
                {
                    Toast($"{Resources.ClipboardHistoryCapacityChanged2}{Resources.Unlimited}{Resources.Period}");
                }
                else
                {
                    Toast($"{Resources.ClipboardHistoryCapacityChanged2}{newCapacity}{Resources.Period}");
                }
            };
            SubViewModel.BeginConnect += (_, url) =>
            {
                ConnectionStatusInstruction = $"{Resources.Try2Connect2} {url}{Resources.Period}";
            };
            SubViewModel.FailToConnectAll += (_, _) =>
            {
                ConnectionStatusInstruction = $"{Resources.AllServersAreUnavailable}";
            };
            SubViewModel.ConnectSuccess += (_, url) =>
            {
                ConnectionStatusInstruction = $"{Resources.Connected2} {url}{Resources.Period}";
            };
            SubViewModel.InvalidUrl += (_, url) =>
            {
                Toast($"\"{url}\"{Resources.NotAValidUrl}");
            };


            SendPlaygroundTextCommand = new Command(SendPlaygroundText);
            SendClipboardTextAsyncCommand = new Command(SendClipboardTextAsync);
            ClearPlaygroundTextCommand = new Command(() => PlaygroundText="");
            settings = _settings;

            if (LanguageList == null)
            {
                LanguageList = new ObservableCollection<LocalizationModel>
                {
                    new LocalizationModel(Localization.Resources.Lang_en, "en"),
                    new LocalizationModel(Localization.Resources.Lang_zh_cn, "zh-CN")
                };
            }
        }
        
        private async void SendClipboardTextAsync()
        {
            string text = await Clipboard.GetTextAsync();
            var result = await SubViewModel.SendTextAsync(text);
            if (result == true)
            {
                Toast(Resources.Sent);
            }
            else
            {
                Toast(Resources.SendFail);
            }
        }

        private async void SendPlaygroundText()
        {
            bool result = await SubViewModel.SendTextAsync(PlaygroundText);
            if (result == true)
            {
                Toast(Resources.Sent);
            }
            else
            {
                Toast(Resources.SendFail);
            }
        }

        private LocalizationModel SearchLanguage(string id)
        {
            foreach (var item in LanguageList)
            {
                if (item.LanguageID == id)
                {
                    return item;
                }
            }
            return LanguageList[0];
        }
    }
}
