using ClipboardSync.Commom.Models;
using ClipboardSync_Client_Mobile.Models;
using ClipboardSync_Client_Mobile.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClipboardSync_Client_Mobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ClipboardManagementViewModel SubViewModel { get; set; }
        public LocalizationModel SelectedLanguage
        {
            get 
            {
                if (selectedLanguage == null)
                {
                    return SearchLanguage(Preferences.Get(localizationSettingName, "en"));
                }
                return selectedLanguage; 
            }
            set
            {
                if (selectedLanguage != value)
                {
                    selectedLanguage = value;
                    Preferences.Set(localizationSettingName, selectedLanguage.LanguageID);
                    OnPropertyChanged();
                }
            }
        }
        public ICommand SendPlaygroundTextCommand { get; set; }
        public ICommand SendClipboardTextAsyncCommand { get; set; }

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

        
        private string _playgroundText;
        private LocalizationModel selectedLanguage;
        private readonly string localizationSettingName = "Localization";

        public MainPageViewModel(ClipboardManagementViewModel viewModel)
        {
            SubViewModel = viewModel;

            SendPlaygroundTextCommand = new Command(SendPlaygroundText);
            SendClipboardTextAsyncCommand = new Command(SendClipboardTextAsync);

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
            SubViewModel.SendText(text);
        }

        private void SendPlaygroundText()
        {
            SubViewModel.SendText(PlaygroundText);
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
