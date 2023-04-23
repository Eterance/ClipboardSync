
using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Models;
using System.Threading.Tasks;

namespace ClipboardSync.Common.Services
{
    public interface ISettingsService
    {
        public int Get(string key, int defaultValue);
        public string Get(string key, string defaultValue);
        public void Set(string key, int value);
        public void Set(string key, string value);
        /// <summary>
        /// Get Jwt Token.
        /// NOTE: If using cookie in implement, due to pre-rendering in Blazor Server,
        /// you can't perform any JS interop until the OnAfterRender lifecycle method.
        /// </summary>
        /// <param name="key"></param>
        public Task<JwtTokensPairModel?> GetTokenAsync();
        public Task SetTokenAsync(JwtTokensPairModel value);

        public bool IsContainsKey(string key);
        public IPinnedListFileHelper PinnedListFileHelper { get; set; }
    }
}
