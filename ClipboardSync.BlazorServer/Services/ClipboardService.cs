using Microsoft.JSInterop;

namespace ClipboardSync.BlazorServer.Services
{
    /// <summary>
    /// Access clipboard in browser using JS Rumtime.
    /// https://www.meziantou.net/copying-text-to-clipboard-in-a-blazor-application.htm
    /// </summary>
    public sealed class ClipboardService
    {
        private readonly IJSRuntime _jsRuntime;

        public ClipboardService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask<string> ReadTextAsync()
        {
            return _jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
        }

        public ValueTask WriteTextAsync(string text)
        {
            return _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
