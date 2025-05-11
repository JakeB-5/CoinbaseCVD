using Discord;

namespace CBCVD.Processor.News;

public abstract class AbstractNews : INews
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    protected AbstractNews(int id, string title, string content)
    {
        Id = id;
        Title = title;
        Content = content;
    }

    public virtual Embed BuildMessage()
    {
        EmbedBuilder builder = new EmbedBuilder();
        builder.Title = $"📰 {Title}";
        builder.Description = Content;
        builder.Color = new Color(0x23, 0xb6, 0xad);
        builder.AddField("Disclaimer", "📢본 정보는 사실확인이 완료되지 않았습니다.\n📢본 정보를 사용한 __**투자손익 등 기타손실**__에 대해서 책임지지 않습니다.");
        return builder.Build();
    }
}
