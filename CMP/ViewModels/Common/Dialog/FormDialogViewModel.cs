using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CMP.Extensions;
using Microsoft.SqlServer.Server;
using Prism.Navigation.Regions;

namespace CMP.ViewModels.Common.Dialog
{
    internal class FormDialogViewModel : BindableBase, IDialogAware
    {
        IRegionManager _RegionManager;
        IEventAggregator _EventAggregator;
        public DialogCloseListener RequestClose { get; }
        public DelegateCommand CancelCommand { get; set;  }
        public DelegateCommand ConfirmCommand { get; set; }
        private string _title = "弹窗";
        private int _Height = 500;
        private int _Width = 400;
        private string FormName = "";
        private object data;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, Title); }
        }
        public int Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, value); }
        }
        public int Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, value); }
        }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }
        public FormDialogViewModel(IRegionManager regionManager,IEventAggregator aggregator)
        {
            _RegionManager = regionManager;
            _EventAggregator = aggregator;
            CancelCommand = new DelegateCommand(Cancel);
            ConfirmCommand = new DelegateCommand(Confirm);
            _EventAggregator.GetEvent<CancelEvent>().Subscribe(Cancel);
        }
        //关闭弹窗
        private void Cancel()
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.No));
        }
        //确定
        private void Confirm()
        {
            _EventAggregator.GetEvent<OperateEvent>().Publish(_title);
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            FormName = parameters.GetValue<string>("FormName");
            if (parameters.ContainsKey("Title"))
            {
                _title = parameters.GetValue<string>("Title");
                data = parameters.GetValue<Object>("Data");
                Height = parameters.GetValue<int>("Height");
                Width = parameters.GetValue<int>("Width");
            }
            _RegionManager.RequestNavigate("RegionForm", FormName);
            FormData formData = new FormData();
            formData.operateType = Title;
            formData.data = data;
            _EventAggregator.GetEvent<MessageEvents>().Publish(formData);
        }
    }
}
