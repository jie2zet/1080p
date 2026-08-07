using CMP.Extensions;
using CMP.ViewModels.Tabs.ConfigTab;
using DryIoc.ImTools;
using HandyControl.Tools.Extension;
using SQLitePCL;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMP.ViewModels.Common.Dialog
{
    public class CopyColumnDialogViewModel : BindableBase, IDialogAware
    {
        private string? selectedZoneName;

        public string? SelectedZoneName
        {
            get { return selectedZoneName; }
            set 
            { 
                selectedZoneName = value;
                RemoveCipiedTargetZone();
                RaisePropertyChanged(); 
            }
        }


        private ObservableCollection<string> zoneNames;

        public ObservableCollection<string> ZoneNames
        {
            get { return zoneNames; }
            set
            {
                zoneNames = value;
                RaisePropertyChanged();
                SetCipiedTargetZone();
                SelectedZoneName = zoneNames.FirstOrDefault()!;
            }
        }

        private ObservableCollection<CopyCalibrationModel> copiedTargetZoneBackup;
        private ObservableCollection<CopyCalibrationModel> copiedTargetZone;

        public ObservableCollection<CopyCalibrationModel> CopiedTargetZone
        {
            get { return copiedTargetZone; }
            set { copiedTargetZone = value; RaisePropertyChanged(); }
        }       

        public DialogCloseListener RequestClose { get; }

        public DelegateCommand ConfirmCommand { get; set; }
        public DelegateCommand CancelCommand => new(() => {
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        });

        private IEventAggregator eventAggregator;

        public CopyColumnDialogViewModel(IEventAggregator eventAggregator)
        {
            zoneNames = [];
            copiedTargetZone = [];
            copiedTargetZoneBackup = [];
            this.eventAggregator = eventAggregator;
            ConfirmCommand = new DelegateCommand(OnConfirm);
        }

        private void OnConfirm()
        {
            DialogParameters param = new DialogParameters();
            if (copiedTargetZoneBackup == null) return;
            if (!CopiedTargetZone.Any(x => x.IsCopy)) return;
            var isCopyCollect = CopiedTargetZone.Where(x => x.IsCopy).ToList();
            param.Add("Result", isCopyCollect);
            param.Add("SelectedZoneName", SelectedZoneName!);
            DialogResult dialogResult = new DialogResult(ButtonResult.OK);
            dialogResult.Parameters = param;
            RequestClose.Invoke(dialogResult);
        }

        private void SetCipiedTargetZone()
        {
            CopiedTargetZone.Clear();
            foreach (var item in ZoneNames)
            {
                CopiedTargetZone.Add(new CopyCalibrationModel() {Name=item, IsCopy=false });
                copiedTargetZoneBackup.Add(new CopyCalibrationModel() {Name=item, IsCopy=false });
            }
        }

        private void RemoveCipiedTargetZone()
        {
            if (CopiedTargetZone == null) return;
            Application.Current.Dispatcher.Invoke(() => {
                CopiedTargetZone.Clear();
                foreach (var item in copiedTargetZoneBackup)
                {
                    CopiedTargetZone.Add(new CopyCalibrationModel() { Name = item.Name, IsCopy = item.IsCopy });
                }
                var model = CopiedTargetZone.FindFirst(x => x.Name == selectedZoneName);
                CopiedTargetZone.Remove(model);
            });
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            //throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters == null) return;
            ZoneNames = parameters.GetValue<ObservableCollection<string>>("ZoneNames");
        }
    }
}
