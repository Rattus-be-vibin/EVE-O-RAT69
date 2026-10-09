namespace EveOPreview.Configuration.Implementation;

internal class AppConfig : IAppConfig
{
	public string ConfigFileName { get; set; }

	public AppConfig()
	{
		ConfigFileName = null;
	}
}
