using ClipboardSync.Common.Models;
using ClipboardSync.Common.ViewModels;
using ClipboardSync.Common.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Packaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using ClipboardSync.Client.Windows.Localization;

namespace ClipboardSync.Client.Windows.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
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
                        new ToastContentBuilder()
                            .AddText(Resources.LanguageDescription)
                            .Show();
                    }
                }
            }
        }

        public ObservableCollection<LocalizationModel> LanguageList { get; set; }

        public string PlaygroundText
        {
            get { return _playgroundText; }
            set
            {
                _playgroundText = value;
                OnPropertyChanged();
            }
        }

        public void Toast(string message)
        {
            // https://learn.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop-msix
            new ToastContentBuilder()
                .AddText(message)
                .Show();
        }

        private string _playgroundText = "";
        private LocalizationModel? selectedLanguage;
        private readonly string localizationSettingName = "Localization";
        private ISettingsService settings;
        private string _connectionStatusInstruction;

        public MainWindowViewModel(ClipboardViewModel viewModel, ISettingsService _settings)
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
        
        public async Task SendClipboardTextAsync(string text)
        {
            bool result = await SubViewModel.SendTextAsync(text);
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
