
namespace ClipboardSync.Common.Services
{
    public interface ISettingsService
    {
        public int Get(string key, int defaultValue);
        public string Get(string key, string defaultValue);
        public void Set(string key, int value);
        public void Set(string key, string value);

        public bool IsContainsKey(string key);
        public IPinnedListFileService PinnedListFile { get; set; }
    }
}
