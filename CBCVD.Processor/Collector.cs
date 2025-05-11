using System.Net;
using System.Net.Http.Json;
using System.Timers;
using MemoryPack;
using MemoryPack.Compression;

namespace CBCVD.Processor;

public class Collector
{
    private string symbol = "BTC-USD";
    private string dataPath = Path.Combine(@"D:\projects\CoinBaseCVD", "data");

    private readonly SemaphoreSlim semaphore = new (10);
    private readonly Queue<Func<Task>> requestQueue = new ();
    private readonly System.Timers.Timer requestTimer = new (1000);
    private List<string> processHistory = new();

    private int startFrom = 0;

    public Collector(string symbol, int startFrom = 0)
    {
        this.symbol = symbol;
        this.dataPath = Path.Combine(dataPath, symbol);
        this.startFrom = startFrom;
    }

    public async Task Start()
    {
        RemoveUnCompletedTrade();
        requestTimer.Elapsed += HandleTimerElapsed;
        requestTimer.Start();
        Console.WriteLine("Run CB Collector");

        decimal lastTradeId = await GetLastTradeId(symbol);
        decimal maxTradeId = GetMaxTradeId();
        
        Console.WriteLine($"Need Update, l {lastTradeId}, m {maxTradeId}");

        var sdd = (int)(maxTradeId / 100_000);
        if (startFrom > sdd)
        {
            sdd = startFrom;
        }
        var idd = (int)(lastTradeId / 100_000);
        Console.WriteLine($"sdd {sdd}, {idd-sdd-1}");
        int limit = 1000;
        int offset = 1;
        int maxPage = 100;
        int sdx = sdd+2;
        int idx = idd-sdd-1;
        
        if(!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);
        
        for (int ddx = sdx; ddx <= (idx + sdx); ddx++)
        {
            List<Task<List<Trade>>> tasks = new List<Task<List<Trade>>>();
            TradeList<Trade> trades = new();

            var aggr = limit * maxPage * (ddx-1);
            // Console.SetCursorPosition(0,0);
            // processHistory = processHistory.Prepend($"aggr: {aggr}... {DateTime.Now}").Take(3).ToList();
            // Console.SetCursorPosition(0,0);

            processHistory.ForEach(Console.WriteLine);
            
            for (int i = 0; i < maxPage; i++)
            {
                var after = aggr + limit * (offset + i);
                tasks.Add(DebounceGetTrades(symbol: symbol, limit: limit, after: after));
            }

            var results = await Task.WhenAll(tasks);
            foreach (var result in results)
            {
                trades.AddRange(result);
            }
            trades.Sort((x, y) => x.trade_id.CompareTo(y.trade_id));            
            var t = new TradeList<Trade>();
            t.AddRange(trades.GroupBy(x => x.trade_id).Select(x => x.First()));
            /*Console.WriteLine($"{t.Count} trades");
            Console.WriteLine($"{t.First().trade_id} {t.Last().trade_id}");
            */
            
            using var compressor = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, t);
            string outputFileName = $"trades.{t.First().trade_id:00000000000}.{t.Last().trade_id:00000000000}.bin";
            File.WriteAllBytes(Path.Combine(dataPath,outputFileName), compressor.ToArray());
        }

    }
    
    private decimal GetMaxTradeId()
    {
        string[] files = Directory.GetFiles(dataPath, "*.bin");
        int tradeId = 0;
        foreach (var file in files)
        {
            var exploded = file.Split('.');
            var id = int.Parse(exploded[2]);
            tradeId = Math.Max(tradeId, id);
        }

        return tradeId;
    }

    private void RemoveUnCompletedTrade()
    {
        string[] files = Directory.GetFiles(dataPath, "*.bin");
        string lastFile = files.OrderByDescending(x => int.Parse(x.Split('.')[2])).First();
        var exploded = lastFile.Split('.');
        decimal sid = decimal.Parse(exploded[1]);
        decimal eid = decimal.Parse(exploded[2]);

        if (eid - sid != 99999)
        {
            File.Delete(lastFile);
        }
    }

    private async Task<int> GetLastTradeId(string symbol = "BTC-USD")
    {
        using HttpClientHandler handler = new();
        handler.UseDefaultCredentials = true;
        handler.UseProxy = false;
        handler.AutomaticDecompression = DecompressionMethods.GZip;
        
        using HttpClient client = new(handler);
        
        client.DefaultRequestHeaders.Clear();
        client.Timeout = TimeSpan.FromSeconds(2);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
                    
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "max-age=0");
        //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-2");
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        client.BaseAddress = new Uri("https://api.exchange.coinbase.com");
        string endpoint = $"/products/{symbol}/trades";
        HttpResponseMessage response = await client.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            var trades = await response.Content.ReadFromJsonAsync<List<Trade>>();
            return trades.Distinct().Max(x => x.trade_id);
        }
        throw new Exception("Failed to Get Last Trade Id");
    } 

    private async Task<List<Trade>> DebounceGetTrades(string symbol = "BTC-USD", int after = 10, int limit = 1000)
    {
        var task = new TaskCompletionSource<List<Trade>>();
        requestQueue.Enqueue(() => GetTrades(task, symbol, after, limit));
        return await task.Task;
    }

    private async Task GetTrades(TaskCompletionSource<List<Trade>> task, string symbol = "BTC-USD", int after = 10, int limit = 1000)
    {
        /*Console.WriteLine($"after: {after} limit: {limit}");
        Console.SetCursorPosition(0,1);
        Console.SetCursorPosition(0,1);*/
        using HttpClientHandler handler = new();
        handler.UseDefaultCredentials = true;
        handler.UseProxy = false;
        handler.AutomaticDecompression = DecompressionMethods.GZip;
        
        
        using HttpClient client = new(handler);
        
        client.DefaultRequestHeaders.Clear();
        client.Timeout = TimeSpan.FromSeconds(2);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
                    
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "max-age=0");
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        client.BaseAddress = new Uri("https://api.exchange.coinbase.com");
        string endpoint = $"/products/{symbol}/trades";

        string requestUri = $"{endpoint}?after={after}&limit={limit}";
        try
        {
            await semaphore.WaitAsync();
            
            HttpResponseMessage response = await client.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                task.SetResult(await response.Content.ReadFromJsonAsync<List<Trade>>());
            }
            else
            {

                requestQueue.Enqueue(() => GetTrades(task, symbol, after, limit));

                /*Console.SetCursorPosition(0,8);
                Console.WriteLine($"{response.StatusCode}] Something Wrong happend after: {after} limit: {limit}");*/
            }
        }
        catch (Exception ex)
        {
            // Console.SetCursorPosition(0,8);
            requestQueue.Enqueue(() => GetTrades(task, symbol, after, limit));
            Console.WriteLine($"{ex.Message}] Something Wrong happend after: {after} limit: {limit}");
        }
        finally
        {
            semaphore.Release();
        }
    }
    
    
    private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
    {
        int remainingRequests = semaphore.CurrentCount;

        while (remainingRequests > 0 && requestQueue.Count > 0)
        {
            var request = requestQueue.Dequeue();
            Task.Run(() => request());
            remainingRequests--;
        }
    }
}
