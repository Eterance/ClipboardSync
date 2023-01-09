using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardSync_Server
{
    public class ServerHub : Hub
    {
        private readonly ILogger<Worker> _logger = null;
        private readonly MessageCache _messageCache = null;

        public ServerHub(ILogger<Worker> logger, MessageCache messageCache)
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

        public async Task SetServerCacheCapacity(int capacity)
        {
            if (capacity != _messageCache.Capacity)
            {
                int old_capacity = _messageCache.Capacity;
                _messageCache.Capacity = capacity;
                _logger.LogInformation($"{DateTimeOffset.Now} Server cache capacity set from {old_capacity} to {capacity}.");
                await Clients.All.SendAsync("GetServerCacheCapacity", capacity);
            }
        }
    }
}