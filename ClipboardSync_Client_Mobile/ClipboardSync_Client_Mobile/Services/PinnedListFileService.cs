using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ClipboardSync_Client_Mobile.Services
{
    public static class PinnedListFileService
    {
        readonly static string _xmlName = "pinnedList.xml";

        /// <summary>
        /// Serialize the list to xml file. Using UTF-8.
        /// </summary>
        /// <param name="list"></param>
        public static void Save<T>(List<T> list)
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _xmlName);
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
        public static List<T> Load<T>()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _xmlName);
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
