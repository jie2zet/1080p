using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CMP.Extensions
{
    public static class DataGridExtensions
    {
        #region 附加属性 - 选中列

        // 定义附加属性以便跟踪选中的列
        public static readonly DependencyProperty SelectedColumnProperty =
            DependencyProperty.RegisterAttached(
                "SelectedColumn",
                typeof(DataGridColumn),
                typeof(DataGridExtensions),
                new PropertyMetadata(null));

        public static DataGridColumn GetSelectedColumn(DependencyObject obj)
        {
            return (DataGridColumn)obj.GetValue(SelectedColumnProperty);
        }

        public static void SetSelectedColumn(DependencyObject obj, DataGridColumn value)
        {
            obj.SetValue(SelectedColumnProperty, value);
        }

        #endregion

        #region 附加属性 - 启用列选择

        // 定义附加属性以启用列选择功能
        public static readonly DependencyProperty EnableColumnSelectionProperty =
            DependencyProperty.RegisterAttached(
                "EnableColumnSelection",
                typeof(bool),
                typeof(DataGridExtensions),
                new PropertyMetadata(false, OnEnableColumnSelectionChanged));

        public static bool GetEnableColumnSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableColumnSelectionProperty);
        }

        public static void SetEnableColumnSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableColumnSelectionProperty, value);
        }

        // 当启用列选择属性改变时触发
        private static void OnEnableColumnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                bool newValue = (bool)e.NewValue;

                // 如果启用列选择
                if (newValue)
                {
                    dataGrid.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
                }
                else
                {
                    dataGrid.PreviewMouseLeftButtonDown -= DataGrid_PreviewMouseLeftButtonDown;
                }
            }
        }

        // 处理 DataGrid 的鼠标点击事件
        private static void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // 获取点击的元素
            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            
            // 向上寻找列头
            DataGridColumnHeader columnHeader = FindParent<DataGridColumnHeader>(originalSource);
            
            // 如果点击的是列头
            if (columnHeader != null)
            {
                // 设置选中的列
                DataGridColumn selectedColumn = columnHeader.Column;
                SetSelectedColumn(dataGrid, selectedColumn);
                
                // 获取列索引
                int columnIndex = columnHeader.Column.DisplayIndex;
                
                // 清除之前的选择
                dataGrid.SelectedCells.Clear();
                
                // 选中当前列的所有单元格
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    var item = dataGrid.Items[i];
                    var row = dataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    
                    if (row != null)
                    {
                        // 确保行已经生成
                        row.UpdateLayout();
                        
                        var cell = GetCell(row, columnIndex);
                        if (cell != null)
                        {
                            cell.IsSelected = true;
                            //DataGridCellInfo cellInfo = new DataGridCellInfo(cell);
                            //dataGrid.SelectedCells.Add(cellInfo);
                        }
                    }
                }
                
                // 触发列选中事件
                dataGrid.RaiseEvent(new ColumnSelectedEventArgs(ColumnSelectedEvent, dataGrid, selectedColumn));
                
                // 阻止事件继续传播（防止触发排序）
                //e.Handled = true;
            }
        }

        #endregion

        #region 自定义事件 - 列选中事件

        // 定义列选中事件
        public static readonly RoutedEvent ColumnSelectedEvent = EventManager.RegisterRoutedEvent(
            "ColumnSelected",
            RoutingStrategy.Bubble,
            typeof(ColumnSelectedEventHandler),
            typeof(DataGridExtensions));

        // 事件处理器委托
        public delegate void ColumnSelectedEventHandler(object sender, ColumnSelectedEventArgs e);

        // 添加事件处理器方法
        public static void AddColumnSelectedHandler(DependencyObject d, ColumnSelectedEventHandler handler)
        {
            if (d is UIElement element)
            {
                element.AddHandler(ColumnSelectedEvent, handler);
            }
        }

        // 移除事件处理器方法
        public static void RemoveColumnSelectedHandler(DependencyObject d, ColumnSelectedEventHandler handler)
        {
            if (d is UIElement element)
            {
                element.RemoveHandler(ColumnSelectedEvent, handler);
            }
        }

        #endregion

        #region 辅助方法

        // 查找指定类型的父元素
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(child);
            
            if (parent == null)
                return null;
                
            if (parent is T typedParent)
                return typedParent;
                
            return FindParent<T>(parent);
        }
        
        // 获取指定行和列的单元格
        private static DataGridCell GetCell(DataGridRow row, int columnIndex)
        {
            if (row == null)
                return null;
                
            DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null)
                return null;
                
            // 获取指定索引的单元格
            if (presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) is DataGridCell cell)
                return cell;
                
            return null;
        }
        
        // 查找视觉树中的第一个指定类型的子元素
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                
                if (child != null && child is T)
                    return (T)child;
                    
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        #endregion
    }

    // 自定义列选中事件参数
    public class ColumnSelectedEventArgs : RoutedEventArgs
    {
        public DataGridColumn Column { get; }

        public ColumnSelectedEventArgs(RoutedEvent routedEvent, object source, DataGridColumn column)
            : base(routedEvent, source)
        {
            Column = column;
        }
    }


}