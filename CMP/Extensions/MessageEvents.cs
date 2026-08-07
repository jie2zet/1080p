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
        public string? RecipeType { get; set; }

        public string? RecipeName { get;set; }

        public string? ConfigFileName { get; set; }

        public bool IsNeedTip { get; set; }
    }
}
