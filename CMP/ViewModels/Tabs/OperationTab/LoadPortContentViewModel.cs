
using CMP.Models.Operation.LoadPort;
using CMP.Models.Resource;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using HandyControl.Controls;
using NetTaste;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Input;

namespace CMP.ViewModels.Tabs.OperationTab
{

    public class LoadPortContentViewModel : BindableBase
    {

        private ILoadPortService _loadPortService;

        private ObservableCollection<LoadPortData> _loadportDatas;

        public ObservableCollection<LoadPortData> LoadPortDatas
        {
            get => _loadportDatas;
            set => SetProperty(ref _loadportDatas, value);
        }

        public LoadPortContentViewModel(ILoadPortService loadPortService)
        {

            _loadPortService = loadPortService;
            LoadPortDatas = new ObservableCollection<LoadPortData>();

            // 从LoadPortManager获取共享实例
            var loadPortDict = LoadPortManager.Instance.GetAllLoadPorts();
            LoadPortDatas = new ObservableCollection<LoadPortData>(loadPortDict.Values);
        }

        
        // LoadPort数据模型
        

    }
}
