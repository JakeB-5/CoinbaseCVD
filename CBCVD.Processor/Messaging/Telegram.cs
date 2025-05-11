using CBCVD.Processor.Config;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CBCVD.Processor.Messaging;

public class Telegram
{
    
    private static readonly Lazy<Telegram> lazy = new Lazy<Telegram>(() => new Telegram());
    public static Telegram Instance { get { return lazy.Value; } }
    private string _botToken { get; set; }

    protected Telegram()
    {
        _botToken = ConfigManager.Get(ConfigKey.TelegramBotToken);
    }
    
    public static async Task SendMessage(Dictionary<string, Stream> cvds)
    {
        var bot = new TelegramBotClient(Instance._botToken);
        // var t = bot.;
        var attachments =
            cvds.Select(x => new InputMediaPhoto(new InputFileStream(x.Value, $"coinbase.cvd.{x.Key}.{DateTime.Now.Ticks}.png")) { });
        
        
        var t = await bot.SendMediaGroupAsync(ConfigManager.Get(ConfigKey.TelegramChannelId), attachments);
        //bot.SendMediaGroupAsync()
    }
}
