using CMP.Models.Warn;
using CMP.Services.Warn;
using CMP.Extensions;
using DryIoc;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System.Data;
using System.Reflection.Emit;
using Unity;

namespace CMP.ViewModels.Tabs.WarnTab
{
    public class AlarmEditFormViewModel : BindableBase
    {

        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private IAlarmService alarmService;
        public string Title => "Edit Alarm";

        private Alarm _currentAlarm;
        public Alarm CurrentAlarm
        {
            get => _currentAlarm;
            set => SetProperty(ref _currentAlarm, value);
        }
        public List<DefaultOperation> DefaultOperations { get; }

        public List<AlarmType> AlarmTypes { get; }

        // ✅ 正确实现 IDialogAware.RequestClose
        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public AlarmEditFormViewModel(IEventAggregator eventAggregator,IRegionManager regionManager)
        {
            eventAggregator.GetEvent<MessageEvents>().Subscribe(Receive);
            eventAggregator.GetEvent<OperateEvent>().Subscribe(Submit);
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;

            // 初始化枚举列表
            AlarmTypes = Enum.GetValues(typeof(AlarmType))
                             .Cast<AlarmType>()
                             .ToList();
           


            // 初始化枚举列表
            DefaultOperations = Enum.GetValues(typeof(DefaultOperation))
                             .Cast<DefaultOperation>()
                             .ToList();

            alarmService = new AlarmService();
        
        }

        private void Receive(FormData formData)
        {
           // Console.WriteLine(  data);


            string operateType = formData.operateType;
            
            if (operateType == "编辑")
            {
                CurrentAlarm = formData.data as Alarm;
                
            }
            else
            {
                CurrentAlarm = new Alarm();
            }
        }

        private void Submit(string operation)
        {
            try
            {
                bool result=false;
                
                if (operation == "新增")
                {
                    result= alarmService.AddAlarm(CurrentAlarm);
                }else if(operation == "编辑")
                {
                    result= alarmService.UpdateAlarm(CurrentAlarm);
                }                
                if (result)
                {
                    GrowMsg growMsg = new GrowMsg();
                    growMsg.enumAlarmType = "Success";
                    growMsg.message = "操作成功";
                    _regionManager.RequestNavigate("RegionMessage", "MessageGrowl");
                    _eventAggregator.GetEvent<GrowlEvent>().Publish(growMsg);
                    _eventAggregator.GetEvent<CancelEvent>().Publish();

                }
            }
            catch (Exception ex)
            {
                GrowMsg growMsg = new GrowMsg();
                growMsg.enumAlarmType = "Err";
                growMsg.message = ex.Message;
                _eventAggregator.GetEvent<GrowlEvent>().Publish(growMsg);
            }

        }

    }
}