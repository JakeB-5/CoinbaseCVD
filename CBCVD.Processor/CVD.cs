namespace CBCVD.Processor;


public class CVD
{
    public DateTime Timestamp { get; set; }
    public decimal CumulativeVolume { get; set; }
    public decimal CumulativeAmount { get; set; }
    
    public decimal DeltaVolume { get; set; }
    public decimal DeltaAmount { get; set; }
    
    public decimal Delta { get; set; }
    
}

public class CVDSeries
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    
    public List<CVD> CVDData = new List<CVD>();

    public CVDSeries(decimal min, decimal max)
    {
        Min = min;
        Max = max;
    }

    public void Normalize()
    {
        if (CVDData.Count > 0)
        {
            var min = CVDData.Where(x=>x.CumulativeVolume!=0).Min(x => x.CumulativeVolume);
            var max = CVDData.Where(x=>x.CumulativeVolume!=0).Max(x => x.CumulativeVolume);
            Console.WriteLine($"${Min} - {CVDData.Count} - {min:0.00} - {max:0.00}");
            if(max-min != 0)
                CVDData.ForEach(x => x.Delta = (x.CumulativeVolume - min) / (max - min));
            
        }
    }
}
