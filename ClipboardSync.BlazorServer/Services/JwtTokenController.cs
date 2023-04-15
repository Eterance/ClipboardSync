﻿using ClipboardSync.BlazorServer.Models;
using ClipboardSync.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClipboardSync.BlazorServer.Services
{
	[Route("api/token")]
	[ApiController]
	public class JwtTokenController : ControllerBase
	{
		private IConfiguration _configuration;
		private CredentialsService _credentialsService;

		public JwtTokenController(IConfiguration config, CredentialsService credentialsService)
		{
			_configuration = config;
			_credentialsService = credentialsService;
		}

		// GET: api/<JwtTokenController>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/<JwtTokenController>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		// GET api/<JwtTokenController>/7
		[Route("api/token/7")]
		[HttpGet("{id}")]
		public string Get2(int id)
		{
			return id.ToString();
		}

		// POST api/<JwtTokenController>/7
		[Route("api/token/7")]
		[HttpPost]
		public async Task<IActionResult> Post2(string sss)
		{
			return Ok($"hello, {sss}!");
		}

		// POST api/<JwtTokenController>
		// https://www.c-sharpcorner.com/article/how-to-implement-jwt-authentication-in-web-api-using-net-6-0-asp-net-core/
		[HttpPost]
		public async Task<IActionResult> GetAccessToken(UserInfo _userInfo)
		{
			if (_userInfo != null && _userInfo.UserName != null && _userInfo.Password != null)
			{
				bool isExist = _credentialsService.ValidateCredentialExistence(_userInfo);

				if (isExist)
				{
					//create claims details based on the user information
					var accessToken = CreateJwtBearerToken(
						_userInfo.UserName.ToString(),
						_configuration["JwtConfiguration:AccessSecret"],
						_configuration["JwtConfiguration:AccessExpiration"]);
					var refreshToken = CreateJwtBearerToken(
						_userInfo.UserName.ToString(),
						_configuration["JwtConfiguration:RefreshSecret"],
						_configuration["JwtConfiguration:RefreshExpiration"]);
					return Ok(new JwtTokensPairModel()
					{
						AccessToken = accessToken,
						RefreshToken = refreshToken,
					});
				}
				else
				{
					return BadRequest("Invalid user name or password");
				}
			}
			else
			{
				return BadRequest("User name, password or both are empty");
			}
		}

		// POST api/<JwtTokenController>
		// https://www.c-sharpcorner.com/article/how-to-implement-jwt-authentication-in-web-api-using-net-6-0-asp-net-core/
		[HttpPost("renew")]
		public async Task<IActionResult> RenewAccessToken(RenewTokenRequestModel renewTokenRequestModel)
		{
			if (renewTokenRequestModel != null
				&& renewTokenRequestModel.IsRenewRefreshToken != null
				&& renewTokenRequestModel.UserName != null
				&& renewTokenRequestModel.RefreshToken != null)
			{
				//create claims details based on the user information
				var accessToken = CreateJwtBearerToken(
					renewTokenRequestModel.UserName,
					_configuration["JwtConfiguration:AccessSecret"],
					_configuration["JwtConfiguration:AccessExpiration"]);
				var refreshToken = (bool)renewTokenRequestModel.IsRenewRefreshToken 
					? CreateJwtBearerToken(
					renewTokenRequestModel.UserName,
					_configuration["JwtConfiguration:RefreshSecret"],
					_configuration["JwtConfiguration:RefreshExpiration"])
					: renewTokenRequestModel.RefreshToken;
				return Ok(new JwtTokensPairModel()
				{
					AccessToken = accessToken,
					RefreshToken = refreshToken,
				});
			}
			else
			{
				return BadRequest("User name, password or both are empty");
			}

		}

		// PUT api/<JwtTokenController>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<JwtTokenController>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}

		private JwtTokenModel CreateJwtBearerToken(string UserName, string secretKey, string expirationSeconds)
		{
			//create claims details based on the user information
			var claims = new[] {
						new Claim(JwtRegisteredClaimNames.Sub, _configuration["JwtConfiguration:Subject"]),
						new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
						new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
						new Claim("UserName", UserName),
					};
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expire = DateTime.UtcNow.AddSeconds(int.Parse(expirationSeconds));
			var jwtAccessToken = new JwtSecurityToken(
				_configuration["JwtConfiguration:Issuer"],
				_configuration["JwtConfiguration:Audience"],
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
	}
}
