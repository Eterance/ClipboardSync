using Blazored.SessionStorage;
using ClipboardSync.BlazorServer.Data;
using ClipboardSync.BlazorServer.Services;
using ClipboardSync.BlazorServer.Services.Jwt;
using ClipboardSync.Common.Models;
using ClipboardSync.Common.Services;
using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Blazored.LocalStorage;

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
            //builder.Services.AddBlazoredSessionStorage();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddSingleton<MessageCacheService>();
			builder.Services.AddSingleton<CredentialsService>();
            //builder.Services.AddSingleton<List<string>>();
            builder.Services.AddSingleton<RefreshTokensManageService>();

            builder.Services.AddScoped<UriModel>();
            builder.Services.AddScoped<ClipboardSignalRService>();
            builder.Services.AddScoped<BlazorServerClipboardService>();
            builder.Services.AddScoped<IPinnedListFileHelper, RemotePinnedListFileHelper>();
            builder.Services.AddScoped<ISettingsService, BlazorServerClientSettingsService>();
            builder.Services.AddScoped<AuthenticationService>();
            builder.Services.AddScoped<ClipboardViewModel>();
            builder.AddJwtBearer();


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

			app.UseAuthentication();
			app.UseAuthorization();

            app.MapControllers();
			app.MapBlazorHub();
            app.MapHub<ServerHub>($"/ServerHub");
            app.MapHub<TestAuthHub>($"/TestAuthHub");
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}