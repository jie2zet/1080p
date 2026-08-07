using System;
using System.Windows;
using HandyControl.Controls;

namespace CMP.ViewModels.Common.Dialog
{
    public class DeleteWaferDialogViewModel : BindableBase
    {
        private string _title = "Delete Wafer";
        private string _loadportID = string.Empty;
        private string _slotID = string.Empty;
        private string _currentChamberName = string.Empty;
        public string CurrentChamberName
        {
            get { return _currentChamberName; }
            set { SetProperty(ref _currentChamberName, value); }
        }
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public int ResultLoadportID { get; private set; }
        public int ResultSlotID { get; private set; }

        public string LoadportID
        {
            get { return _loadportID; }
            set { SetProperty(ref _loadportID, value); }
        }

        public string SlotID
        {
            get { return _slotID; }
            set { SetProperty(ref _slotID, value); }
        }

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand OKCommand { get; set; }

        public Action<bool?> CloseWindowAction { get; set; }

        public DeleteWaferDialogViewModel(string chamberName = "")
        {
            CurrentChamberName = chamberName ?? string.Empty;
            CancelCommand = new DelegateCommand(CancelClick);
            OKCommand = new DelegateCommand(OKClick);
        }

        private void OKClick()
        {
            if (!ValidateInput())
            {
                return;
            }

            if (int.TryParse(LoadportID, out int loadportId) && int.TryParse(SlotID, out int slotId))
            {
                ResultLoadportID = loadportId - 1;
                ResultSlotID = slotId - 1;
                CloseWindowAction?.Invoke(true);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(LoadportID))
            {
                Growl.WarningGlobal("请输入 Loadport ID");
                return false;
            }

            if (!int.TryParse(LoadportID, out int loadportId) || loadportId < 1)
            {
                Growl.WarningGlobal("Loadport ID 必须是有效的数字");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SlotID))
            {
                Growl.WarningGlobal("请输入 Slot ID");
                return false;
            }

            if (!int.TryParse(SlotID, out int slotId) || slotId < 1 || slotId > 25)
            {
                Growl.WarningGlobal("Slot ID 必须是 1-25 之间的有效数字");
                return false;
            }

            return true;
        }

        private void CancelClick()
        {
            CloseWindowAction?.Invoke(false);
        }
    }
}

