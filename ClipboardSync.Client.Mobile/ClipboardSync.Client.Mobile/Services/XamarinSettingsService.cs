using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Models;
using ClipboardSync.Common.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ClipboardSync.Client.Mobile.Services
{
    internal class XamarinSettingsService : ISettingsService
    {
        public IPinnedListFileHelper PinnedListFileHelper { get; set; }

        public XamarinSettingsService(IPinnedListFileHelper pinnedListFileService) 
        {
            PinnedListFileHelper = pinnedListFileService;
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

        public async Task<JwtTokensPairModel?> GetTokenAsync()
        {
            string key = "JwtTokenModels";
            if (!Preferences.ContainsKey($"{key}_AccessToken_Token"))
            {
                return null;
            }
            JwtTokensPairModel value = new()
            {
                AccessToken = new(),
                RefreshToken = new(),
            };
            value.AccessToken.Token = Preferences.Get($"{key}_AccessToken_Token", value.AccessToken.Token);
            value.AccessToken.Expiration = Preferences.Get($"{key}_AccessToken_Expiration", value.AccessToken.Expiration ?? DateTime.Now);
            value.RefreshToken.Token = Preferences.Get($"{key}_RefreshToken_Token", value.RefreshToken.Token);
            value.RefreshToken.Expiration = Preferences.Get($"{key}_RefreshToken_Expiration", value.RefreshToken.Expiration ?? DateTime.Now);
            return value;
        }

        public async Task SetTokenAsync(JwtTokensPairModel value)
        {
            string key = "JwtTokenModels";
            Preferences.Set($"{key}_AccessToken_Token", value.AccessToken.Token);
            Preferences.Set($"{key}_AccessToken_Expiration", value.AccessToken.Expiration ?? DateTime.Now);
            Preferences.Set($"{key}_RefreshToken_Token", value.RefreshToken.Token);
            Preferences.Set($"{key}_RefreshToken_Expiration", value.RefreshToken.Expiration ?? DateTime.Now);
        }
    }
}
