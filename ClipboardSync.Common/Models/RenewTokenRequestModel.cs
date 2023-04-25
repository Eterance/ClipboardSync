using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync.Common.Models
{
	public class RenewTokenRequestModel
	{
		public JwtTokenModel? RefreshToken { get; set; }
		public bool? IsRenewRefreshToken { get; set; }
	}
}
