using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Data.Common;
using ClipboardSync.Common.Localization;

namespace ClipboardSync.Commom.Services
{
    public class SignalRCoreService
    {
        public EventHandler<string> MessageReceived { get; set; }
        public EventHandler<string> ConnectStatusUpdate { get; set; }
        public EventHandler<List<string>> MessagesSync { get; set; }
        public EventHandler<int> ServerCacheCapacityUpdated { get; set; }
        public EventHandler<Exception> UnexpectedError { get; set; }
        /// <summary>
        /// CHS: 丢失与服务器的连接。
        /// ENG: Lost connection to server.
        /// </summary>
        public EventHandler<Exception> LostConnection { get; set; }
        /// <summary>
        /// CHS: 成功连接 SignalR 服务器。事件参数（string）：成功连接上的服务器地址。
        /// ENG: Successfully connected to SignalR server. Event parameter (string): The server address that successfully connected to.
        /// </summary>
        public EventHandler<string> Connected { get; set; }
        /// <summary>
        /// CHS: 尝试连接 SignalR 服务器，但是失败。事件参数（List<string>）: 尝试连接的服务器地址列表。
        /// ENG: Try to connect to SignalR server, but failed. Event parameter (List<string>): The server address list that tried to connect to.
        /// </summary>
        public EventHandler<List<string>> ConnectFailed { get; set; }

        private HubConnection _connection;

        public async Task ConnectAsync(List<string> ipEndPoints, CancellationToken token = default)
        {
            // Let the old one die
            Unsubscribe(_connection);
            _ = _connection?.StopAsync();
            _ = _connection?.DisposeAsync();
            
            List<string> tried = new();
            string failedPrefix = "";
            foreach (string ipEndPoint in ipEndPoints)
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl($"http://{ipEndPoint}/ServerHub")
                    .WithAutomaticReconnect()
                    .Build();
                Subscribe(_connection);
                ConnectStatusUpdate?.Invoke(this, $"{failedPrefix}{Resources.Try2Connect2} {ipEndPoint}{Resources.Period}");
                try
                {                    
                    await _connection.StartAsync(token);
                    if (token.IsCancellationRequested == true)
                    {
                        ConnectStatusUpdate?.Invoke(this, Resources.ConnectAborted);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    failedPrefix = $"{Resources.Failed2Connect2} {ipEndPoint}{Resources.Comma}";
                    tried.Add(ipEndPoint);
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    continue;
                }
                Connected?.Invoke(this, ipEndPoint);
                ConnectStatusUpdate?.Invoke(this, $"{Resources.Connected2} {ipEndPoint}{Resources.Period}");
                return;
            }
            // 如果连接是被 CancellationToken 取消的，不触发连接失败事件。
            // 因为可能后面的连接成功后前面的连接尝试才被中止，这时触发连接失败是很奇怪的。
            if (!token.IsCancellationRequested)
            {
                ConnectStatusUpdate?.Invoke(this, Resources.AllServersAreUnavailable);
                ConnectFailed?.Invoke(this, tried);
            }
        }

        private void Subscribe(HubConnection hubConnection)
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

        private void Unsubscribe(HubConnection hubConnection)
        {
            hubConnection?.Remove("ReceiveMessage");
            hubConnection?.Remove("GetServerCacheCapacity");
            hubConnection?.Remove("SyncMessages");
            if (hubConnection != null)
            {
                hubConnection.Closed -= ConnectionClosed;
            }
        }

        private async Task ConnectionClosed(Exception ex)
        {
            LostConnection?.Invoke(this, ex);
        }

        public async Task SendMessage(string message)
        {
            try
            {
                await _connection.InvokeAsync("BroadcastMessage", message);
            }
            catch (Exception ex)
            {
                UnexpectedError?.Invoke(this, ex);
            }
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
    }
}
