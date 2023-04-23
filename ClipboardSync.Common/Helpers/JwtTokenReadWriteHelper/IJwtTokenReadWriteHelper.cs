using ClipboardSync.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClipboardSync.Common.Helpers
{
    public interface IJwtTokenReadWriteHelper
    {

        /// <summary>
        /// Serialize the list to xml file. Using UTF-8.
        /// </summary>
        /// <param name="list"></param>
        public Task Save(string key, JwtTokenModel tokenValue);

        /// <summary>
        /// Deserialize the list from xml file. Using UTF-8.
        /// </summary>
        /// <returns></returns>
        public Task<JwtTokenModel> Load(string key);
    }
}
