using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.Models
{
	public class JwtTokensPairModel
	{
		public JwtTokenModel? AccessToken { get; set; }

		public JwtTokenModel? RefreshToken { get; set; }
	}
}
