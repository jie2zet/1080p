using System.Collections.ObjectModel;

namespace CMP.Models.Recipe.RecipeEdit;

public class TreeNode:BindableBase
{
    private bool isExpanded;

    public bool IsExpanded
    {
        get { return Level == 0; }
        set { isExpanded = value; }
    }

    private string _header = string.Empty;
    public string? Header { get => _header; set => SetProperty(ref _header!, value); }
    public string? Icon { get; set; } 
    public int Level { get; set; }
    public object? Tag { get; set; }
    public ObservableCollection<TreeNode>? Children { get; set; }
}