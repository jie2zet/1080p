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
    }
}
