using CMP.Models.Operation.LoadPort;
using CMP.Services.LoadPort;
using Prism.Events;

namespace CMP.Extensions
{
    //消息事件
    class MessageEvents : PubSubEvent<FormData>
    {
     
    }
    public class GrowlEvent : PubSubEvent<GrowMsg>
    {

    }
    //操作事件
    public class OperateEvent : PubSubEvent<string>
    {

    }
    public class AddGroupOrUserEvent : PubSubEvent<string>
    {
    }
    public class CancelEvent : PubSubEvent { }

    /// <summary>
    /// LoadPort 管理器完成初始化后通知主界面刷新 Slot Maps。
    /// </summary>
    public class LoadPortsReadyEvent : PubSubEvent { }

    /// <summary>
    /// LoadPort 状态发生变化时发布；EAP 或其他模块自行订阅。
    /// </summary>
    public class LoadPortStatusChangedEvent : PubSubEvent<LoadPortStatusChangedEventArgs> { }

    /// <summary>
    /// LoadPort 状态变更事件参数。
    /// </summary>
    public class LoadPortStatusChangedEventArgs
    {
        /// <summary>串口名，如 COM12。</summary>
        public string PortName { get; init; } = string.Empty;

        /// <summary>界面显示序号，COM12–COM15 对应 1–4。</summary>
        public int DisplayNumber { get; init; }

        /// <summary>LoadPort 索引，0–3。</summary>
        public int LoadportId { get; init; }

        /// <summary>上一帧流程状态。</summary>
        public LoadportFoupStatus PreviousWorkflow { get; init; }

        public LoadportFoupStatus CurrentWorkflow { get; init; }

        /// <summary>当前硬件状态快照；尚未收到 STATE 响应时可能为 null。</summary>
        public LoadPortStatus? HardwareStatus { get; init; }

        /// <summary>RFID 读到的 CarrierId；未读到或 FOUP 移走后为空。</summary>
        public string CarrierId { get; init; } = string.Empty;

        /// <summary>端口搬送状态，对应 FOUP 流程 <see cref="LoadportFoupStatus"/>。</summary>
        public LoadportFoupStatus PortTransferStatus { get; init; }

        /// <summary>相对上一帧发生变化的字段名。</summary>
        public IReadOnlyList<string> ChangedFields { get; init; } = Array.Empty<string>();

        public DateTime Timestamp { get; init; } = DateTime.Now;
    }

    
    /// <summary>
    /// Recipe保存事件，用于通知其他组件Recipe已被保存
    /// </summary>
    public class RecipeSavedEvent : PubSubEvent<RecipeSavedEventArgs>
    {
    }
    /// <summary>
    /// 获取Config下已设置的Recipe选择项
    /// </summary>
    public class GetRecipeConfigEvent : PubSubEvent<GetRecipeConfigEventArgs>
    {     
    }
   
    public class GrowMsg
    {
        public string enumAlarmType="";
        public string message = "";
        public string token = "";
    }
    //表单数据
    public class FormData
    {
        public string operateType = "";
        public object data;
        public string FormName = "";
    }
    
    /// <summary>
    /// Recipe保存事件参数
    /// </summary>
    public class RecipeSavedEventArgs
    {
        /// <summary>
        /// Recipe类型/文件夹名称（如 "Load Recipe", "Polish Recipe" 等）
        /// </summary>
        public string? RecipeType { get; set; }
        
        /// <summary>
        /// Recipe名称
        /// </summary>
        public string? RecipeName { get; set; }
        
        /// <summary>
        /// 保存时间
        /// </summary>
        public DateTime SavedTime { get; set; }
    }

    public class GetRecipeConfigEventArgs
    {
        /// <summary>
        /// Recipe类型/文件夹名称（如 "Load Recipe", "Polish Recipe" 等）
        /// </summary>
        public string? RecipeType { get; set; }
        /// <summary>
        /// Recipe的文件名称
        /// </summary>
        public string? RecipeName { get;set; }
        /// <summary>
        /// 对应的EC配置文件名称
        /// </summary>
        public string? ConfigFileName { get; set; }
        /// <summary>
        /// 是否需要提示用户，默认不提示
        /// </summary>
        public bool IsNeedTip { get; set; }
    }
}
