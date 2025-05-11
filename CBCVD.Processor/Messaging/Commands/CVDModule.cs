using CBCVD.Processor.Config;
using Discord;
using Discord.Commands;

namespace CBCVD.Processor.Messaging.Commands;

public class CVDModule : ModuleBase<SocketCommandContext>
{
    public static Dictionary<string, Stream> _latestResult = new Dictionary<string, Stream>();
    public static DateTime _lastGenerated = DateTime.MinValue;
    public static IUserMessage _latestMessage { get; set; }

    private static bool _isGenerating { get; set; } = false;
    
    [Command("cvd")]
    public async Task CvdCommand()
    {

        if (_isGenerating)
        {
            await Context.Message.ReplyAsync("CVD 생성 중... Wait!!");
            return;
        }
            
        var now = DateTime.Now;
        var requestTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        
        _ = Task.Run(async () =>
        {
            Console.WriteLine($"{_lastGenerated}, {requestTime}");
            if (_lastGenerated < requestTime || !_latestResult.Any())
            {
                await Context.Message.ReplyAsync("CVD 생성 중...");
                await GenerateCVD();
                var attachments = _latestResult.Select(x => new FileAttachment(x.Value, $"coinbase.cvd.{x.Key}.{DateTime.Now.Ticks}.png", $"Coinbase CVD {x.Key} (2W)"));
            
                // var channel = await Context.Client.GetChannelAsync(ulong.Parse(ConfigManager.Get(ConfigKey.DiscordOnChainChannelId))) as IMessageChannel;
                var thread = await GenerateThread();
                _latestMessage = await thread.SendFilesAsync(attachments, $"Coinbase CVD (2W) {DateTime.Now:yyyy-MM-dd HH:mm:ss} [{String.Join(", ", _latestResult.Keys)}]");
            }
            
            await Context.Channel.SendMessageAsync($"{_latestMessage.GetJumpUrl()} Coinbase CVD {_lastGenerated:yyyy-MM-dd HH}");
        });
        
    }

    public static async Task GenerateCVD()
    {
        _isGenerating = true;
        var processor = new CVDProcessor();
        var result = await processor.Run();
        _latestResult = result;
        _lastGenerated = DateTime.Now;
        _isGenerating = false;
    }

    private async Task<IThreadChannel> GenerateThread(string symbol="")
    {
        var channel = await Context.Client.GetChannelAsync(ulong.Parse(ConfigManager.Get(ConfigKey.DiscordOnChainChannelId))) as ITextChannel;

        var threads = await channel.GetActiveThreadsAsync();
        var threadName = $"Coinbase CVD {DateTime.Now:yyyy-MM-dd}";
        // var threadName = $"Coinbase CVD {symbol} {DateTime.Now:yyyy-MM-dd}";
        if (!threads.Any(x => x.Name.Equals(threadName)))
        {
            return await channel.CreateThreadAsync(threadName);
        }
        
        return threads.First(x => x.Name.Equals(threadName));
    }
    
}
