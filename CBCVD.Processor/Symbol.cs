namespace CBCVD.Processor;

public class Symbol
{
    public string symbol { get; set; }
    private TimeSpan range = TimeSpan.FromDays(14);

    public int collectStartFrom = 0;
    public DateTime aggrStartFrom = DateTime.Now;
    public int aggrStartFromIndex = 0;
}
