using ClipboardSync.Commom.Models;
using ClipboardSync.Commom.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Packaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipboardSync_Client_Windows.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
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

        public MainWindowViewModel(ClipboardManagementViewModel viewModel)
        {
            SubViewModel = viewModel;

            if (LanguageList == null)
            {
                LanguageList = new ObservableCollection<LocalizationModel>
                {
                    new LocalizationModel(Localization.Resources.Lang_en, "en"),
                    new LocalizationModel(Localization.Resources.Lang_zh_cn, "zh-CN")
                };
            }
        }
        
        private async void SendClipboardTextAsync(string text)
        {
            SubViewModel.SendText(text);
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
