using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ClipboardSync.Common.Services
{
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
        public void Save<T>(List<T> list)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Deserialize the list from xml file. Using UTF-8.
        /// </summary>
        /// <returns></returns>
        public List<T> Load<T>()
        {
            if (File.Exists(fileName) == false)
            {
                Save(new List<string>());
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
            {
                return (List<T>)serializer.Deserialize(reader);
            }
        }
    }
}
