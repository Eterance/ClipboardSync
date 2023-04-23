using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClipboardSync.Common.Helpers
{
    public interface IPinnedListFileHelper
    {

        /// <summary>
        /// Serialize the list to xml file. Using UTF-8.
        /// </summary>
        /// <param name="list"></param>
        public void Save(List<string> list);

        /// <summary>
        /// Deserialize the list from xml file. Using UTF-8.
        /// </summary>
        /// <returns></returns>
        public Task<List<string>> Load();
    }
}
