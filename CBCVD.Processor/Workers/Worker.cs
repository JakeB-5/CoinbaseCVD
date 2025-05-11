namespace CBCVD.Processor.Workers;

public class Worker : IDisposable
{
    private List<IWorker> _workers = new List<IWorker>();

    public void AddWorker(IWorker worker)
    {
        _workers.Add(worker);
    }

    public void Start()
    {
        foreach (var worker in _workers)
        {
            worker.Start();
        }
    }

    public void Dispose()
    {
        foreach (var worker in _workers)
            worker.Dispose();
    }
}
