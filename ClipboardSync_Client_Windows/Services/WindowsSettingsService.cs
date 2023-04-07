using ClipboardSync.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources.Extensions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ClipboardSync_Client_Windows.Services
{
    internal class WindowsSettingsService: ISettingsService
    {
        private Dictionary<string, string> stringSettings;
        private Dictionary<string, int> intSettings;
        private string intSettingsFileName = "intSettings.xml";
        private string stringSettingsFileName = "stringSettings.xml";

        public WindowsSettingsService()
        {
            intSettings = Deserialize<int>(intSettingsFileName);
            stringSettings = Deserialize<string>(stringSettingsFileName);
        }

        int ISettingsService.Get(string key, int defaultValue)
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

        string ISettingsService.Get(string key, string defaultValue)
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

        bool ISettingsService.IsContainsKey(string key)
        {
            return stringSettings.ContainsKey(key) || intSettings.ContainsKey(key);
        }

        void ISettingsService.Set(string key, int value)
        {
            intSettings[key] = value;
            Serialize(intSettings, intSettingsFileName);
        }

        void ISettingsService.Set(string key, string value)
        {
            stringSettings[key] = value;
            Serialize(stringSettings, stringSettingsFileName);
        }

        private void Serialize<T>(Dictionary<string, T> dict, string dictFileName)
        {
            // Create a DataContractSerializer instance for the dictionary type
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<int, string>));

            // Open a file stream to save the serialized data
            using (FileStream stream = new FileStream(dictFileName, FileMode.Create))
            {
                // Create an XmlWriter to write the serialized data to the file stream
                XmlWriter writer = XmlWriter.Create(stream);

                // Serialize the dictionary to the XmlWriter
                serializer.WriteObject(writer, dict);
            }
        }

        private Dictionary<string, T> Deserialize<T>(string dictFileName)
        {
            if (File.Exists(dictFileName) == false)
            {
                return new Dictionary<string, T>();
            }
            // Create a DataContractSerializer instance for the dictionary type
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<int, string>));
            Dictionary<string, T> dict;
            // Open the file stream containing the serialized data
            using (FileStream stream = new FileStream("dictFileName", FileMode.Open))
            {
                // Create an XmlReader to read the serialized data from the file stream
                XmlReader reader = XmlReader.Create(stream);

                // Deserialize the dictionary from the XmlReader
                dict = (Dictionary<string, T>)serializer.ReadObject(reader);
            }
            return dict;
        }
    }
}
