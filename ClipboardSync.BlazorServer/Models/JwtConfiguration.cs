namespace ClipboardSync.BlazorServer.Models
{
    /// <summary>
    /// JWT Token配置
    /// </summary>
    public class JwtConfiguration
    {
        /// <summary>
        ///     签发人
        /// </summary>
        public string Issuer { get; set; } = string.Empty;
        /// <summary>
        ///     受众
        /// </summary>
        public string Audience { get; set; } = string.Empty;
		/// <summary>
		///     
		/// </summary>
		public string Subject { get; set; } = string.Empty;
		/// <summary>
		///     AccessToken密钥
		/// </summary>
		public string AccessSecret { get; set; } = string.Empty;
        /// <summary>
        ///     RefreshToken密钥
        /// </summary>
        public string RefreshSecret { get; set; } = string.Empty;
        /// <summary>
        ///     AccessToken有效时长
        /// </summary>
        public int AccessExpiration { get; set; }
        /// <summary>
        ///     RefreshToken有效时长
        /// </summary>
        public int RefreshExpiration { get; set; }
        /// <summary>
        ///     允许的时差
        /// </summary>
        public int ClockSkew { get; set; }
    }

}
