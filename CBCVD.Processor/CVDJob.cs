using CBCVD.Processor.Config;
using CBCVD.Processor.Messaging.Commands;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace CBCVD.Processor;

public class CVDJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
            var now = DateTime.Now;
            var requestTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            
            var processor = new CVDProcessor();
            var cvds = await processor.Run();

            try
            {
                // await Messaging.Telegram.SendMessage(cvds);
                var _client = new DiscordSocketClient(new()
                {
                    LogLevel = LogSeverity.Verbose,
                    GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged,
                });
                
                await _client.LoginAsync(TokenType.Bot, ConfigManager.Get(ConfigKey.DiscordBotToken));
                await _client.StartAsync();
                
                var channel =
                    await _client.GetChannelAsync(ulong.Parse(ConfigManager.Get(ConfigKey.DiscordOnChainChannelId))) as
                        ITextChannel;
              
                var attachments = cvds.Select(x => new FileAttachment(x.Value,
                    $"coinbase.cvd.{x.Key}.{DateTime.Now.Ticks}.png", $"Coinbase CVD {x.Key} (2W)"));
                await channel.SendFilesAsync(attachments,
                    $"Coinbase CVD (2W) {DateTime.Now:yyyy-MM-dd HH:mm:ss} [{String.Join(", ", cvds.Keys)}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.InnerException.ToString());
                // throw ex;
            }
            // if (CVDModule._lastGenerated < requestTime || !CVDModule._latestResult.Any())
            // {
            //     await CVDModule.GenerateCVD();
            //     await Messaging.Discord.SendCVD(CVDModule._latestResult);
            // }

        
    }
}
