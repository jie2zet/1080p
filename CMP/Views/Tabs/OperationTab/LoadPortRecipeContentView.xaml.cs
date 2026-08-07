using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMP.Views.Tabs.OperationTab
{
    /// <summary>
    /// LoadPortRecipeContentView.xaml 的交互逻辑
    /// </summary>
    public partial class LoadPortRecipeContentView : UserControl
    {
        public LoadPortRecipeContentView()
        {
            InitializeComponent();
        }

        private void BatchWaferNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            ApplyBatchWaferName(sender as TextBox);
            e.Handled = true;
        }

        private void ApplyBatchWaferName(TextBox? textBox)
        {
            if (textBox == null || System.Windows.Controls.Validation.GetHasError(textBox))
            {
                return;
            }

            var applyMethod = DataContext?.GetType().GetMethod("ApplyBatchWaferName");
            applyMethod?.Invoke(DataContext, [textBox.Text]);

            textBox.Clear();
        }
    }
}
