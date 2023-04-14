using ClipboardSync.BlazorServer.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClipboardSync.BlazorServer.Hubs
{
    public class FilesHub : Hub
    {
        private readonly ILogger _logger = null;
        private readonly string folderName = "ClipboardSync_Server";

        public FilesHub(ILogger<ServerHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // called when a client connects to the hub
            _logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")}  FilesHubClient Online.");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // called when a client disconnects from the hub
            _logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")}  FilesHubClient Offline.");

            await base.OnDisconnectedAsync(exception);
        }


        public void SaveStringList(List<string> list, string fileName)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fullFileName = Path.Combine(directoryPath, fileName);
            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            using (StreamWriter writer = new StreamWriter(fullFileName, false, Encoding.UTF8))
            {
                serializer.Serialize(writer, list);
            }
        }

        public async Task<List<string>> LoadStringList(string fileName)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fullFileName = Path.Combine(directoryPath, fileName);
            if (File.Exists(fullFileName) == false)
            {
                SaveStringList(new List<string>(), fileName);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
            using (StreamReader reader = new StreamReader(fullFileName, Encoding.UTF8))
            {
                List<string>? list = serializer.Deserialize(reader) as List<string>;
                return list ?? new();
            }
        }
    }
}