using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClipboardSync.BlazorServer.Services
{
	[Authorize]
	[Route("api/testauthorize")]
	[ApiController]
	public class TestAuthorizeController : ControllerBase
	{
		// GET: api/<TestAuthorizeController>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "yes,", "you access TestAuthorizeController get api!" };
		}

		// GET api/<TestAuthorizeController>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return id.ToString();
		}

		// POST api/<TestAuthorizeController>
		[HttpPost]
		public void Post([FromBody] string value)
		{
			Console.WriteLine(value);
		}

		// PUT api/<TestAuthorizeController>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<TestAuthorizeController>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
