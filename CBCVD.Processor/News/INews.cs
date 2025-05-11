using Discord;

namespace CBCVD.Processor.News;

public interface INews
{
    public Embed BuildMessage();
}
