using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.Models
{
	public class JwtTokenModel
	{
		public string? Token { get; set; }
		public DateTime? Expiration { get; set; }
	}
}
