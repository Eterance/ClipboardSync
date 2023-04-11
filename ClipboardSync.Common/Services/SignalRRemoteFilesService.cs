using ClipboardSync.Common.Localization;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardSync.Common.Services
{
    public class SignalRRemoteFilesService
    {

        protected HubConnection _connection;

        public bool IsConnected { get; private set; } = false;

        public virtual async Task ConnectAsync(Uri uri, CancellationToken token = default)
        {
            _ = _connection?.StopAsync();
            _ = _connection?.DisposeAsync();
            
            _connection = new HubConnectionBuilder()
                    .WithUrl(uri)
                    .WithAutomaticReconnect()
                    .Build();
            try
            {
                await _connection.StartAsync(token);
                IsConnected = true;
            }
            catch (Exception ex)
            {
            }
            return;
        }


        public async Task SaveStringList(List<string> list, string fileName)
        {
            try
            {
                await _connection.InvokeAsync("SaveStringList", list, fileName);
            }
            catch (Exception ex)
            {
            }
        }

        public List<string> LoadStringList(string fileName)
        {
            try
            {
                var result = _connection.InvokeAsync<List<string>>("LoadStringList", fileName);
                return result.Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
