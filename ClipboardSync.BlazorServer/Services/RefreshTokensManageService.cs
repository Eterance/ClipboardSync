using ClipboardSync.BlazorServer.Models;
using ClipboardSync.Common.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace ClipboardSync.BlazorServer.Services
{
	public class RefreshTokensManageService
    {
		private IConfiguration _configuration;
		private List<JwtTokenModel> _refreshTokens = new();
		private readonly string fileName = "RefreshTokens.xml";


		public RefreshTokensManageService(IConfiguration config)
		{
			_configuration = config;
			string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _configuration["DataFolderName"]);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
			string fullFileName = Path.Combine(directoryPath, fileName);
			if (File.Exists(fullFileName) == false)
			{
				SaveList();
			}
			XmlSerializer serializer = new XmlSerializer(typeof(List<JwtTokenModel>));
			using (StreamReader reader = new StreamReader(fullFileName, Encoding.UTF8))
			{
                _refreshTokens = serializer.Deserialize(reader) as List<JwtTokenModel> ?? new();
			}
		}

		public bool Contains(JwtTokenModel refreshToken)
		{
            foreach (var item in _refreshTokens)
            {
				if (refreshToken.Token == item.Token)
				{
                    return true;
                }
            }
			return false;
        }

        public bool Remove(JwtTokenModel refreshToken)
        {
			for (int i = 0; i < _refreshTokens.Count; i++)
			{ 
				var item = _refreshTokens[i];
                if (refreshToken.Token == item.Token)
                {
					_refreshTokens.Remove(item);
                    SaveList();
                    return true;
                }
            }
            return false;
        }

        public bool Remove(string refreshToken)
        {
            for (int i = 0; i < _refreshTokens.Count; i++)
            {
                var item = _refreshTokens[i];
                if (refreshToken == item.Token)
                {
                    _refreshTokens.Remove(item);
                    SaveList();
                    return true;
                }
            }
            return false;
        }

        public void Add(JwtTokenModel refreshToken)
        {
            _refreshTokens.Add(refreshToken);
            SaveList();
        }

        public void ReplaceAndAdd(JwtTokenModel oldRefreshToken, JwtTokenModel newRefreshToken)
        {
            Remove(oldRefreshToken);
            Add(newRefreshToken);
        }

        public void SaveList()
		{
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _configuration["DataFolderName"]);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fullFileName = Path.Combine(directoryPath, fileName);
            XmlSerializer serializer = new XmlSerializer(typeof(List<JwtTokenModel>));
			using (StreamWriter writer = new StreamWriter(fullFileName, false, Encoding.UTF8))
			{
				serializer.Serialize(writer, _refreshTokens);
			}
		}

	}
}
