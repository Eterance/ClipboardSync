using ClipboardSync.BlazorServer.Data;
using ClipboardSync.BlazorServer.Hubs;
using ClipboardSync.BlazorServer.Services;
using ClipboardSync.Common.Services;
using ClipboardSync.Common.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.ResponseCompression;

namespace ClipboardSync.BlazorServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddSingleton<MessageCacheService>();

            builder.Services.AddScoped<SignalRRemoteFilesService>();
            builder.Services.AddScoped<SignalRCoreService>();
            builder.Services.AddScoped<IPinnedListFileService, RemotePinnedListFileService>();
            builder.Services.AddScoped<ISettingsService, BlazorServerClientSettingsService>();
            builder.Services.AddScoped<ClipboardManagementViewModel>();

            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            builder.WebHost.UseUrls("http://*:50001");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseResponseCompression();
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }


            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapHub<FilesHub>($"/FilesHub");
            app.MapHub<ServerHub>($"/ServerHub");
            app.MapHub<ChatHub>($"/ChatHub");
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}