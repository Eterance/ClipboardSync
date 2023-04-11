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
        private SignalRRemoteFilesService _signalRService;

        public RemotePinnedListFileService(SignalRRemoteFilesService signalrFileServices)
        {
            _signalRService = signalrFileServices;
        }

        public void Save(List<string> list)
        {
            if (!_signalRService.IsConnected)
            {
                throw new Exception();
            }
            _ = _signalRService.SaveStringList(list, _xmlName);
        }

        public List<string> Load()
        {
            if (!_signalRService.IsConnected)
            {
                throw new Exception();
            }
            return _signalRService.LoadStringList(_xmlName);
        }
    }
}
