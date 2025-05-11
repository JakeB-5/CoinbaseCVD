namespace CBCVD.Processor.Config;

public class ConfigManager
{
    
    public static string DataPath = @"D:\projects\CoinBaseCVD";

    private static readonly Lazy<ConfigManager> lazy = new Lazy<ConfigManager>(() => new ConfigManager());
    public static ConfigManager Instance { get { return lazy.Value; } }

    private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();
    protected ConfigManager()
    {
        LoadConfigData();
    }

    private void LoadConfigData()
    {
        using StreamReader sr = new StreamReader(Path.Combine(DataPath, "data.ini"));
        String line;
        String delimStr = "=";
        char[] delimiter = delimStr.ToCharArray();
        while ((line = sr.ReadLine()) != null)
        {
            var strData = line.Split(delimiter);
            _settings.Add(strData[0], strData[1]);
        }
    }
    
    private string GetSetting(ConfigKey key)
    {
        return _settings[Enum.GetName(typeof(ConfigKey), key)];
    }

    public static string Get(ConfigKey key, string defaultValue = "")
    {
        return Instance.GetSetting(key);
    }
}


public enum ConfigKey
{
    DiscordBotToken,
    DiscordOnChainChannelId,
    DiscordTestChannelId,
    DiscordNewsChannelId,
    TelegramBotToken,
    TelegramChannelId,
}

