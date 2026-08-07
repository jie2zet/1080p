namespace CMP.Constants.Plc.RobotControl;

public static class CacheBodyAddress
{
    // A缓存腔体
    public const string CacheADoorOpenRead = "Y10C6";
    public const string CacheADoorOpenWrite = "M10100";
    public const string CacheADoorCloseRead = "Y10C7";
    public const string CacheADoorCloseWrite = "M10101";

    public const string CacheADoorOpenStatus = "X10B6";
    public const string CacheADoorCloseStatus = "X10B7";
    public const string CacheAFiber1Status = "X10B8";
    public const string CacheALiquidLevelStatus = "X10B9";
    public const string CacheADoorSwitchStatus = "X10BA";

    // B缓存腔体
    public const string CacheBDoorOpenRead = "Y10C8";
    public const string CacheBDoorOpenWrite = "M10200";
    public const string CacheBDoorCloseRead = "Y10C9";
    public const string CacheBDoorCloseWrite = "M10201";

    public const string CacheBDoorOpenStatus = "X10BB";
    public const string CacheBDoorCloseStatus = "X10BC";
    public const string CacheBFiber1Status = "X10BD";
    public const string CacheBLiquidLevelStatus = "X10BE";
    public const string CacheBDoorSwitchStatus = "X10BF";
}
