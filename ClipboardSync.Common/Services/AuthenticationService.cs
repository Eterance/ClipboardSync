using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClipboardSync.Common.Services
{
    public class AuthenticationService
    {
        public string? ServerUrl { get; set; }
        private ISettingsService _settingService;
        private HttpClient _httpClient = new HttpClient();
        private ILogger<AuthenticationService>? _logger;

        JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public AuthenticationService(ISettingsService jwtTokenReadWriteHelper) 
        {
            _settingService = jwtTokenReadWriteHelper;
        }

        public AuthenticationService(ISettingsService jwtTokenReadWriteHelper, ILogger<AuthenticationService> logger)
        {
            _settingService = jwtTokenReadWriteHelper;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>(bool) false: Network error; (bool) false: need to re-login; string?: 
        /// refreshed AccessToken, will be null if not all true</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<Tuple<bool, bool, string?>> GetAccessTokenAsync()
        {
            if (ServerUrl == null)
            {
                throw new NullReferenceException("serverUri is null.");
            }
            JwtTokensPairModel? tokens = await _settingService.GetJwtTokensPairAsync(ServerUrl);
            if (tokens != null)
            {
                // Renew if AccessToken expired
                if (tokens.AccessToken.Expiration < DateTime.UtcNow)
                {
                    var sss = await RefreshAccessTokenAsync(tokens.RefreshToken);
                    if (sss.Item1 == false)// network error
                    {
                        return Tuple.Create(false, true, default(string));
                    }
                    if (sss.Item2 == false) // refreshtoken expired, re-login
                    {
                        return Tuple.Create(true, false, default(string));
                    }
                    return Tuple.Create(true, true, sss.Item3);
                }
                else
                { // Renew in background if AccessToken will expire within 10 sec
                    if (tokens.AccessToken.Expiration?.AddSeconds(10) < DateTime.UtcNow)
                    {
                        _ = RefreshAccessTokenAsync(tokens.AccessToken);
                    }
                    return Tuple.Create(true, true, tokens.AccessToken.Token);
                }
            }
            // Never login, need login
            else 
            { 
                return Tuple.Create(true, false, default(string));
            }
        }

        public async Task DeleteTokensPairAsync()
        {
            if (ServerUrl == null)
            {
                throw new NullReferenceException("serverUri is null.");
            }
            await _settingService.DeleteJwtTokensPairAsync(ServerUrl);
        }

        /// <summary>
        /// Login async.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns>(bool) false: Network error; (bool) false: username or password error; string?: 
        /// refreshed AccessToken, will be null if not all true</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<Tuple<bool, bool, string?>> LoginAsync(UserInfo userInfo)
        {
            if (ServerUrl == null)
            {
                throw new NullReferenceException("serverUri is null.");
            }
            Uri uri = new Uri($"{ServerUrl}api/auth/token/get");
            string json = JsonSerializer.Serialize(userInfo, _serializerOptions);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync(uri, content);
            }
            catch
            {
                // Network Error
                return Tuple.Create(false, true, default(string));
            }
            if (response.IsSuccessStatusCode)
            {
                string response_content = await response.Content.ReadAsStringAsync();
                try
                {
                    JwtTokensPairModel tokensPair = JsonSerializer.Deserialize<JwtTokensPairModel>(response_content, _serializerOptions) ?? new();
                    await _settingService.SetJwtTokensPairAsync(ServerUrl, tokensPair);
                    // Login success
                    return Tuple.Create(true, true, tokensPair.AccessToken.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    // WTF Error
                    return Tuple.Create(false, true, default(string));
                }

            }
            else
            {
                // User name or password wrong
                return Tuple.Create(true, false, default(string));
            }
        }

        /// <summary>
        /// Refresh AccessToken using RefreshToken. RefreshToken is automatically refreshed if it is nearly to expire.
        /// </summary>
        /// <param name="RefreshToken"></param>
        /// <returns>(bool) false: Network error; (bool) false: RefreshAccess expired, please login again; string?: 
        /// refreshed AccessToken, will be null if not all true</returns>
        /// <exception cref="NullReferenceException">serverUri is null.</exception>
        private async Task<Tuple<bool, bool, string?>> RefreshAccessTokenAsync(JwtTokenModel RefreshToken)
        {
            if (ServerUrl == null)
            {
                throw new NullReferenceException("serverUri is null.");
            }
            Uri uri = new Uri($"{ServerUrl}api/auth/token/renew");
            RenewTokenRequestModel renew = new()
            {
                RefreshToken = RefreshToken,
                // Whether expire in 1 hour, if so refresh RefreshToken
                IsRenewRefreshToken = RefreshToken.Expiration?.AddSeconds(3600) < DateTime.UtcNow,
            };
            string json = JsonSerializer.Serialize(renew, _serializerOptions);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync(uri, content);
            }
            catch
            {
                // Network Error
                return Tuple.Create(false, true, default(string));
            }
            if (response.IsSuccessStatusCode)
            {
                string response_content = await response.Content.ReadAsStringAsync();
                try
                {
                    JwtTokensPairModel tokensPair = JsonSerializer.Deserialize<JwtTokensPairModel>(response_content, _serializerOptions) ?? new();
                    await _settingService.SetJwtTokensPairAsync(ServerUrl, tokensPair);
                    // Refresh success
                    return Tuple.Create(true, true, tokensPair.AccessToken.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    // WTF Error
                    return Tuple.Create(false, true, default(string));
                }

            }
            else
            {
                // Invalid Refresh Token
                return Tuple.Create(true, false, default(string));
            }
        }

        public async Task<bool> Ping()
        {
            if (ServerUrl == null)
            {
                throw new NullReferenceException("serverUri is null.");
            }
            Uri uri = new Uri($"{ServerUrl}api/auth/test/ping");
            try
            {
                var response = await _httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Network Error
                return false;
            }
        }

        public async Task<bool> ValidateAccessToken()
        {
            if (ServerUrl == null)
            {
                throw new NullReferenceException("serverUri is null.");
            }
            Uri uri = new Uri($"{ServerUrl}api/auth/test/auth");
            try
            {
                var response = await _httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Network Error
                return false;
            }
        }
    }
}
