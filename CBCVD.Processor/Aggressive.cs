using MemoryPack;
using MemoryPack.Compression;

namespace CBCVD.Processor;

public class Aggressive
{
    private string symbol = "BTC-USD";
    private string dataPath = Path.Combine(@"D:\projects\CoinBaseCVD", "data");
    private string aggressivePath = Path.Combine(@"D:\projects\CoinBaseCVD", "aggrs");
    
    public Aggressive(string symbol)
    {
        this.symbol = symbol;
        this.dataPath = Path.Combine(dataPath, symbol);
        this.aggressivePath = Path.Combine(aggressivePath, symbol);
    }
    
    
    public void Start()
    {
        var trades = new TradeList<Trade>();
        string[] files = Directory.GetFiles(dataPath);
        // var targetMonth = new DateTime(2023, 07, 1);
        // var lastFileIndex = 5466;
        var targetMonth = new DateTime(2023, 11, 1);
        var lastFileIndex = 5734;

        switch (symbol)
        {
            case "BTC-USD":
                targetMonth = new DateTime(2023, 11, 1);
                lastFileIndex = 5734;
                break;
            
            case "SOL-USD":
                targetMonth = new DateTime(2023, 11, 1);
                lastFileIndex = 938;
                break;
            case "ETH-USD":
                targetMonth = new DateTime(2023, 11, 1);
                lastFileIndex = 4749;
                lastFileIndex = 1310;
                break;
        }

        for (var current = targetMonth; current <= DateTime.Now; current = current.AddMonths(1))
        {
            Console.WriteLine($"Trades for {current}");

            for (var i = lastFileIndex; i < files.Length; i++)
            {
                using var decompressor = new BrotliDecompressor();
                var fileName = files[i];
                var buffer = File.ReadAllBytes(fileName);
                var decompressedBuffer = decompressor.Decompress(buffer);
                var _trades = MemoryPackSerializer.Deserialize<TradeList<Trade>>(decompressedBuffer);
                // Console.WriteLine($"{_trades.Count} trades {fileName}");

                var filtered = _trades.Where(t => t.time >= current && t.time < current.AddMonths(1)).ToList();

                if (filtered.Count == 0)
                {
                    lastFileIndex = i-1;
                    break;
                }
                else
                {
                    trades.AddRange(filtered);

                }
            }

            trades.Sort( (a, b) => a.trade_id.CompareTo(b.trade_id));
            var data = trades.GroupBy(t => new { t.time.Ticks, t.side })
                .Select(
                    g =>
                    {
                        return new Trade()
                        {
                            trade_id = g.First().trade_id,
                            time = g.First().time,
                            size = g.Sum(t => t.size),
                            price = g.Average(t => t.price),
                            side = g.Key.side
                        };
                    }).ToList();

            TradeList<Trade> result = new ();
            result.AddRange(data);

            Console.WriteLine($"{result.Count} Trades for {current}");

            using var compressor = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, result);
            string outputFileName = $"trades.{current:yyyy-MM}.bin";
            if(!Directory.Exists(aggressivePath))
                Directory.CreateDirectory(aggressivePath);
            File.WriteAllBytes(Path.Combine(aggressivePath,outputFileName), compressor.ToArray());
            trades.Clear();

        }

        
    }

}
