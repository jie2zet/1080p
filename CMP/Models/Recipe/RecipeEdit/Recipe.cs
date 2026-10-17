using CMP.Utils;
using CMP.Views.Tabs.OperationTab;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.Xml;
using static CMP.Models.Recipe.RecipeEdit.Recipe;

namespace CMP.Models.Recipe.RecipeEdit;

public class Recipe
{
    public List<TotalRecipe> Total { get; set; } = [];
    public List<UnitRecipe> Units { get; set; } = [];

    public class UnitRecipe
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Uuid { get; set; }
        public ObservableCollection<Content?>? Contents { get; set; } = [];
    }

    public class TotalRecipe
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Uuid { get; set; }
    }

    public class Content:BindableBase
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Uuid { get; set; }

        private ObservableCollection<Data>? data;
        public ObservableCollection<Data>? Data { get => data; set => SetProperty(ref data, value); }
    }

    public class Data:BindableBase
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public string? Type { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Default { get; set; } = string.Empty;
        public List<string>? Options { get; set; }
        public List<string>? CompatibleName { get; set; }

        /// <summary>
        /// 参数可操作性规则（规格元数据，来自模板 / Excel「说明」列）。null 表示无限制。
        /// 派生于规格而非数据，不落盘。
        /// </summary>
        [JsonIgnore]
        public OperableRule? OperableRule { get; set; }

        private bool _isUsed = true;

        /// <summary>
        /// 该参数是否启用（来自模板 / Excel「不使用」列：不使用=1 → false）。不落盘。
        /// false 时界面上**不显示**该行；行数据仍保留并照常落盘与下发。
        /// </summary>
        [JsonIgnore]
        public bool IsUsed
        {
            get => _isUsed;
            set => SetProperty(ref _isUsed, value);
        }

        private ObservableCollection<Step> _steps = new ObservableCollection<Step>();
        public ObservableCollection<Step> Steps
        {
            get => _steps;
            set => SetProperty(ref _steps, value);
        }
 

    }

    public class Step:BindableBase
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _value = string.Empty;
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private bool _isOperable = true;

        /// <summary>
        /// 当前步骤上该参数是否可操作（派生态，由 RecipeOperabilityResolver 计算）。不落盘。
        /// </summary>
        [JsonIgnore]
        public bool IsOperable
        {
            get => _isOperable;
            set => SetProperty(ref _isOperable, value);
        }

        private string? _operableReason;

        /// <summary>
        /// 不可操作的原因原文（Excel「说明」列），仅 IsOperable 为 false 时有值。不落盘。
        /// </summary>
        [JsonIgnore]
        public string? OperableReason
        {
            get => _operableReason;
            set => SetProperty(ref _operableReason, value);
        }
    }
}
