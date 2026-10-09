using System.IO;
using Newtonsoft.Json;

namespace EveOPreview.Configuration.Implementation;

internal class ConfigurationStorage : IConfigurationStorage
{
	private const string CONFIGURATION_FILE_NAME = "EVE-O Preview.json";

	private readonly IAppConfig _appConfig;

	private readonly IThumbnailConfiguration _thumbnailConfiguration;

	public ConfigurationStorage(IAppConfig appConfig, IThumbnailConfiguration thumbnailConfiguration)
	{
		_appConfig = appConfig;
		_thumbnailConfiguration = thumbnailConfiguration;
	}

	public void Load()
	{
		string configFileName = GetConfigFileName();
		if (File.Exists(configFileName))
		{
			string value = File.ReadAllText(configFileName);
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ObjectCreationHandling = ObjectCreationHandling.Replace
			};
			JsonConvert.PopulateObject(value, _thumbnailConfiguration, settings);
			_thumbnailConfiguration.ApplyRestrictions();
		}
	}

	public void Save()
	{
		string contents = JsonConvert.SerializeObject(_thumbnailConfiguration, Formatting.Indented);
		string configFileName = GetConfigFileName();
		try
		{
			File.WriteAllText(configFileName, contents);
		}
		catch (IOException)
		{
		}
	}

	private string GetConfigFileName()
	{
		return string.IsNullOrEmpty(_appConfig.ConfigFileName) ? "EVE-O Preview.json" : _appConfig.ConfigFileName;
	}
}
