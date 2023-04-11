using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ClipboardSync.Common.Services
{
    /// <summary>
    /// Save/Load PinnedList at local machine.
    /// </summary>
    public class LocalPinnedListFileService: IPinnedListFileService
    {
        readonly static string _xmlName = "pinnedList.xml";
        private string fileName;

        public LocalPinnedListFileService(string folderName)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            fileName = Path.Combine(directoryPath, _xmlName);
        }

        /// <summary>
        /// Serialize the list to xml file. Using UTF-8.
        /// </summary>
        /// <param name="list"></param>
        public void Save(List<string> list)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Deserialize the list from xml file. Using UTF-8.
        /// </summary>
        /// <returns></returns>
        public List<string> Load()
        {
            if (File.Exists(fileName) == false)
            {
                Save(new List<string>());
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
            {
                return (List<string>)serializer.Deserialize(reader);
            }
        }
    }
}
