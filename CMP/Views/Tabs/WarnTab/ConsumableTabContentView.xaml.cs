using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CMP.Views.Tabs.WarnTab
{
    /// <summary>
    /// ConsumableTabContentView.xaml 的交互逻辑
    /// </summary>
    public partial class ConsumableTabContentView : UserControl
    {
        public ConsumableTabContentView()
        {
            InitializeComponent();
        }

        private void OnNumericPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsAllowedText(sender as TextBox, e.Text, IsFloatInput);
        }

        private void OnPositiveIntPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsAllowedText(sender as TextBox, e.Text, IsPositiveIntInput);
        }

        private void OnNumericPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void OnNumericPasting(object sender, DataObjectPastingEventArgs e)
        {
            CancelIfNotAllowed(sender, e, IsFloatInput);
        }

        private void OnPositiveIntPasting(object sender, DataObjectPastingEventArgs e)
        {
            CancelIfNotAllowed(sender, e, IsPositiveIntInput);
        }

        private static void CancelIfNotAllowed(object sender, DataObjectPastingEventArgs e, Func<string, bool> isValid)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.UnicodeText)
                || e.DataObject.GetData(DataFormats.UnicodeText) is not string text
                || !IsAllowedText(sender as TextBox, text, isValid))
            {
                e.CancelCommand();
            }
        }

        private static bool IsAllowedText(TextBox? box, string? incoming, Func<string, bool> isValid)
        {
            if (box == null || incoming == null)
            {
                return false;
            }

            var current = box.Text ?? string.Empty;
            var start = Math.Clamp(box.SelectionStart, 0, current.Length);
            var length = Math.Clamp(box.SelectionLength, 0, current.Length - start);
            var next = current.Remove(start, length).Insert(start, incoming);
            return isValid(next);
        }

        private static bool IsPositiveIntInput(string text)
        {
            return string.IsNullOrEmpty(text)
                || uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _);
        }

        private static bool IsFloatInput(string text)
        {
            if (string.IsNullOrEmpty(text) || text == ".")
            {
                return true;
            }

            return double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) && value >= 0
                || double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out value) && value >= 0;
        }
    }
}
