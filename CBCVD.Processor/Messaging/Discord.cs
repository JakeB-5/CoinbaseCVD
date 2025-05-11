using System.Reflection;
using CBCVD.Processor.Config;
using CBCVD.Processor.News;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace CBCVD.Processor.Messaging;

public class Discord
{
    
    private static readonly Lazy<Discord> lazy = new Lazy<Discord>(() => new Discord());
    public static Discord Instance { get { return lazy.Value; } }
    private DiscordSocketClient _client { get; set; }
    private CommandService commands { get; set; } 
    private string _botToken { get; set; }
    
    protected Discord()
    {
        _client = new DiscordSocketClient(new ()
        {
            LogLevel = LogSeverity.Verbose,
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged, 
        });
        commands = new CommandService(new CommandServiceConfig()        //명령어 수신 클라이언트 초기화
        {
            LogLevel = LogSeverity.Verbose                              //봇의 로그 레벨 설정
        });
        
        _client.Log += Log;
        commands.Log += Log;
        _client.MessageReceived += MessageReceived;
        _client.Ready += () =>
        {
            Console.WriteLine("Discord Bot Ready!");
            return Task.CompletedTask;
        };
        _botToken = ConfigManager.Get(ConfigKey.DiscordBotToken);
        Connect().Wait();
    }

    private async Task MessageReceived(SocketMessage msg)
    {
        var message = msg as SocketUserMessage;
        if (message == null) return;
        int pos = 0;
        if (!(message.HasCharPrefix('!', ref pos) ||
              message.HasMentionPrefix(_client.CurrentUser, ref pos)) ||
            message.Author.IsBot)
            return;
        var context = new SocketCommandContext(_client, message);                    //수신된 메시지에 대한 컨텍스트 생성   

        //await context.Channel.SendMessageAsync("명령어 수신됨 - " + message.Content);
        var result = await commands.ExecuteAsync(                context: context, argPos: pos, services: null);  
        Console.WriteLine($"Message received: {message.Content} from {message.Channel.Id}");
    }


    private async Task Connect()
    {
        if (_client.LoginState != LoginState.LoggedIn)
        {
            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();
            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
            Console.WriteLine("Discord Bot Connected!");
            await Task.Delay(-1);

        }
    }
    

    public static async Task SendCVD(Dictionary<string, Stream> cvds)
    {
        
        var _client = new DiscordSocketClient(new ()
        {
            LogLevel = LogSeverity.Verbose,
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged, 
        });
        
        var channel = await _client.GetChannelAsync(ulong.Parse(ConfigManager.Get(ConfigKey.DiscordOnChainChannelId))) as ITextChannel;

        var threads = await channel.GetActiveThreadsAsync();
        var threadName = $"Coinbase CVD {DateTime.Now:yyyy-MM-dd}";
        var thread = threads.First(x => x.Name.Equals(threadName));
        
        if (!threads.Any(x => x.Name.Equals(threadName)))
        {
            await channel.CreateThreadAsync(threadName);
        }
        else
        {
            thread = threads.First(x => x.Name.Equals(threadName));
        }
        var attachments = cvds.Select(x => new FileAttachment(x.Value, $"coinbase.cvd.{x.Key}.{DateTime.Now.Ticks}.png", $"Coinbase CVD {x.Key} (2W)"));
        await thread.SendFilesAsync(attachments, $"Coinbase CVD (2W) {DateTime.Now:yyyy-MM-dd HH:mm:ss} [{String.Join(", ", cvds.Keys)}]");
        // await Discord.Instance._sendCVD(cvds);
    }

    private async Task _sendNews(INews news)
    {
        var _client = new DiscordSocketClient(new ()
        {
            LogLevel = LogSeverity.Verbose,
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged, 
        });
            
        await _client.LoginAsync(TokenType.Bot, ConfigManager.Get(ConfigKey.DiscordBotToken));
        await _client.StartAsync();

        var channel = await _client.GetChannelAsync(ulong.Parse(ConfigManager.Get(ConfigKey.DiscordTestChannelId))) as IMessageChannel;
        await channel.SendMessageAsync("", embed: news.BuildMessage());
    }
    
    public static async Task SendNews(INews news)
    {
        var client = new DiscordSocketClient(new ()
        {
            LogLevel = LogSeverity.Verbose,
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged, 
        });
            
        await client.LoginAsync(TokenType.Bot, ConfigManager.Get(ConfigKey.DiscordBotToken));
        await client.StartAsync();

        var channel = await client.GetChannelAsync(ulong.Parse(ConfigManager.Get(ConfigKey.DiscordNewsChannelId))) as IMessageChannel;
        await channel.SendMessageAsync("", embed: news.BuildMessage());
    }
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}
