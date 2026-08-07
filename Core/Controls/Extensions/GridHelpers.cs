using System.Windows;
using System.Windows.Controls;

namespace Core.Controls.Extensions
{
    public static class GridHelpers
    {
        #region RowDefinitions Attached Property

        public static readonly DependencyProperty RowDefinitionsProperty =
            DependencyProperty.RegisterAttached(
                "RowDefinitions", typeof(string), typeof(GridHelpers),
                new PropertyMetadata(null, OnRowDefinitionsChanged));

        public static string GetRowDefinitions(DependencyObject obj)
        {
            return (string)obj.GetValue(RowDefinitionsProperty);
        }

        public static void SetRowDefinitions(DependencyObject obj, string value)
        {
            obj.SetValue(RowDefinitionsProperty, value);
        }

        private static void OnRowDefinitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid) return;
            if (e.NewValue is not string rowDefs) return;

            grid.RowDefinitions.Clear();

            string[] defs = rowDefs.Split(',');
            foreach (string def in defs)
            {
                string trimDef = def.Trim();
                if (string.IsNullOrEmpty(trimDef)) continue;

                grid.RowDefinitions.Add(CreateRowDefinition(trimDef));
            }
        }

        private static RowDefinition CreateRowDefinition(string value)
        {
            var rd = new RowDefinition();
            if (value.EndsWith("*"))
            {
                double factor = 1.0;
                if (value.Length > 1)
                    double.TryParse(value.Substring(0, value.Length - 1), out factor);
                
                rd.Height = new GridLength(factor, GridUnitType.Star);
            }
            else if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                rd.Height = GridLength.Auto;
            }
            else if (double.TryParse(value, out double pixels))
            {
                rd.Height = new GridLength(pixels);
            }
            
            return rd;
        }

        #endregion

        #region ColumnDefinitions Attached Property

        public static readonly DependencyProperty ColumnDefinitionsProperty =
            DependencyProperty.RegisterAttached(
                "ColumnDefinitions", typeof(string), typeof(GridHelpers),
                new PropertyMetadata(null, OnColumnDefinitionsChanged));

        public static string GetColumnDefinitions(DependencyObject obj)
        {
            return (string)obj.GetValue(ColumnDefinitionsProperty);
        }

        public static void SetColumnDefinitions(DependencyObject obj, string value)
        {
            obj.SetValue(ColumnDefinitionsProperty, value);
        }

        private static void OnColumnDefinitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid) return;
            if (e.NewValue is not string colDefs) return;

            grid.ColumnDefinitions.Clear();

            string[] defs = colDefs.Split(',');
            foreach (string def in defs)
            {
                string trimDef = def.Trim();
                if (string.IsNullOrEmpty(trimDef)) continue;

                grid.ColumnDefinitions.Add(CreateColumnDefinition(trimDef));
            }
        }

        private static ColumnDefinition CreateColumnDefinition(string value)
        {
            var cd = new ColumnDefinition();
            if (value.EndsWith("*"))
            {
                double factor = 1.0;
                if (value.Length > 1)
                    double.TryParse(value.Substring(0, value.Length - 1), out factor);
                
                cd.Width = new GridLength(factor, GridUnitType.Star);
            }
            else if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                cd.Width = GridLength.Auto;
            }
            else if (double.TryParse(value, out double pixels))
            {
                cd.Width = new GridLength(pixels);
            }
            
            return cd;
        }

        #endregion
    }
}