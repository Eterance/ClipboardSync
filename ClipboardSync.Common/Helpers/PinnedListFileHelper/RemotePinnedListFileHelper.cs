using ClipboardSync.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClipboardSync.Common.Helpers
{
    /// <summary>
    /// Save/Load PinnedList at Server side via SignalR.
    /// </summary>
    public class RemotePinnedListFileHelper : IPinnedListFileHelper
    {
        readonly static string _xmlName = "pinnedList.xml";
        //private SignalRRemoteFilesService _signalRService;
        private HttpClient _httpClient;
        public UriModel UriModel { get; set; }
        JsonSerializerOptions _serializerOptions;

        public RemotePinnedListFileHelper(UriModel uriModel)
        {
            // https://github.com/xamarin/xamarin-forms-samples/blob/main/WebServices/TodoREST/TodoREST/Data/RestService.cs
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            UriModel = uriModel;
            _httpClient = new();
        }
        public async void Save(List<string> list)
        {
            Uri uri = new Uri($"{UriModel.RootUri}api/files/stringlist?filename={_xmlName}");
            string json = JsonSerializer.Serialize(list, _serializerOptions);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(uri, content);
        }

        public async Task<List<string>> Load()
        {
            List<string> Items = new List<string>();
            Uri uri = new Uri($"{UriModel.RootUri}api/files/stringlist?filename={_xmlName}");
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                Items = JsonSerializer.Deserialize<List<string>>(content, _serializerOptions) ?? Items;
                return Items;
            }
            else 
            {
                throw new Exception();
            }
        }
    }
}
