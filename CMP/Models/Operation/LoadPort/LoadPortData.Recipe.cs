using CMP.Services.Auth;
using CMP.Models.Resource;
using CMP.Services.LoadPort;
using CMP.Services.Log;
using CMP.ViewModels.Tabs.OperationTab;
using CMP.Views.Tabs.OperationTab;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using System.Windows;
using System.Windows.Input;
using Prism.Events;
using Prism.Ioc;

namespace CMP.Models.Operation.LoadPort
{
    public partial  class LoadPortData
    {
        // Recipe 命令属性（如果尚未初始化）
        public ICommand AssignCommand { get; private set; }
        public ICommand RejectCommand { get; private set; }

        private void  initLoadPortRecipe() {
            AssignCommand = OperationAudit.Create<string>("Assign", ExecuteAssignCommand, () => TabName, CanAssignCommand).ObservesProperty(() => IsConnected);
            RejectCommand = OperationAudit.Create<string>("Reject", ExecuteRejectCommand, () => TabName, CanRejectCommand).ObservesProperty(() => IsConnected);
        }

      
        private void ExecuteAssignCommand(string obj) {

          
            // 使用 HandyControl 的对话框
            ShowHandyControlWindow();
          
        }

        // 或者使用传统的 Window 方式但使用 HandyControl 样式
        // 在 LoadPortData.cs 的 ShowHandyControlWindow 方法中
        private void ShowHandyControlWindow()
        {
            try
            {
                // 创建窗口内容（使用 HandyControl 样式）
                var recipeWindow = new HandyControl.Controls.Window
                {
                    Title = $"Recipe Assignment - {PortNumber}",
                    Width = 1750,
                    Height = 1000,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                // 设置内容
                var content = new LoadPortRecipeContentView();
                
                var eventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
                var viewModel = new LoadPortRecipeContentViewModel(this, "", eventAggregator);
                viewModel.ResetAndInitialize(); // 重置选中状态
                _ = viewModel.InitializeRuntimeStateAsync();
                // 设置关闭窗口的回调
                viewModel.CloseWindowAction = (dialogResult) =>
                {
                    recipeWindow.DialogResult = dialogResult;
                    recipeWindow.Close();
                };

                content.DataContext = viewModel;
                recipeWindow.Content = content;

                // 设置所有者窗口
                var parentWindow = Application.Current.Windows.OfType<HandyControl.Controls.Window>()
                                             .FirstOrDefault(w => w.IsActive);
                if (parentWindow != null)
                {
                    recipeWindow.Owner = parentWindow;
                }

                // 显示对话框
                var result = recipeWindow.ShowDialog();
                // 处理结果...
            }
            catch (Exception ex)
            {
                Growl.Error($"Error showing recipe window: {ex.Message}");
                AddLogMessage($"Error showing recipe window: {ex.Message}");
            }
        }

       
    
        private void ExecuteRejectCommand(string obj)
        {
            Growl.Info("reject");
        }

        private bool CanAssignCommand(string obj)
        {
            return IsConnected; 
        }
        private bool CanRejectCommand(string obj)
        {

            return IsConnected; 
        }
    }
}
