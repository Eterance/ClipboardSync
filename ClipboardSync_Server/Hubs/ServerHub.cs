using ClipboardSync_Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClipboardSync_Server.Hubs
{
    public class ServerHub : Hub
    {
        private readonly ILogger _logger = null;
        private readonly MessageCacheService _messageCache = null;
        private readonly string folderName = "ClipboardSync_server";

        public ServerHub(ILogger<ServerHub> logger, MessageCacheService messageCache)
        {
            _logger = logger;
            //_logger.LogInformation($"{DateTimeOffset.Now} MyHub.Constructor()");
            _messageCache = messageCache;
        }

        public override async Task OnConnectedAsync()
        {
            // called when a client connects to the hub
            _logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")}  id: {Context.ConnectionId} Online.");
            await Clients.Caller.SendAsync("SyncMessages", _messageCache.GetMessages());
            await Clients.Caller.SendAsync("GetServerCacheCapacity", _messageCache.Capacity);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // called when a client disconnects from the hub
            _logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")}  id{Context.ConnectionId} Offline.");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastMessage(string message)
        {
            bool temp = _messageCache.Push(message);
            if (temp)
            {
                _logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")}  message: {message}");
                await Clients.Others.SendAsync("ReceiveMessage", message);
            }
        }

        // Server Management Methods

        public async Task GetServerCacheCapacity()
        {
            await Clients.Caller.SendAsync("GetServerCacheCapacity", _messageCache.Capacity);
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

        public List<string> LoadStringList(string fileName)
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