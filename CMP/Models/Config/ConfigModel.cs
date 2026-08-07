using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CMP.Models.Config
{
    public class ConfigModel:BindableBase
    {
        private int _eCID;
        private string _name = string.Empty;
        private string _unit = string.Empty;
        private string _setValue = string.Empty;
        private string _newValue = string.Empty;
        private string _defaultValue = string.Empty;
        private string _min = string.Empty;
        private string _max = string.Empty;
        private string _class = string.Empty;
        private ConfigDataType _templateType;
        private List<string> _options = [];
        private string _discription = string.Empty;
        private string _PLCLink = string.Empty;
        private string _sheetName = "Defualt";

        [SortAndIgnore(Ignore = true)]
        public int ECID 
        { get => _eCID; set => SetProperty(ref _eCID, value); }

        [SortAndIgnore(Sort = 1)]
        public string Name 
        { get => _name; set => SetProperty(ref _name, value); }

        [SortAndIgnore(Sort = 3)]
        public string Unit 
        { get => _unit; set => SetProperty(ref _unit, value); }

        [SortAndIgnore(Sort = 2)]
        public string SetValue 
        { get => _setValue; set => SetProperty(ref _setValue,value); }

        [SortAndIgnore(Ignore = true)]
        public string NewValue 
        { get => _newValue; set => SetProperty(ref _newValue, value); }

        [SortAndIgnore(Ignore = true)]
        public string DefaultValue 
        { get => _defaultValue; set => SetProperty(ref _defaultValue, value); }

        [SortAndIgnore(Sort = 5)]
        public string Min 
        { get => _min; set => SetProperty(ref _min, value); }

        [SortAndIgnore(Sort = 6)]
        public string Max 
        { get => _max; set => SetProperty(ref _max, value); }

        [SortAndIgnore(Sort = 0)]
        public string Class 
        { get => _class; set => SetProperty(ref _class, value); }

        [SortAndIgnore(Sort = 4)]
        public ConfigDataType TemplateType 
        { get => _templateType; set => SetProperty(ref _templateType, value); }

        [SortAndIgnore(Sort = 7)]
        public List<string> Option 
        { get => _options; set => SetProperty(ref _options, value); }

        [SortAndIgnore(Ignore = true)]
        public string Discription 
        { get => _discription; set => SetProperty(ref _discription, value); }

        [SortAndIgnore(Ignore = true)]
        public string PLCLink
        { get => _PLCLink; set => SetProperty(ref _PLCLink, value); }

        [SortAndIgnore(Sort = 8)]
        public string SheetName
        { get => _sheetName; set => SetProperty(ref _sheetName, value); }

        [SortAndIgnore(Ignore = true)]
        public uint Words { get; set; }
    }

    public enum ConfigDataType
    {
        Enum = 1,
        Int = 2,
        Double = 3
    }

    /// <summary>
    /// 关联SheetName，方便后续根据SheetName进行分组展示
    /// </summary>
    public class ConfigSheetModel:BindableBase
    {
        public string SheetName { get; set; } = string.Empty;
        public ObservableCollection<ConfigModel> ConfigModes { get; set; } = [];
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SortAndIgnoreAttribute : Attribute
    {
        /// <summary>
        /// The specified Property can be ignored.
        /// </summary>
        public bool Ignore { get; set; }
        /// <summary>
        /// The order of specified Property
        /// </summary>
        public int Sort { get; set; }

        public SortAndIgnoreAttribute()
        {
        }
    }

    public enum ExportWay
    { 
      All,
      Single
    }
}
