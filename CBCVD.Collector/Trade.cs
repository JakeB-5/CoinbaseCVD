using MemoryPack;

namespace CBCVD.Collector;


//{"trade_id":1,"side":"buy","size":"0.01000000","price":"300.00000000","time":"2014-12-01T05:33:56.761199Z"}

[MemoryPackable(SerializeLayout.Explicit)]
public partial class Trade
{
    [MemoryPackOrder(0)]
    public int trade_id { get; set; }
    
    [MemoryPackOrder(1)]
    public string side { get; set; }
    
    [MemoryPackOrder(2)]
    public decimal size { get; set; }
    
    [MemoryPackOrder(3)]
    public decimal price { get; set; }

    [MemoryPackIgnore]
    public decimal amount => size * price;
    
    [MemoryPackOrder(4)]
    public DateTime time { get; set; }
    
    public override string ToString()
    {
        return string.Format("{0,10}{1,8}{2,10:0.000} {3,11:0.00} {7,10:#.00}    {4}.{5:000}{6:000}", trade_id, side, size, price, time.ToString("yyyy-MM-dd HH:mm:ss"),time.Millisecond, time.Microsecond, amount);
    }
}


[MemoryPackable(GenerateType.Collection)]
public partial class TradeList<T> : List<T> { }
