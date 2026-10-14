using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CMP.Views.Tabs.RecipeTab;

/// <summary>
/// Recipe 编辑器步骤列的单元格视觉与绑定。
/// 步骤下标（stepIndex）只有建列时可知，因此"不可操作 → 灰显"的 DataTrigger 必须按列构造，
/// 这里集中承载该逻辑，便于单独验证绑定路径。
/// </summary>
public static class RecipeStepCellStyle
{
    /// <summary>不可操作单元格的背景色。</summary>
    public static readonly Brush DisabledBackground =
        new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));

    /// <summary>不可操作单元格的前景色。</summary>
    public static readonly Brush DisabledForeground = Brushes.Gray;

    /// <summary>某种步骤列的可操作性（是否可编辑）绑定。</summary>
    public static Binding CreateIsOperableBinding(int stepIndex) =>
        new($"Steps[{stepIndex}].IsOperable");

    /// <summary>某种步骤列的不可操作原因（ToolTip）绑定。</summary>
    public static Binding CreateOperableReasonBinding(int stepIndex) =>
        new($"Steps[{stepIndex}].OperableReason");

    /// <summary>
    /// 构造步骤列单元格样式：垂直居中；当该行该步 IsOperable 为 false 时灰底灰字。
    /// </summary>
    public static Style CreateCellStyle(int stepIndex)
    {
        var style = new Style(typeof(DataGridCell));
        style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));

        var notOperableTrigger = new DataTrigger
        {
            Binding = CreateIsOperableBinding(stepIndex),
            Value = false
        };
        notOperableTrigger.Setters.Add(new Setter(Control.BackgroundProperty, DisabledBackground));
        notOperableTrigger.Setters.Add(new Setter(Control.ForegroundProperty, DisabledForeground));
        style.Triggers.Add(notOperableTrigger);

        return style;
    }
}
