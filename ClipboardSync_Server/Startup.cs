using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync_Server
{
    public class Startup
    {
        // https://stackoverflow.com/questions/64839928/how-do-i-add-signalr-to-a-net-core-windows-service
        // https://learn.microsoft.com/zh-cn/aspnet/core/signalr/background-services?view=aspnetcore-5.0
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MessageCache>();
            services.AddSignalR();
            services.AddHostedService<Worker>(); 
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();               // pre-requisite for app.UseEndpoints()
            app.UseEndpoints(endpoints =>
            {
                string url = $"/ServerHub";
                endpoints.MapHub<ServerHub>(url);
            });
        }
    }
}