using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardSync.Common.Services
{
    public class ClipboardSignalRService
    {
        public EventHandler<string>? MessageReceived { get; set; }
        public EventHandler<List<string>>? MessagesSync { get; set; }
        public EventHandler<int>? ServerCacheCapacityUpdated { get; set; }
        public EventHandler<Exception>? UnexpectedError { get; set; }
        /// <summary>
        /// CHS: 丢失与服务器的连接。
        /// ENG: Lost connection to server.
        /// </summary>
        public EventHandler<Exception>? LostConnection { get; set; }

        protected HubConnection? _connection;
        /// <summary>
        /// CHS: 指示是否已连接到服务器。
        /// ENG: Indicate whether connected to the hub.
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <param name="token"></param>
        /// <returns>Whether connect was successful or not.</returns>
        public async Task<bool> ConnectAsync(string url, string accessToken, CancellationToken token = default)
        {
            // Let the old one die
            Unsubscribe(_connection);
            _ = _connection?.StopAsync();
            _ = _connection?.DisposeAsync();

            _connection = new HubConnectionBuilder()
                    .WithUrl(url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(accessToken);
                    })
                    .WithAutomaticReconnect()
                    .Build();
            Subscribe(_connection);
            try
            {
                await _connection.StartAsync(token);
                if (token.IsCancellationRequested == true)
                {
                    IsConnected = false;
                }
                else 
                {
                    IsConnected = true;
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
            }
            return IsConnected;
        }

        protected void Subscribe(HubConnection hubConnection)
        {
            hubConnection.Closed += ConnectionClosed;
            hubConnection.On<string>("ReceiveMessage", (message) =>
            {
                MessageReceived?.Invoke(this, message);
            });

            hubConnection.On<int>("GetServerCacheCapacity", (capacity) =>
            {
                ServerCacheCapacityUpdated?.Invoke(this, capacity);
            });

            hubConnection.On<List<string>>("SyncMessages", (messages) =>
            {
                MessagesSync?.Invoke(this, messages);
            });
        }

        protected void Unsubscribe(HubConnection hubConnection)
        {
            hubConnection?.Remove("ReceiveMessage");
            hubConnection?.Remove("GetServerCacheCapacity");
            hubConnection?.Remove("SyncMessages");
            if (hubConnection != null)
            {
                hubConnection.Closed -= ConnectionClosed;
            }
        }

        protected async Task ConnectionClosed(Exception ex)
        {
            IsConnected = false;
            LostConnection?.Invoke(this, ex);
        }

        public async Task SendMessageAsync(string message)
        {
            await _connection.InvokeAsync("BroadcastMessage", message);
        }

        public async Task SetServerCacheCapacity(int capacity)
        {
            try
            {
                await _connection.InvokeAsync("SetServerCacheCapacity", capacity);
            }
            catch (Exception ex)
            {
                UnexpectedError?.Invoke(this, ex);
            }
        }

        public async Task SetServerCacheCapacity<T>(int capacity)
        {
            try
            {
                await _connection.InvokeAsync("SetServerCacheCapacity", capacity);
            }
            catch (Exception ex)
            {
                UnexpectedError?.Invoke(this, ex);
            }
        }
    }
}
