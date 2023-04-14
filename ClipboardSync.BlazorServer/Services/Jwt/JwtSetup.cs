using ClipboardSync.BlazorServer.Models;
using ClipboardSync.BlazorServer.Services.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ClipboardSync.BlazorServer.Services.Jwt
{
    /// <summary>
    /// JWT配置
    /// </summary>
    public static class JwtSetup
    {
		/// <summary>
		/// 添加 JWT 中间件。
		/// https://blog.imguan.com/2022/03/09/aspnetcore-signalr-jwt/
		/// </summary>
		public static void AddJwtBearer(this WebApplicationBuilder builder)
        {
            //将配置文件中的相关内容反序列化
            var tokenInfo = builder.Configuration.GetSection("JwtConfiguration").Get<JwtConfiguration>();
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    //验证失败时的处理
                    OnAuthenticationFailed = context =>
                    {
                        //若失败类型为过期，则返回特定Header，便于客户端判断
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers.Add("tokenErr", "expired");
                        return Task.CompletedTask;
                    },
                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenInfo.AccessSecret)),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ValidIssuer = tokenInfo.Issuer,
                    ValidAudience = tokenInfo.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(tokenInfo.ClockSkew)
                };
            });
        }
    }

}
