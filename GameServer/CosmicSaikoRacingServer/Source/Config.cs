using System.Text.Json;

namespace CSR;

public class Config
{
	public int LocalServerPort { get; set; }
	public int GameServerPort { get; set; }
	public string GameLogPath { get; set; }
	public string LocalNetworkLogPath { get; set; }
	public string GameNetworkLogPath { get; set; }
	public string ModuleChipPath { get; set; }
	public string MatchingServerAddress { get; set; }
	public int MatchingServerPort { get; set; }
	public string CardDataPath { get; set; }
}

public class ConfigManager
{
	public static Config Config { private set; get; }
	public static void Load()
	{
		Config? config;
		string configStr;
		configStr = File.ReadAllText("Config.json");
		config = JsonSerializer.Deserialize<Config>(configStr);

		if (config == null)
			throw new Exception("config is null");
		Config = config;
	}
	
}