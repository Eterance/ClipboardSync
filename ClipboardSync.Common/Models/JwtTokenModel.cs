using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.Models
{
	public class JwtTokenModel
	{
		public string? Token { get; set; }
		/// <summary>
		/// UTC Time.
		/// </summary>
		public DateTime? Expiration { get; set; }
	}
}
