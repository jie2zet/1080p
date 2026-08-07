using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using CMP.Extensions;
using HandyControl.Data;

namespace CMP.ViewModels.Common.Message
{
    public  class MessageGrowlViewModel:BindableBase
    {
        public MessageGrowlViewModel(IEventAggregator eventAggregator,IRegionManager regionManager)
        {
            eventAggregator.GetEvent<GrowlEvent>().Subscribe(Recrive);
        }
        private void Recrive(GrowMsg obj)
        {
            GrowlInfo growlInfo = new GrowlInfo();
            growlInfo.Message = obj.message;
            growlInfo.WaitTime = 3;
            string msgType = obj.enumAlarmType;
            switch (msgType)
            {
                case "Success":
                    HandyControl.Controls.Growl.Success(growlInfo);
                    break;
                case "Info":
                    HandyControl.Controls.Growl.Info(growlInfo);
                    break;
                case "Warning":
                    HandyControl.Controls.Growl.Warning(growlInfo);
                    break;
                case "Error":
                    HandyControl.Controls.Growl.Error(growlInfo);
                    break;
                case "Fatal":
                    HandyControl.Controls.Growl.Fatal(growlInfo);
                    break;
            }
        }
    }
}
