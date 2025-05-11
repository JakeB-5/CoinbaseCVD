namespace CBCVD.Processor.Workers;

public abstract class BaseWorker : IDisposable, IWorker
{
    private System.Threading.Timer timer;// = new System.Timers.Timer();
    protected abstract int Interval { get; set; }
    protected abstract bool IsWorkOnFirstTime { get; set; }
    protected abstract TimeSpan? targetTime { get; set; }
    
    public void Start()
    {
        int due = 1000000;
        int period = Interval;

        if (!IsWorkOnFirstTime)
            due = Interval;
        else
            due = 0;
        
        if (targetTime != null && IsWorkOnFirstTime)
        {
            period = (int)targetTime.Value.TotalMilliseconds;
        }


        timer = new System.Threading.Timer(BaseTimerElapsed, null, due, period);

    }

    private void BaseTimerElapsed(object? sender)
    {
        TimerElapsed(sender);

        if (targetTime != null)
        {
            RecalculateTargetTime();
        }
    }

    private void RecalculateTargetTime()
    {
        DateTime now = DateTime.Now;
        DateTime targetDateTime = DateTime.Today.Add(targetTime.Value);
        if (now > targetDateTime)
            targetDateTime = targetDateTime.AddDays(1);

        var tick = (targetDateTime - now).TotalMilliseconds;
        timer.Change((int)tick, (int)TimeSpan.FromDays(1).TotalMilliseconds);
    }

    public void Dispose()
    {
        timer.Dispose();
    }


    public abstract void TimerElapsed(object? sender);
}
