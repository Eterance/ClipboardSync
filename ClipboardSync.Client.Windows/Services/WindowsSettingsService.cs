﻿using ClipboardSync.Common.Collections;
using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Models;
using ClipboardSync.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClipboardSync.Client.Windows.Services
{
    internal class WindowsSettingsService: ISettingsService
    {
        private Dictionary<string, string> stringSettings;
        private Dictionary<string, int> intSettings;
        private SerializableDictionary<string, JwtTokensPairModel> tokenPairsDict;
        private string intSettingsFileName = "intSettings.ini";
        private string stringSettingsFileName = "stringSettings.ini";
        private string tokenPairsFileName = "tokenPairs.xml";
        private string rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private string _directoryPath = "ClipboardSync_Desktop";

        public IPinnedListFileHelper PinnedListFileHelper { get; set; }

        public WindowsSettingsService(
            IPinnedListFileHelper pinnedListFileService, 
            string directoryPath)
        {
            PinnedListFileHelper = pinnedListFileService;
            intSettings = DeserializeInt(Path.Combine(rootFolder, _directoryPath, intSettingsFileName));
            stringSettings = DeserializeString(Path.Combine(rootFolder, _directoryPath, stringSettingsFileName));
            tokenPairsDict = XmlDeserialize<SerializableDictionary<string, JwtTokensPairModel>>(Path.Combine(rootFolder, _directoryPath, tokenPairsFileName))??new();
            _directoryPath = directoryPath;
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
            Serialize(intSettings, Path.Combine(rootFolder, _directoryPath, intSettingsFileName));
        }

        public void Set(string key, string value)
        {
            stringSettings[key] = value;
            Serialize(stringSettings, Path.Combine(rootFolder, _directoryPath, stringSettingsFileName));
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

        private T? XmlDeserialize<T>(string dictFileName)
        {
            if (File.Exists(dictFileName) == false)
            {
                return default(T);
            }
            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            using var fs = new FileStream(dictFileName, FileMode.Open);
            T result = (T)mySerializer.Deserialize(fs);
            if (result == null)
            {
                result = default;
            }
            return result;
        }

        private void XmlSerialize<T>(T value, string dictFileName)
        {
            // Insert code to set properties and fields of the object.  
            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            // To write to a file, create a StreamWriter object.  
            StreamWriter myWriter = new StreamWriter(dictFileName);
            mySerializer.Serialize(myWriter, value);
            myWriter.Close();
        }

        public async Task<JwtTokensPairModel?> GetJwtTokensPairAsync(string key)
        {

            bool success = tokenPairsDict.TryGetValue(key, out JwtTokensPairModel? value);
            if (success)
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public async Task SetJwtTokensPairAsync(string key, JwtTokensPairModel value)
        {
            tokenPairsDict[key] = value;
            XmlSerialize(tokenPairsDict, Path.Combine(_directoryPath, tokenPairsFileName));
        }

        public async Task DeleteJwtTokensPairAsync(string key)
        {
            tokenPairsDict.Remove(key);
            XmlSerialize(tokenPairsDict, Path.Combine(_directoryPath, tokenPairsFileName));
        }
    }
}
