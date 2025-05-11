namespace CBCVD.Processor.Workers;

public interface IWorker
{
    void TimerElapsed(object? sender);
    void Start();
    void Dispose();
}
