using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ClipboardSync.Common.Services
{
    /// <summary>
    /// Save/Load PinnedList at Server side via SignalR.
    /// </summary>
    public class RemotePinnedListFileService : IPinnedListFileService
    {
        readonly static string _xmlName = "pinnedList.xml";
        private string fileName;
        private SignalRCoreService _signalRService;

        public RemotePinnedListFileService(SignalRCoreService signalrServices)
        {
            _signalRService = signalrServices;
        }

        public void Save(List<string> list)
        {
            _ = _signalRService.SaveStringList(list, _xmlName);
        }

        public List<string> Load()
        {
            return _signalRService.LoadStringList(_xmlName);
        }
    }
}
