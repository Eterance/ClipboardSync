using Blazored.LocalStorage;
using Blazored.SessionStorage;
using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Models;
using ClipboardSync.Common.Services;

namespace ClipboardSync.BlazorServer.Services
{
    internal class BlazorServerClientSettingsService: ISettingsService
    {

        private Dictionary<string, string> stringSettings;
        private Dictionary<string, int> intSettings;
        private ILocalStorageService localStorage;

        public IPinnedListFileHelper PinnedListFileHelper { get; set; }

        public BlazorServerClientSettingsService(IPinnedListFileHelper pinnedListFileService, ILocalStorageService storage)
        {
            PinnedListFileHelper = pinnedListFileService;
            localStorage = storage;
            intSettings = new ();
            stringSettings = new ();
        }

        public int Get(string key, int defaultValue)
        {
            bool success = intSettings.TryGetValue(key, out int value);
            if (success)
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public string Get(string key, string defaultValue)
        {
            bool success = stringSettings.TryGetValue(key, out string? value);
            if (success)
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public bool IsContainsKey(string key)
        {
            return stringSettings.ContainsKey(key) || intSettings.ContainsKey(key);
        }

        public void Set(string key, int value)
        {
            intSettings[key] = value;
            //Serialize(intSettings, intSettingsFileName);
        }

        public void Set(string key, string value)
        {
            stringSettings[key] = value;
            //Serialize(stringSettings, stringSettingsFileName);
        }

        private void Serialize<T>(Dictionary<string, T> dict, string dictFileName)
        {
            /*// Create a DataContractSerializer instance for the dictionary type
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<int, string>));

            // Open a file stream to save the serialized data
            using (FileStream stream = new FileStream(dictFileName, FileMode.Create))
            {
                // Create an XmlWriter to write the serialized data to the file stream
                XmlWriter writer = XmlWriter.Create(stream);

                // Serialize the dictionary to the XmlWriter
                serializer.WriteObject(writer, dict);
            }*/
            using (StreamWriter sw = new StreamWriter(dictFileName))
            {
                foreach (KeyValuePair<string, T> kvp in dict)
                {
                    sw.WriteLine("{0}={1}", kvp.Key, kvp.Value);
                }
            }
        }

        private Dictionary<string, int> DeserializeInt(string dictFileName)
        {
            if (File.Exists(dictFileName) == false)
            {
                return new Dictionary<string, int>();
            }
            Dictionary<string, int> dict = new();
            using (StreamReader sr = new StreamReader(dictFileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] keyValue = line.Split('=');
                    dict[keyValue[0]] = int.Parse(keyValue[1]);
                }
            }
            return dict;
        }

        private Dictionary<string, string> DeserializeString(string dictFileName)
        {
            if (File.Exists(dictFileName) == false)
            {
                return new Dictionary<string, string>();
            }
            Dictionary<string, string> dict = new();
            using (StreamReader sr = new StreamReader(dictFileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] keyValue = line.Split('=');
                    dict[keyValue[0]] = keyValue[1];
                }
            }
            return dict;
        }

        /// <summary>
        /// Get Token 
        /// NOTE: Due to pre-rendering in Blazor Server you can't perform any JS interop until the OnAfterRender lifecycle method.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<JwtTokensPairModel?> GetJwtTokensPairAsync(string key)
        {
            // https://github.com/Blazored/SessionStorage
            return await localStorage.GetItemAsync<JwtTokensPairModel>(key);
        }

        public async Task SetJwtTokensPairAsync(string key, JwtTokensPairModel value)
        {
            await localStorage.SetItemAsync(key, value);
        }

        public async Task DeleteJwtTokensPairAsync(string key)
        {
            await localStorage.RemoveItemAsync(key);
        }
    }
}
