﻿using ClipboardSync.Common.Services;
using Xamarin.Essentials;

namespace ClipboardSync_Client_Mobile.Services
{
    internal class XamarinSettingsService : ISettingsService
    {
        public PinnedListFileService PinnedListFile { get; set; }

        public XamarinSettingsService(PinnedListFileService pinnedListFileService=null) 
        {
            if (pinnedListFileService == null)
            {
                PinnedListFile = new("ClipboardSync_Mobile");
            }
            else 
            {
                PinnedListFile = pinnedListFileService;
            }
        }

        int ISettingsService.Get(string key, int defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        string ISettingsService.Get(string key, string defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        bool ISettingsService.IsContainsKey(string key)
        {
            return Preferences.ContainsKey(key);
        }

        void ISettingsService.Set(string key, int value)
        {
            Preferences.Set(key, value);
        }

        void ISettingsService.Set(string key, string value)
        {
            Preferences.Set(key, value);
        }
    }
}
