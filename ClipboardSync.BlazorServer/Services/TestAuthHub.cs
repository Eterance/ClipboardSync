using ClipboardSync.BlazorServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClipboardSync.BlazorServer.Services
{
    [Authorize]
    public class TestAuthHub : Hub
    {
        private readonly ILogger _logger = null;

        public TestAuthHub(ILogger<TestAuthHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // called when a client connects to the hub
            _logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")}  id: {Context.ConnectionId} Online.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // called when a client disconnects from the hub
            _logger.LogInformation($"{DateTime.Now.ToString("hh:mm:ss.fff")}  id{Context.ConnectionId} Offline.");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<string> Test()
        {
            return "you access TestAuthHub!";
        }

    }
}