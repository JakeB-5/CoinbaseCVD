using MemoryPack;
using MemoryPack.Compression;

namespace CBCVD.Processor;

public class Calculator
{
    private string symbol = "BTC-USD";
    private string dataPath = Path.Combine(@"D:\projects\CoinBaseCVD", "data");
    private string aggressivePath = Path.Combine(@"D:\projects\CoinBaseCVD", "aggrs");
    private string outputPath = Path.Combine(@"D:\projects\CoinBaseCVD", "outputs");
    
    private List<CVDSeries> seriesList = new List<CVDSeries>();
    
    private int Range = -7;

    public Calculator(string symbol)
    {
        this.symbol = symbol;
        this.dataPath = Path.Combine(dataPath, symbol);
        this.aggressivePath = Path.Combine(aggressivePath, symbol);
        
        seriesList.Add(new (0, 1_000_000_000));
        seriesList.Add(new (100, 1_000));
        seriesList.Add(new (1_000, 10_000));
        seriesList.Add(new (10_000, 100_000));
        seriesList.Add(new (100_000, 1_000_000));
        seriesList.Add(new (1_000_000, 10_000_000));
        seriesList.Add(new (10_000_000, 1_000_000_000));
    }

    public (Dictionary<DateTime, decimal> closePrices, List<CVDSeries> seriesList) Start()
    {
        var target = false;
        string[] files = Directory.GetFiles(aggressivePath);
        Dictionary<DateTime, decimal> closePrices = new Dictionary<DateTime, decimal>();
        
        foreach (string file in files)
        {
            if (file == Path.Combine(aggressivePath,"trades.2024-07.bin"))
            {
                target = true;
            }

            if (!target) continue;
            
            // Console.WriteLine(file);
            
            using var decompressor = new BrotliDecompressor();

            var buffer = File.ReadAllBytes(file);
            var decompressedBuffer = decompressor.Decompress(buffer);
            var trades = MemoryPackSerializer.Deserialize<TradeList<Trade>>(decompressedBuffer);


            trades.GroupBy(x => new DateTime(x.time.Year, x.time.Month, x.time.Day, x.time.Hour, 0, 0))
                .Select(x =>
                {
                    closePrices.Add(x.Key, x.Last().price);
                    return 0;
                }).ToList();
            foreach (var cvdSeries in seriesList)
            {
                CalcCVD(cvdSeries, trades.Where(t => t.amount >= cvdSeries.Min && t.amount < cvdSeries.Max));
            }

        }

        return (closePrices: closePrices, seriesList: seriesList);
    }
    private void CalcCVD(CVDSeries series, IEnumerable<Trade> trades)
    {
        decimal cq = 0, ca = 0;
        if (series.CVDData.Any())
        {
            cq = series.CVDData.Last().CumulativeVolume;
            ca = series.CVDData.Last().CumulativeAmount;
            
        }
        series.CVDData.AddRange(
            trades.GroupBy(x => new DateTime(x.time.Year, x.time.Month, x.time.Day, x.time.Hour, 0, 0))
                .Select(x =>
                {
                    var q = x.Sum(y => y.side == "buy" ? y.size : -y.size);
                    var a = x.Sum(y => y.side == "buy" ? y.amount : -y.amount);
                    var dq = q;
                    var da = a;
                    cq += q;
                    ca += a;
                    return new CVD()
                    {
                        Timestamp = x.Key,
                        CumulativeVolume = cq,
                        CumulativeAmount = ca,
                        DeltaVolume = dq,
                        DeltaAmount = da,
                    };
                }));
    }
}
