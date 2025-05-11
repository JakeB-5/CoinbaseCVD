using CBCVD.Processor.Messaging.Commands.Arguments;
using Discord.Commands;

namespace CBCVD.Processor.Messaging.Commands;

public class TestModule : ModuleBase<SocketCommandContext>
{

    [Command("test")]
    public async Task TestCommand(string namedArg)
    {
        await Context.Channel.SendMessageAsync($"Received: {Context.Message.Content}, arg: {namedArg}");
    }
}
