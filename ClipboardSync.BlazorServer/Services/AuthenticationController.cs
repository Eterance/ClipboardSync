using ClipboardSync.BlazorServer.Models;
using ClipboardSync.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClipboardSync.BlazorServer.Services
{
	[Route("api/auth")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private IConfiguration _configuration;
		private CredentialsService _credentialsService;
		// TODO: 完整的 refresh token管理器，可读写到本地
		private List<string> _validRefreshTokens;
		private ILogger<AuthenticationController> _logger;
		private bool isDebug = true;


        public AuthenticationController(IConfiguration config, CredentialsService credentialsService, List<string> validRefreshTokens, ILogger<AuthenticationController> logger)
		{
			_configuration = config;
			_credentialsService = credentialsService;
			_validRefreshTokens = validRefreshTokens;
			_logger = logger;
		}

		[HttpGet("ping")]
		public IActionResult Ping()
		{
			return Ok("OK");
		}

        // POST api/<JwtTokenController>
        // https://www.c-sharpcorner.com/article/how-to-implement-jwt-authentication-in-web-api-using-net-6-0-asp-net-core/
        [HttpPost("token/get")]
		public async Task<IActionResult> GetAccessToken(UserInfo _userInfo)
		{
			if (_userInfo != null && _userInfo.UserName != null && _userInfo.Password != null)
			{
				bool isExist = _credentialsService.ValidateCredentialExistence(_userInfo);

				if (isExist)
				{
					//create claims details based on the user information
					var accessToken = CreateJwtBearerToken(
						_configuration["JwtConfiguration:AccessSecret"],
						_configuration["JwtConfiguration:AccessExpiration"]);
					var refreshToken = CreateJwtBearerToken(
						_configuration["JwtConfiguration:RefreshSecret"],
						_configuration["JwtConfiguration:RefreshExpiration"]);
                    _validRefreshTokens.Add(refreshToken.Token);
                    _logger.LogInformation($"UTC {DateTime.UtcNow} Token Pairs Generate Premitted.");
					if (isDebug)
                    {
                        _logger.LogInformation($"UTC {DateTime.UtcNow} AccessToken: \"{accessToken.Token}\" Expiration: {accessToken.Expiration}");
                        _logger.LogInformation($"UTC {DateTime.UtcNow} RefreshToken: \"{refreshToken.Token}\" Expiration: {refreshToken.Expiration}");
                    }
                    return Ok(new JwtTokensPairModel()
					{
						AccessToken = accessToken,
						RefreshToken = refreshToken,
					});
                }
				else
                {
                    if (isDebug)
                    {
                        _logger.LogInformation($"UTC {DateTime.UtcNow} username: {_userInfo.UserName} PW: {_userInfo.Password}");
                    }
                    _logger.LogInformation($"UTC {DateTime.UtcNow} Token Pairs Generate Denied. Reason: Invalid user name or password");
                    return BadRequest("Invalid user name or password");
				}
			}
			else
            {
                _logger.LogInformation($"UTC {DateTime.UtcNow} Token Pairs Generate Denied. Reason: User name, password or both are empty");
                return BadRequest("User name, password or both are empty");
			}
		}

		// POST api/<JwtTokenController>
		// https://www.c-sharpcorner.com/article/how-to-implement-jwt-authentication-in-web-api-using-net-6-0-asp-net-core/
		[HttpPost("token/renew")]
		public async Task<IActionResult> RenewAccessToken(RenewTokenRequestModel renewTokenRequestModel)
		{
			if (renewTokenRequestModel != null
				&& renewTokenRequestModel.IsRenewRefreshToken != null
				&& renewTokenRequestModel.RefreshToken != null
                && renewTokenRequestModel.RefreshToken.Token != null)
			{
                // A valid RefreshToken and in the premitted list
                if (ValidateToken(renewTokenRequestModel.RefreshToken.Token, _configuration["JwtConfiguration:RefreshSecret"]) 
					&& _validRefreshTokens.Contains(renewTokenRequestModel.RefreshToken.Token))
				{
					var accessToken = CreateJwtBearerToken(
						_configuration["JwtConfiguration:AccessSecret"],
						_configuration["JwtConfiguration:AccessExpiration"]);
                    var refreshToken = renewTokenRequestModel.RefreshToken;
                    if ((bool)renewTokenRequestModel.IsRenewRefreshToken)
                    {
						refreshToken = CreateJwtBearerToken(
							_configuration["JwtConfiguration:RefreshSecret"],
							_configuration["JwtConfiguration:RefreshExpiration"]);
                        // Replace premited RefreshToken
                        _validRefreshTokens.Add(refreshToken.Token);
                        _validRefreshTokens.Remove(renewTokenRequestModel.RefreshToken.Token);
                    }
                    _logger.LogInformation($"UTC {DateTime.UtcNow} Token Pairs Renew Permitted. IsRenewRefreshToken:{renewTokenRequestModel.IsRenewRefreshToken}");
					if (isDebug)
                    {
                        _logger.LogInformation($"UTC {DateTime.UtcNow} AccessToken: \"{accessToken.Token}\" Expiration: {accessToken.Expiration}");
                        _logger.LogInformation($"UTC {DateTime.UtcNow} RefreshToken: \"{refreshToken.Token}\" Expiration: {refreshToken.Expiration}");
                    }
                    return Ok(new JwtTokensPairModel()
					{
						AccessToken = accessToken,
						RefreshToken = refreshToken,
					});
				}
				else
                {
                    if (isDebug)
                    {
                        _logger.LogInformation($"UTC {DateTime.UtcNow} RefreshToken: \"{renewTokenRequestModel.RefreshToken.Token}\" Expiration: {renewTokenRequestModel.RefreshToken.Expiration}");
                    }
                    _logger.LogInformation($"UTC {DateTime.UtcNow} Token Pairs Renew Denied. Reason: Invalid Refresh Token");
                    return BadRequest("Invalid Refresh Token");
                }
			}
			else
            {
                _logger.LogInformation($"UTC {DateTime.UtcNow} Token Pairs Renew Denied. Reason: Invalid Renewal Request");
                return BadRequest("Invalid Renewal Request");
			}
		}


        [HttpDelete("token/delete")]
        public void Delete(string RefreshToken)
        {
			_validRefreshTokens.Remove(RefreshToken);
            _logger.LogInformation($"UTC {DateTime.UtcNow} RefreshToken Removed.");
        }

        // PUT api/<JwtTokenController>/5
        [HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}


		private JwtTokenModel CreateJwtBearerToken(string secretKey, string expirationSeconds)
		{
			//create claims details based on the user information
			var claims = new[] {
						new Claim(JwtRegisteredClaimNames.Sub, _configuration["JwtConfiguration:Subject"]),
						new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
						new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
					};
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expire = DateTime.UtcNow.AddSeconds(int.Parse(expirationSeconds));
			var jwtAccessToken = new JwtSecurityToken(
				issuer: _configuration["JwtConfiguration:Issuer"],
				audience: _configuration["JwtConfiguration:Audience"],
				claims,
				expires: expire,
				signingCredentials: signInCredentials,
				notBefore: DateTime.UtcNow);
			return new JwtTokenModel()
			{
				Token = new JwtSecurityTokenHandler().WriteToken(jwtAccessToken),
				Expiration = expire,
			};
        }

        /// <summary>
        /// https://stackoverflow.com/a/71243969
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="UserName"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private bool ValidateToken(string authToken, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters(secretKey);

            SecurityToken validatedToken;
			try 
			{ 
				IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
				return true;
			}
            catch
			{
				return false;
			}
        }

        private TokenValidationParameters GetValidationParameters(string secretKey)
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtConfiguration:Issuer"],
                ValidAudience = _configuration["JwtConfiguration:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) // The same key as the one that generate the token
            };
        }
    }
}
