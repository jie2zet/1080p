using CMP.Models.Config;
using CMP.Models.Dialog;
using DocumentFormat.OpenXml.Bibliography;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMP.ViewModels.Common.Dialog
{
    public class ExportCsvDialogViewModel : BindableBase, IDialogResultable<ExportFileModel>
    {
        private ExportFileModel _result;
        public ExportFileModel Result { get => _result; set => SetProperty(ref _result, value); }
        public Action? CloseAction { get; set; }

        private Visibility _isVisable;

        public Visibility IsVisable { get => _isVisable; set => SetProperty(ref _isVisable, value); }

        public DelegateCommand SelectFolderCommand { get; set; }
        public DelegateCommand<object> ComboBoxChangedCommand { get; set; }
        public DelegateCommand OkCommand => new(() => CloseAction?.Invoke());
        public DelegateCommand CancelCommand => new(() => { Result = new() { Directory = string.Empty, FileName = string.Empty }; CloseAction?.Invoke(); });

        public ExportCsvDialogViewModel()
        {
            _result = new ExportFileModel() { SelectedExportWay = ExportWay.All };
            _isVisable = _result.SelectedExportWay == ExportWay.Single ? Visibility.Visible : Visibility.Hidden;
            SelectFolderCommand = new DelegateCommand(OnSelectFolder);
            ComboBoxChangedCommand = new DelegateCommand<object>(OnComboBoxChanged);
        }

        private void OnComboBoxChanged(object obj)
        {
            if (obj == null) return;
            if (obj is not ExportWay way) return;
            IsVisable = way == ExportWay.Single ? Visibility.Visible : Visibility.Hidden;
            Result.SelectedExportWay = way;
        }

        private void OnSelectFolder()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            dialog.Title = "请选择文件夹";
            if (dialog.ShowDialog() == true)
            {
                Result.Directory = dialog.FolderName;
            }
        }
    }
}
