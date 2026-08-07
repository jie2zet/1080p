namespace CMP.Modules.Resource.Models
{
    /// <summary>
    /// 设备接口
    /// </summary>
    public interface IDevice
    {
        string Name { get; }
        DeviceStatus Status { get; }
        bool IsConnected { get; }
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public enum DeviceStatus
    {
        Disconnected,
        Uninitialized,
        Initialized,
        Manual,
        Auto,
        Alarm,
        Offline
    }

    /// <summary>
    /// 机器人手臂（从原位置迁移）
    /// </summary>
    public class RoboticArm : IDevice
    {
        public string Name { get; set; }
        public DeviceStatus Status { get; set; }
        public bool IsConnected { get; set; }
        // 其他属性从原RoboticArm迁移
    }

    /// <summary>
    /// 腔室（从原位置迁移）
    /// </summary>
    public class Chamber : IDevice
    {
        public string Name { get; set; }
        public DeviceStatus Status { get; set; }
        public bool IsConnected { get; set; }
        // 其他属性从原Chamber迁移
    }
}

