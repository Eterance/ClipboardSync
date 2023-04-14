using ClipboardSync.BlazorServer.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace ClipboardSync.BlazorServer.Services
{
	public class CredentialsService
	{
		private IConfiguration _configuration;
		private List<UserInfo> _credentials;
		private readonly string fileName = "Credentials.xml";


		public CredentialsService(IConfiguration config)
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
				SaveList(new List<UserInfo>(), fullFileName);
			}
			XmlSerializer serializer = new XmlSerializer(typeof(List<UserInfo>));
			using (StreamReader reader = new StreamReader(fullFileName, Encoding.UTF8))
			{
				_credentials = serializer.Deserialize(reader) as List<UserInfo> ?? new();
			}
			if (_credentials.Count == 0)
			{
				_credentials.Add(new UserInfo()
				{
					UserName = _configuration["DefaultCredential:UserName"], 
					Password = _configuration["DefaultCredential:Password"]
				});
			}
		}

		public bool ValidateCredentialExistence(UserInfo userInfo) 
		{
            foreach (var item in _credentials)
            {
				if (item.UserName == userInfo.UserName)
				{
					if (item.Password == userInfo.Password)
					{ 
						return true; 
					}
					else
					{ 
						return false; 
					}
				}
            }
			return false;
        }

		public void SaveList(List<UserInfo> list, string fullFileName)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(List<UserInfo>));
			using (StreamWriter writer = new StreamWriter(fullFileName, false, Encoding.UTF8))
			{
				serializer.Serialize(writer, list);
			}
		}

	}
}
