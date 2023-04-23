using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Services;
using Xamarin.Essentials;

namespace ClipboardSync_Client_Mobile.Services
{
    internal class XamarinSettingsService : ISettingsService
    {
        public IPinnedListFileHelper PinnedListFile { get; set; }

        public XamarinSettingsService(IPinnedListFileHelper pinnedListFileService) 
        {
            PinnedListFile = pinnedListFileService;
        }

        public int Get(string key, int defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        public string Get(string key, string defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        public bool IsContainsKey(string key)
        {
            return Preferences.ContainsKey(key);
        }

        public void Set(string key, int value)
        {
            Preferences.Set(key, value);
        }

        public void Set(string key, string value)
        {
            Preferences.Set(key, value);
        }
    }
}
