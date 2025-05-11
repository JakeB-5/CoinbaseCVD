namespace CBCVD.Processor.Workers;

public class CVDWorker : BaseWorker
{
    protected override int Interval { get; set; }
    protected override bool IsWorkOnFirstTime { get; set; } = false;
    protected override TimeSpan? targetTime { get; set; } = TimeSpan.FromSeconds(60*61);
    public override void TimerElapsed(object? sender)
    {
        Task.Run(async () =>
        {
            Console.WriteLine("CVD Processor Start");
            var processor = new CVDProcessor();
            await processor.Run();
            Console.WriteLine("CVD Processor Finish");
        });
    }
}
