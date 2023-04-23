
using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Models;
using System.Linq.Expressions;

namespace ClipboardSync.Common.Services
{
    public interface ISettingsService
    {
        public int Get(string key, int defaultValue);
        public string Get(string key, string defaultValue);
        /// <summary>
        /// Get Jwt Token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns>True: Token Exist</returns>
        public bool Get(string key, out JwtTokenModel defaultValue);
        public void Set(string key, int value);
        public void Set(string key, string value);
        public void Set(string key, JwtTokenModel value);

        public bool IsContainsKey(string key);
        public IPinnedListFileHelper PinnedListFile { get; set; }
        public IJwtTokenReadWriteHelper TokenRwHelper { get; set; }
    }
}
