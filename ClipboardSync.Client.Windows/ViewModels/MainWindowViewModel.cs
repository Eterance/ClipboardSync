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

        
        private string _playgroundText;
        private LocalizationModel selectedLanguage;
        private readonly string localizationSettingName = "Localization";
        private ISettingsService settings;

        public MainWindowViewModel(ClipboardViewModel viewModel, ISettingsService _settings)
        {
            SubViewModel = viewModel;
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
        
        public void SendClipboardText(string text)
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
