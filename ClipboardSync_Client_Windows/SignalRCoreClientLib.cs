using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ClipboardSync_Client_Windows
{
    public class SignalRCoreClientLib
    {
        public EventHandler<string> MessageReceived;
        public EventHandler<string> ErrorOcurr;
        public EventHandler<List<string>> MessagesSync;

        private HubConnection _connection;

        public async Task Connect(string serverIp, int port)
        {
            if (_connection == null)
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl($"http://{serverIp}:{port}/ServerHub")
                    .WithAutomaticReconnect()
                    .Build();

                _connection.Closed += async (error) =>
                {
                };

                _connection.On<string>("ReceiveMessage", (message) =>
                {
                    MessageReceived?.Invoke(this, message);
                });

                _connection.On<int>("GetServerCacheCapacity", (capacity) =>
                {
                });

                _connection.On<List<string>>("SyncMessages", (messages) =>
                {
                    MessagesSync?.Invoke(this, messages);
                });
            }
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                ErrorOcurr?.Invoke(this, $"{ex.Message}; base Exception: {ex.GetBaseException().Message}");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await Connect(serverIp, port);
            }
        }

        public async Task SendMessage(string message)
        {
            try
            {
                await _connection.InvokeAsync("BroadcastMessage", message);
            }
            catch (Exception ex)
            {
                ErrorOcurr?.Invoke(this, ex.Message);
            }
        }
    }
}
