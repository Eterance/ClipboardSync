using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ClipboardSync.Common.Services
{
    public interface IPinnedListFileService
    {

        /// <summary>
        /// Serialize the list to xml file. Using UTF-8.
        /// </summary>
        /// <param name="list"></param>
        public void Save<T>(List<T> list);

        /// <summary>
        /// Deserialize the list from xml file. Using UTF-8.
        /// </summary>
        /// <returns></returns>
        public List<T> Load<T>();
    }
}
