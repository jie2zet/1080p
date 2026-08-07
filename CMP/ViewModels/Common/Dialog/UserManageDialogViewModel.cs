using CMP.Models.Config;
using CMP.Models.UserData;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CMP.Extensions;

namespace CMP.ViewModels.Common.Dialog
{
    public class UserManageDialogViewModel:BindableBase,IDialogAware
    {
        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        private string blockName1;

        public string BlockName1
        {
            get { return blockName1; }
            set { blockName1 = value; RaisePropertyChanged(); }
        }

        private string blockName2;

        public string BlockName2
        {
            get { return blockName2; }
            set { blockName2 = value; RaisePropertyChanged(); }
        }

        private string blockName3;

        public string BlockName3
        {
            get { return blockName3; }
            set { blockName3 = value; RaisePropertyChanged(); }
        }

        private string textBox1;

        public string TextBox1
        {
            get { return textBox1; }
            set { textBox1 = value; RaisePropertyChanged(); }
        }
        

        private string textBox2;

        public string TextBox2
        {
            get { return textBox2; }
            set { textBox2 = value; RaisePropertyChanged(); }
        }

        private string textBox3;

        public string TextBox3
        {
            get { return textBox3; }
            set { textBox3 = value; RaisePropertyChanged(); }
        }

        private bool isClicCancle = false;

        public DelegateCommand CancelCommand { get; set; }

        public DelegateCommand OKCommand { get; set; }

        public DialogCloseListener RequestClose { get; }

        IEventAggregator _eventAggregator;

        public UserManageDialogViewModel( IEventAggregator eventAggregator )
        {
            _eventAggregator = eventAggregator;
            CancelCommand = new DelegateCommand(CancleClick);
            OKCommand = new DelegateCommand(OKClick);
        }

        private void OKClick()
        {
            DialogParameters param = new DialogParameters();
            if (Title == null) return;

            UserModel user = new UserModel();
            user.UserName = TextBox1;
            user.Password = TextBox2;
            if (Enum.TryParse<PermissionEnum>(TextBox3, true, out var level))
            {
                user.Level = level;
                user.Group = level.ToString();
            }
            else
            {
                user.Group = TextBox3;
            }

            if (Title.ToLower().Contains("add"))
            {
                param.Add("AddUserResult", user);
            }
            else if (Title.ToLower().Contains("edit"))
            {
                param.Add("EditUserResult", user);
            }


            _eventAggregator.GetEvent<AddGroupOrUserEvent>().Subscribe(ShowMessage);
            DialogResult dialogResult = new DialogResult(ButtonResult.OK);
            dialogResult.Parameters = param;
            RequestClose.Invoke(dialogResult);
        }

        private void CancleClick()
        {
            isClicCancle = true;
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public bool CanCloseDialog()
        {
            if (isClicCancle)
            {
                isClicCancle = false;
                return true;
            }

            var allowClose = true;
            if (string.IsNullOrEmpty(TextBox1) || string.IsNullOrEmpty(TextBox3))
            {
                allowClose = false;
                MessageBox.Show("不设置为空的文本框。");
            }
            else if (Title != null && Title.ToLower().Contains("add") && string.IsNullOrEmpty(TextBox2))
            {
                allowClose = false;
                MessageBox.Show("新增用户时密码不能为空。");
            }

            return allowClose;
        }

        public void OnDialogClosed()
        {
            //
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("Title");
            BlockName1 = parameters.GetValue<string>("BlockName1");
            BlockName2 = parameters.GetValue<string>("BlockName2");
            BlockName3 = parameters.GetValue<string>("BlockName3");
            TextBox1 = parameters.GetValue<string>("TextBox1");
            TextBox2 = parameters.GetValue<string>("TextBox2");
            TextBox3 = parameters.GetValue<string>("TextBox3");
        }
        public void ShowMessage(string message) 
        {
            MessageBox.Show(message, "", MessageBoxButton.OK);
            _eventAggregator.GetEvent<AddGroupOrUserEvent>().Unsubscribe(ShowMessage);
        }
    }
}
