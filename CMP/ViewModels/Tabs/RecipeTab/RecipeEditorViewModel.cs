using CMP.Controls.Basic.InteractiveDialog;
using CMP.Extensions;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate;
using CMP.Services.Auth;
using CMP.Utils;
using CMP.Views.Tabs.RecipeTab;
using Core.Controls.Behaviors;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SqlSugar;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using WPFLocalizeExtension.Extensions;
using static CMP.Models.Recipe.RecipeEdit.Recipe;
using ComboBox = System.Windows.Controls.ComboBox;
using Dialog = HandyControl.Controls.Dialog;
using MessageBox = HandyControl.Controls.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace CMP.ViewModels.Tabs.RecipeTab;

public class RecipeEditorViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private bool newItemVisible;

    public bool NewItemVisible
    {
        get => newItemVisible;
        set => SetProperty(ref newItemVisible, value);
    }

    private bool addItemVisible;

    public bool AddItemVisible
    {
        get => addItemVisible;
        set => SetProperty(ref addItemVisible, value);
    }

    private bool saveasItemVisible;

    public bool SaveasItemVisible
    {
        get => saveasItemVisible;
        set => SetProperty(ref saveasItemVisible, value);
    }

    private bool deleteItemVisible;

    public bool DeleteItemVisible
    {
        get { return deleteItemVisible; }
        set { deleteItemVisible = value; RaisePropertyChanged(); }
    }

    private bool renameItemVisible;

    public bool RenameItemVisible
    {
        get { return renameItemVisible; }
        set { renameItemVisible = value; RaisePropertyChanged(); }
    }

    string recipeBasePath = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\";

    //private ObservableCollection<Recipe.Data>? _recipeData;
    ///// <summary>
    ///// The selected Recipe
    ///// </summary>
    //public ObservableCollection<Recipe.Data>? RecipeData
    //{
    //    get => _recipeData;
    //    set => SetProperty(ref _recipeData, value);
    //}

    private Recipe.Content? _recipeData;
    /// <summary>
    /// The selected Recipe
    /// </summary>
    public Recipe.Content? RecipeData
    {
        get => _recipeData;
        set => SetProperty(ref _recipeData, value);
    }
    private Recipe? _recipeConf;
    private readonly DataGrid _dataGrid;
    private List<string> _copiedColumnData;

    private ObservableCollection<TreeNode>? _unitRecipesList;

    public ObservableCollection<TreeNode>? UnitRecipesList
    {
        get => _unitRecipesList;
        set => SetProperty(ref _unitRecipesList, value);
    }


    public DelegateCommand? AddStepCommand { get; set; }
    public DelegateCommand? DeleteStepCommand { get; set; }
    public DelegateCommand? NewStepCommand { get; set; }
    public DelegateCommand? CopyStepCommand { get; set; }
    public DelegateCommand? PasteStepCommand { get; set; }
    public DelegateCommand? InsertStepCommand { get; set; }

    public DelegateCommand? SaveRecipeCommand { get; set; }
    public DelegateCommand<string>? TreeNewCommand { get; set; }
    public DelegateCommand? TreeAddCommand { get; set; }
    public DelegateCommand? TreeSaveAsCommand { get; set; }
    public DelegateCommand? TreeDeleteCommand { get; set; }
    public DelegateCommand? TreeRenameCommand { get; set; }
    public DelegateCommand<object>? TreeViewNodeChangedCommand { get; set; }

    public TreeNode? SelectedUnitRecipe { get; set; }

    private DataGridColumn? _selectedColumn;
    public DataGridColumn? SelectedColumn
    {
        get => _selectedColumn;
        set => SetProperty(ref _selectedColumn, value);
    }

    public DelegateCommand<ObservableCollection<DataGridColumn>>? ColumnReorderedCommand { get; set; }

    private string? _dialogResult;
    public string? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    public RecipeEditorViewModel(DataGrid mainDataGrid, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        InitCommands();
        InitRecipesData();
        _dataGrid = mainDataGrid;
        _copiedColumnData = new List<string>();       
    }

    /// <summary>
    /// Initializes the commands for the view model.
    /// </summary>
    private void InitCommands()
    {
        AddStepCommand = new DelegateCommand(OnAddStep);
        InsertStepCommand = new DelegateCommand(OnInsertStep);
        DeleteStepCommand = new DelegateCommand(OnDeleteStep);
        CopyStepCommand = new DelegateCommand(OnCopyStep);
        PasteStepCommand = new DelegateCommand(OnPasteStep);
        NewStepCommand = new DelegateCommand(OnNewStep);
        SaveRecipeCommand = new DelegateCommand(OnSaveRecipe);
        TreeNewCommand = new DelegateCommand<string>(OnTreeNew);
        TreeAddCommand = new DelegateCommand(OnTreeAdd);
        TreeSaveAsCommand = new DelegateCommand(OnTreeSaveAs);
        TreeDeleteCommand = new DelegateCommand(OnTreeDelete);
        TreeRenameCommand = new DelegateCommand(OnTreeRename);
        TreeViewNodeChangedCommand = new DelegateCommand<object>(OnTreeViewNodeChanged);
        ColumnReorderedCommand = new DelegateCommand<ObservableCollection<DataGridColumn>>(OnColumnReordered);
    }

    private void OnColumnReordered(ObservableCollection<DataGridColumn> columns)
    {
        if (columns == null) return;
        if (RecipeData == null || RecipeData!.Data!.Count == 0)
            return;
        var dataTable = RecipeData.Data;
        int frontStepIndex = 5;
        var stepColumns = columns
            .Where(c => c.DisplayIndex >= frontStepIndex) // 跳过前5个固定列
            .OrderBy(c => c.DisplayIndex)
            .ToList();

        int sourceIndex = 0;
        int columnIndex = 0;

        for (int i = 0; i < stepColumns.Count; i++)
        {
            int stepIndex = i;
            if (stepIndex < 0) break;
            
            var columnHeader = stepColumns[i].Header.ToString();// Get the column header
            if (columnHeader == null) continue;
            var columnStep = columnHeader.Replace("Step", "").Trim();
            var columnStepIndex = Convert.ToInt32(columnStep) - 1;// Get the column header Step index
            var data = dataTable[0];
            if (data.Steps[stepIndex].Id.ToString() != columnStep)
            {
                sourceIndex = data.Steps[stepIndex].Id - 1;
                columnIndex = columnStepIndex;                

                break;
            }
        }

        foreach (var data in dataTable)
        {
            int tempId = data.Steps[sourceIndex].Id;
            string tempValue = data.Steps[sourceIndex].Value;
            //交换值
            data.Steps[sourceIndex].Id = data.Steps[columnIndex].Id;
            data.Steps[sourceIndex].Value = data.Steps[columnIndex].Value;

            data.Steps[columnIndex].Id = tempId;
            data.Steps[columnIndex].Value = tempValue;
        }

        RefreshEditor();
    }
            
    /// <summary>
    /// Initializes the recipes data from the JSON configuration file.
    /// </summary>
    private void InitRecipesData()
    {
        _recipeConf = new Recipe();
        var dic = ReadAllRepiceJson();
        foreach (var item in dic)
        {
            UnitRecipe unitRecipe = new UnitRecipe();
            unitRecipe.Name = item.Key;
            unitRecipe.Contents = item.Value!;
            _recipeConf?.Units.Add(unitRecipe);
        }
        InitUnitRecipesList();
    }

    /// <summary>
    /// Initializes the UnitRecipesList with the recipe configuration.
    /// </summary>
    private void InitUnitRecipesList()
    {
        if (_recipeConf == null) return;

        UnitRecipesList = new ObservableCollection<TreeNode>();
        // add root node
        var rootNode = new TreeNode
        {
            Header = "Unit Recipes",
            Icon = "BoxArchiveSolid",
            Level = 0,
            Children = new ObservableCollection<TreeNode>()
        };
        UnitRecipesList.Add(rootNode);
        if (_recipeConf == null) return;
        foreach (var unit in _recipeConf.Units)
        {
            var unitNode = new TreeNode
            {
                Header = $"{unit.Name}",
                Icon = "FolderClosedSolid",
                Level = 1,
                Children = new ObservableCollection<TreeNode>(),
                Tag = unit.Uuid
            };

            if (unit.Contents != null)
            {
                foreach (var content in unit.Contents)
                {
                    var contentNode = new TreeNode
                    {
                        Header = content?.Name,
                        Icon = "FileLinesSolid",
                        Level = 2,
                        Tag = content?.Uuid
                    };

                    unitNode.Children.Add(contentNode);
                }
            }

            UnitRecipesList[0].Children?.Add(unitNode);
        }
    }

    /// <summary>
    /// Handles the addition of a new step to the recipe data.
    /// </summary>
    private void OnAddStep()
    {
        // Add a new step to the column
        if (RecipeData == null || RecipeData!.Data!.Count == 0)
            return;
        foreach (var data in RecipeData.Data)
        {
            var newStep = new Recipe.Step
            {
                Id = data.Steps.Count + 1,
                Value = data.Default
            };
            if (data.Type == "option" || data.Type == "list")
            {                
                if (data.Options == null || data.Options.Count == 0) 
                {
                    newStep.Value = string.Empty;
                    data.Steps.Add(newStep);
                    continue;
                }
                try
                {
                    var match = data.Options.Where(o => o.StartsWith(data.Default)).First();
                    newStep.Value = match;
                }
                catch (Exception)
                {
                    newStep.Value = data.Options[0];
                }
            }
            data.Steps.Add(newStep);
        }

        // Refresh the editor to show the new step
        RefreshEditor();
    }

    /// <summary>
    /// Handles the insertion of a new step at the selected column index.
    /// </summary>
    private void OnInsertStep()
    {
        //if (RecipeData == null || RecipeData!.Data!.Count == 0)
        //    return;
        //int frontIndex = 5;
        //foreach (var data in RecipeData.Data)
        //{
        //    var newStep = new Recipe.Step
        //    {
        //        Id = data.Steps.Count + 1,
        //        Value = string.Empty
        //    };
        //    if (SelectedColumn != null) data.Steps.Insert(SelectedColumn.DisplayIndex - frontIndex, newStep);
        //}

        //ReFillStepData();
    }

    /// <summary>
    /// Handles the deletion of the selected step from the recipe data.
    /// </summary>
    private void OnDeleteStep()
    {
        // delete the selected step
        if (RecipeData == null || RecipeData!.Data!.Count == 0)
            return;
        int frontStepIndex = 5;

        if (SelectedColumn != null)
        {
            foreach (var data in RecipeData.Data)
            {
                int beDeleteIndex = SelectedColumn.DisplayIndex - frontStepIndex;
                if (beDeleteIndex < 0 || beDeleteIndex >= data.Steps.Count)
                {
                    break;
                }
                data.Steps.RemoveAt(beDeleteIndex);
            }
        }
        ReFillStepData();
    }

    /// <summary>
    /// Handles the copying of the selected step data to the clipboard.
    /// </summary>
    private void OnCopyStep()
    {
        // Copy the entire column of data
        if (RecipeData == null || RecipeData!.Data!.Count == 0)
            return;
        int frontStepIndex = 5;
        if (SelectedColumn != null)
        {
            int columnIndex = SelectedColumn.DisplayIndex - frontStepIndex;
            if (columnIndex < 0) return;

            // Clear previous copied data
            _copiedColumnData = new List<string>();

            // Store all values from this column
            foreach (var data in RecipeData.Data)
            {
                if (columnIndex < data.Steps.Count)
                {
                    _copiedColumnData.Add(data.Steps[columnIndex].Value);
                }
                else
                {
                    break;
                }
            }
        }

        // Also put the first value in the clipboard for legacy support
        //if (_copiedColumnData.Count > 0)
        //{
        //    Clipboard.SetText(_copiedColumnData[0]);
        //}
    }

    /// <summary>
    /// Handles the pasting of copied step data into the recipe data.
    /// </summary>
    private void OnPasteStep()
    {
        if (RecipeData == null || RecipeData!.Data!.Count == 0 || _copiedColumnData.Count == 0)
            return;
        int frontStepIndex = 5;
        if (SelectedColumn != null)
        {
            int columnIndex = SelectedColumn.DisplayIndex - frontStepIndex;
            if (columnIndex < 0) return;
            if (RecipeData!.Data!.Count != _copiedColumnData.Count) return;

            for (int i = 0; i < RecipeData!.Data!.Count; i++)
            {
                var data = RecipeData.Data[i];                
                var newStep = new Recipe.Step
                {
                    Id = 0,
                    Value = _copiedColumnData[i]
                };

                if (columnIndex < data.Steps.Count - 1)
                {
                    data.Steps.Insert(columnIndex, newStep);
                }
                else
                {
                    data.Steps.Add(newStep);
                }
            }
        }

        // Refill step IDs and refresh
        ReFillStepData();
        _copiedColumnData.Clear();
        // Indicate success
        Growl.SuccessGlobal(new GrowlInfo
        {
            Message = Lang.Resources.PasteStepSuccessInfo,
            ShowCloseButton = true,
        });
    }

    /// <summary>
    /// Handles the creation of a new step with the copied data.
    /// </summary>
    private void OnNewStep()
    {
        // Add a new column at the end with the copied data
        if (RecipeData == null || RecipeData!.Data!.Count == 0 || _copiedColumnData.Count == 0)
            return;

        // Add a new step to each recipe data item at the end
        for (int i = 0; i < RecipeData!.Data!.Count; i++)
        {
            var data = RecipeData.Data[i];
            var newStep = new Recipe.Step
            {
                Id = data.Steps.Count + 1,
                Value = i < _copiedColumnData.Count ? _copiedColumnData[i] : string.Empty
            };
            data.Steps.Add(newStep);
        }

        // Refresh the editor to show the new step
        RefreshEditor();

        // Indicate the operation was successful
        Growl.SuccessGlobal(new GrowlInfo
        {
            Message = Lang.Resources.NewStepSuccessInfo,
            ShowCloseButton = true,
        });
    }

    /// <summary>
    /// Handles the saving of the recipe data to the JSON configuration file.
    /// </summary>
    private void OnSaveRecipe()
    {
        // save the recipe
        if (_recipeConf == null) return;

        string? savedRecipeType = null;
        string? savedRecipeName = null;

        var uuid = SelectedUnitRecipe?.Tag?.ToString();
        if (!string.IsNullOrEmpty(uuid))
        {
            foreach (var unit in _recipeConf.Units)
            {
                if (unit.Contents == null) continue;

                foreach (var content in unit.Contents)
                {
                    if (content?.Uuid != uuid) continue;

                    content.Data = RecipeData!.Data;
                    savedRecipeType = unit.Name;
                    savedRecipeName = content.Name;
                    break;
                }
            }
        }

        SaveUnitRecipes();

        // 发布Recipe保存事件，通知其他组件
        if (!string.IsNullOrEmpty(savedRecipeType) && !string.IsNullOrEmpty(savedRecipeName))
        {
            _eventAggregator.GetEvent<RecipeSavedEvent>().Publish(new RecipeSavedEventArgs
            {
                RecipeType = savedRecipeType,
                RecipeName = savedRecipeName,
                SavedTime = DateTime.Now
            });
        }

        // Indicate success
        Growl.SuccessGlobal(new GrowlInfo
        {
            Message = Lang.Resources.SaveRecipeSuccessInfo,
            ShowCloseButton = true,
        });

        if (!string.IsNullOrEmpty(savedRecipeType) && !string.IsNullOrEmpty(savedRecipeName))
        {
            AuditLogService.WriteCurrentUser(
                AuditActions.RecipeSave,
                "Success",
                $"{savedRecipeType}/{savedRecipeName}");
        }
    }

    private void OnTreeViewNodeChanged(object obj)
    {
        if (obj == null)
        {
            return;
        }
        if (obj is not TreeNode node)
        {
            return;
        }
        SelectedUnitRecipe = node;
        NewItemVisible = node.Level == 0;
        AddItemVisible = node.Level == 1;
        SaveasItemVisible = node.Level == 2;
        DeleteItemVisible = node.Level == 2;
        RenameItemVisible = node.Level == 2;
        this.DialogResult = SelectedUnitRecipe.Header;
        var uuid = SelectedUnitRecipe?.Tag?.ToString();
        if (SelectedUnitRecipe?.Level != 2)
        {
            return;
        }
        if (string.IsNullOrEmpty(uuid) || _recipeConf == null)
        {
            return;
        }

        foreach (var unit in _recipeConf.Units)
        {
            if (unit.Contents == null)
            {
                continue;
            }
            foreach (var content in unit.Contents)
            {
                if (content?.Uuid != uuid) continue;
                RecipeData = new Recipe.Content();
                string jsonFile = Path.Combine(recipeBasePath, unit!.Name!, $"{content.Name}.json");
                var json = FileUtil.ReadFile(jsonFile);
                var tempContent = JsonConvert.DeserializeObject<Recipe.Content>(json)!;
                if (tempContent.Data == null) tempContent.Data = [];
                RecipeData.Id = tempContent.Id;
                RecipeData.Uuid = tempContent.Uuid;
                RecipeData.Name = tempContent.Name;
                RecipeData.Data = new ObservableCollection<Data>(tempContent.Data!);

                foreach (var data in RecipeData.Data)
                {
                    if (data.Name == "Algorithm")
                    {
                        data.Options = GetEPDAlgorithm();
                    }
                    else if (data.Name == "Model")
                    {
                        data.Options = GetEPDModel();
                    }

                    var folderName = data.Name;
                    if (folderName == "Head Sweep Recipe"
                        || folderName == "Condition Recipe"
                        || folderName == "Condition Sweep Recipe")
                    {
                        string folderDirectory = Path.Combine(recipeBasePath, folderName);
                        data.Options = GetRecipeFileNames(folderDirectory);
                        foreach (var item in data.Steps)
                        {
                            var arr = item.Value.Split(":");
                            if (arr.Length > 1)
                            {
                                item.Value = arr[1];
                            }
                        }
                    }
                }

                RefreshEditor();
                return;
            }
        }
    }

    private List<string?> GetEPDAlgorithm()
    {
        string folderPath = "D:\\EPD\\File\\Algorithm";
        List<string?> jsonFileNames = ["None"];
        if (!Directory.Exists(folderPath))
        {
            return jsonFileNames;
        }
        jsonFileNames = [.. Directory.EnumerateFiles(folderPath, "*.json").Select(Path.GetFileNameWithoutExtension)];
        jsonFileNames.Insert(0, "None");
        return jsonFileNames;
    }

    private List<string?> GetEPDModel()
    {
        string folderPath = "D:\\EPD\\File\\Model";
        List<string?> jsonFileNames = ["None"];
        if (!Directory.Exists(folderPath))
        {
            return jsonFileNames;
        }
        jsonFileNames = Directory.EnumerateFiles(folderPath, "*.json")
        .Select(Path.GetFileNameWithoutExtension)
        .ToList();
        jsonFileNames.Insert(0, "None");
        return jsonFileNames;
    }

    /// <summary>
    /// 获取指定的Recipe文件夹下的所有.json文件名称（不带扩展名），用于在下拉框中显示可选项
    /// </summary>
    /// <returns></returns>
    private static List<string> GetRecipeFileNames(string FullFolderName)
    {
        if (string.IsNullOrEmpty(FullFolderName)) return [];
        if (!Directory.Exists(FullFolderName)) return [];
        var jsonFileNames = Directory.EnumerateFiles(FullFolderName, "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .ToList();
        if (jsonFileNames == null || jsonFileNames.Count == 0) return ["None"];
        List<string> options = ["None"];
        for (int i = 0; i < jsonFileNames.Count; i++)
        {
            options.Add($"{jsonFileNames[i]}");
        }
        return options;
    }

    private async void OnTreeNew(string type)
    {
        var result = await Dialog.Show<InteractiveDialog>().Initialize<InteractiveDialogViewModel>(vm => { vm.Message = DialogResult; }).GetResultAsync<string>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            if (result == null) return;
            if (string.IsNullOrEmpty(result)) return;
            if (result == "Cancel") return;
            // Create a new unit
            var newUnit = new Recipe.UnitRecipe
            {
                Name = result,
                Type = type,
                Uuid = Guid.NewGuid().ToString(),
                Contents = []
            };
            string UnitRecipePath = recipeBasePath + newUnit.Name;
            if (!Directory.Exists(UnitRecipePath)) { Directory.CreateDirectory(UnitRecipePath); }

            // Add the new unit to the list
            _recipeConf?.Units.Add(newUnit);

            // Create a new tree node for the unit
            var unitNode = new TreeNode
            {
                Header = $"{newUnit.Name}",
                Icon = "FolderClosedSolid",
                Level = 1,
                Tag = newUnit.Uuid,
                Children = new ObservableCollection<TreeNode>()
            };
            UnitRecipesList?[0].Children?.Add(unitNode);
            AuditLogService.WriteCurrentUser(AuditActions.RecipeCreate, "Success", $"Unit:{newUnit.Name}");
        });
    }

    private async void OnTreeAdd()
    {
        var result = await Dialog.Show<AddRecipeEditorView>().Initialize<AddRecipeEditorViewModel>(vm => { vm.Message = DialogResult ?? string.Empty; }).GetResultAsync<Tuple<RecipeModelTemplate, string>>();
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (result == null) return;
            if (result.Item1 == RecipeModelTemplate.None) return;
            if (string.IsNullOrEmpty(result.Item2)) return;
            if (SelectedUnitRecipe == null) return;
            if (_recipeConf == null) return;
            string jsonFilePath = recipeBasePath + SelectedUnitRecipe.Header + "\\" + result.Item2 + ".json";
            if (File.Exists(jsonFilePath))
            {
                Growl.Error("The file already exists. Re-creation is prohibited!");
                return;
            }
            // Create a new content
            var newContent = new Recipe.Content
            {
                Name = result.Item2,
                Uuid = Guid.NewGuid().ToString(),
                Data = []
            };

            switch (result.Item1)
            {
                case RecipeModelTemplate.ConditionRecipeTemplate:
                    newContent.Data = ConditionRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.DechuckRecipeTemplate:
                    newContent.Data = DechuckRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.DryRecipeTemplate:
                    newContent.Data = DryRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.ExsituContidionSweepRecipeTemplate:
                    newContent.Data = ExsituContidionSweepRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.HeadSweepRecipeTemplate:
                    newContent.Data = HeadSweepRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.LoadRecipeTemplate:
                    newContent.Data = LoadRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.PenA1RecipeTemplate:
                    newContent.Data = PenA1RecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.PolishRecipeTemplate:
                    newContent.Data = PolishRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.RollRecipeTemplate:
                    newContent.Data = RollRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.UnloadRecipeTemplate:
                    newContent.Data = UnloadRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.LoadRetryRecipeTemplate:
                    newContent.Data = LoadRetryRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.DechuckRetryRecipeTemplate:
                    newContent.Data = DechuckRetryRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.UnloadRetryRecipeTemplate:
                    newContent.Data = UnloadRetryRecipeTemplate.Template;
                    break;
                case RecipeModelTemplate.None:
                    return;
                default:
                    return;
            }

            foreach (var unit in _recipeConf.Units)
            {
                if (unit.Name != SelectedUnitRecipe.Header) continue;
                var ContentNoData = new Recipe.Content() { Id = newContent.Id, Name = newContent.Name, Uuid = newContent.Uuid };
                unit.Contents?.Add(ContentNoData);
                break;
            }

            var contentNode = new TreeNode
            {
                Header = newContent.Name,
                Icon = "FileLinesSolid",
                Level = 2,
                Tag = newContent.Uuid
            };
            SelectedUnitRecipe.Children?.Add(contentNode);
            string jsonFile = Path.Combine(recipeBasePath, SelectedUnitRecipe.Header!, $"{newContent.Name}.json");
            var json = JsonConvert.SerializeObject(newContent, Formatting.Indented);
            FileUtil.WriteFile(jsonFile, json);

            // 发布Recipe新增事件，通知其他组件
            _eventAggregator.GetEvent<RecipeSavedEvent>().Publish(new RecipeSavedEventArgs
            {
                RecipeType = SelectedUnitRecipe.Header?.ToString(),
                RecipeName = newContent.Name,
                SavedTime = DateTime.Now
            });

            Growl.SuccessGlobal(new GrowlInfo
            {
                Message = Lang.Resources.AddSuccessfully,
                ShowCloseButton = true,
            });
            AuditLogService.WriteCurrentUser(
                AuditActions.RecipeCreate,
                "Success",
                $"{SelectedUnitRecipe.Header}/{newContent.Name}");

        });
    }

    private async void OnTreeSaveAs()
    {
        var result = await Dialog.Show<InteractiveDialog>().Initialize<InteractiveDialogViewModel>(vm => { vm.Message = DialogResult; }).GetResultAsync<string>();
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (result == null) return;
            if (string.IsNullOrEmpty(result)) return;
            if (result == "Cancel") return;

            if (_recipeConf == null) return;
            if (SelectedUnitRecipe == null) return;
            // Create a new content
            var newContent = new Recipe.Content
            {
                Name = result,
                Uuid = Guid.NewGuid().ToString(),
                Data = []
            };
            bool isbreak = false;
            string folder = "";
            foreach (var unit in _recipeConf.Units)
            {
                if (isbreak) break;
                foreach (var item in unit.Contents!)
                {
                    if (item?.Uuid != SelectedUnitRecipe?.Tag?.ToString()) continue;
                    folder = unit.Name!;
                    string sourceJsonFile = recipeBasePath + folder + "\\" + item?.Name + ".json";
                    //newContent.Data = item?.Data;
                    var jsonStr = FileUtil.ReadFile(sourceJsonFile);
                    Recipe.Content temp = JsonConvert.DeserializeObject<Recipe.Content>(jsonStr)!;
                    newContent.Data = temp?.Data;

                    Recipe.Content tempContent = new Recipe.Content() { Id = newContent.Id, Name = newContent.Name, Uuid = newContent.Uuid };
                    unit.Contents.Add(tempContent);
                    isbreak = true;
                    break;
                }
            }

            TreeNode? parentTreeNode = null;
            foreach (var item in UnitRecipesList![0].Children!)
            {
                foreach (var node in item.Children!)
                {
                    if (node.Header == SelectedUnitRecipe?.Header && node.Tag == SelectedUnitRecipe?.Tag)
                    {
                        parentTreeNode = item;
                        break;
                    }
                }
            }
            if (parentTreeNode == null) return;
            // Add the new content to the 1 level node
            var contentNode = new TreeNode
            {
                Header = newContent.Name,
                Icon = "FileLinesSolid",
                Level = 2,
                Tag = newContent.Uuid
            };
            parentTreeNode.Children?.Add(contentNode);
            string jsonFilePath = recipeBasePath + folder + "\\" + newContent.Name + ".json";
            if (File.Exists(jsonFilePath))
            {
                Growl.Error("The file already exists. Re-creation is prohibited!");
                return;
            }
            else
            {
                FileUtil.WriteFile(jsonFilePath, JsonConvert.SerializeObject(newContent, Formatting.Indented));
            }

            // 发布Recipe另存为事件，通知其他组件
            _eventAggregator.GetEvent<RecipeSavedEvent>().Publish(new RecipeSavedEventArgs
            {
                RecipeType = parentTreeNode.Header?.ToString(),
                RecipeName = newContent.Name,
                SavedTime = DateTime.Now
            });

            Growl.SuccessGlobal(new GrowlInfo
            {
                Message = Lang.Resources.AddSuccessfully,
                ShowCloseButton = true,
            });
            AuditLogService.WriteCurrentUser(
                AuditActions.RecipeSaveAs,
                "Success",
                $"{parentTreeNode.Header}/{newContent.Name}");
        });
    }

    private void OnTreeDelete()
    {
        // ask for confirmation
        var result = MessageBox.Show(Lang.Resources.DeleteConfirm, Lang.Resources.Delete, MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        if (_recipeConf == null) return;
        if (SelectedUnitRecipe == null || UnitRecipesList == null) return;

        string? deletedRecipeType = null;
        string? deletedRecipeName = null;

        if (SelectedUnitRecipe.Level == 1)
        {
            deletedRecipeType = SelectedUnitRecipe.Header?.ToString();

            foreach (var item in _recipeConf.Units)
            {
                if (item.Name == SelectedUnitRecipe.Header)
                {
                    _recipeConf.Units.Remove(item);
                    break;
                }
            }
            string unitRecipePath = recipeBasePath + SelectedUnitRecipe.Header;
            if (Directory.Exists(unitRecipePath))
            {
                Directory.Delete(unitRecipePath, true);
            }
            for (int i = 0; i < UnitRecipesList[0]?.Children?.Count; i++)
            {
                if (UnitRecipesList[0].Children == null) break;
                var item = UnitRecipesList[0]?.Children?[i];
                if (item == null) continue;
                if (item.Header == SelectedUnitRecipe.Header)
                {
                    UnitRecipesList[0]?.Children?[i].Children?.Clear();
                    UnitRecipesList[0].Children?.RemoveAt(i);
                    break;
                }
            }

        }
        if (SelectedUnitRecipe.Level == 2)
        {
            deletedRecipeName = SelectedUnitRecipe.Header?.ToString();

            // Remove the selected unit node from the list
            bool isBreak = false;
            foreach (var unit in _recipeConf.Units)
            {
                if (isBreak)
                {
                    break;
                }
                if (unit == null) continue;
                if (unit.Contents == null) continue;
                string folderName = unit.Name!;
                foreach (var content in unit.Contents)
                {
                    if (content == null) continue;
                    if (content.Uuid == SelectedUnitRecipe.Tag?.ToString())
                    {
                        deletedRecipeType = folderName;  // 记录Recipe类型
                        unit.Contents.Remove(content);
                        isBreak = true;
                        string recipeFilePath = recipeBasePath + folderName + "\\" + content.Name + ".json";
                        File.Delete(recipeFilePath);
                        break;
                    }
                }
            }
            // Remove the selected unit node from the tree
            isBreak = false;
            if (UnitRecipesList[0].Children != null)
            {
                foreach (var unit in UnitRecipesList[0].Children!)
                {
                    if (isBreak)
                    {
                        break;
                    }
                    if (unit.Children == null) continue;
                    foreach (var item in unit.Children)
                    {
                        if (item.Header != SelectedUnitRecipe.Header) continue;
                        unit.Children?.Remove(item);
                        isBreak = true;
                        break;
                    }
                }
            }

        }
        else
        {
            return;
        }

        // 发布Recipe删除事件，通知其他组件重新加载列表
        if (!string.IsNullOrEmpty(deletedRecipeType))
        {
            _eventAggregator.GetEvent<RecipeSavedEvent>().Publish(new RecipeSavedEventArgs
            {
                RecipeType = deletedRecipeType,
                RecipeName = deletedRecipeName ?? string.Empty,
                SavedTime = DateTime.Now
            });

            AuditLogService.WriteCurrentUser(
                AuditActions.RecipeDelete,
                "Success",
                string.IsNullOrEmpty(deletedRecipeName)
                    ? deletedRecipeType
                    : $"{deletedRecipeType}/{deletedRecipeName}");
        }
    }

    private async void OnTreeRename()
    {
        if (SelectedUnitRecipe == null) return;
        if (_recipeConf == null) return;
        TreeNode treeNode = SelectedUnitRecipe;
        var result = await Dialog.Show<InteractiveDialog>().Initialize<InteractiveDialogViewModel>(vm => { vm.Message = DialogResult; }).GetResultAsync<string>();
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Save the recipe
            if (result == null) return;
            if (string.IsNullOrEmpty(result)) return;
            if (result == "Cancel") return;

            string? oldRecipeType = null;
            string? newRecipeType = null;
            string? oldRecipeName = null;
            string? newRecipeName = null;

            switch (treeNode.Level)
            {
                case 1:
                    oldRecipeType = treeNode.Header?.ToString();
                    newRecipeType = result;

                    foreach (var unit in _recipeConf.Units)
                    {
                        if (unit.Name != treeNode.Header) continue;
                        string sourceDirectory = recipeBasePath + unit.Name;
                        string destDirectory = recipeBasePath + result;
                        if (Directory.Exists(destDirectory))
                        {
                            Growl.Error(Lang.Resources.FolderRenameFailedTip);
                            return;
                        }
                        Directory.Move(sourceDirectory, destDirectory);
                        unit.Name = result;
                        break;
                    }

                    for (int i = 0; i < UnitRecipesList?[0].Children?.Count; i++)
                    {
                        var item = UnitRecipesList?[0].Children?[i];
                        if (item == null) continue;
                        if (item.Header != treeNode.Header) continue;
                        item.Header = result;
                    }
                    treeNode.Header = result;

                    // 发布重命名事件，通知其他组件（文件夹级别）
                    if (!string.IsNullOrEmpty(oldRecipeType))
                    {
                        _eventAggregator.GetEvent<RecipeSavedEvent>().Publish(new RecipeSavedEventArgs
                        {
                            RecipeType = oldRecipeType,
                            RecipeName = string.Empty,
                            SavedTime = DateTime.Now
                        });

                        AuditLogService.WriteCurrentUser(
                            AuditActions.RecipeRename,
                            "Success",
                            $"{oldRecipeType}->{newRecipeType}");
                    }
                    break;
                case 2:
                    oldRecipeName = treeNode.Header?.ToString();
                    newRecipeName = result;

                    bool isBreak = false;
                    foreach (var unit in _recipeConf.Units)
                    {
                        if (isBreak) break;
                        foreach (var content in unit.Contents!)
                        {
                            if (content == null) continue;
                            if (content.Uuid != treeNode.Tag?.ToString()) continue;
                            oldRecipeType = unit.Name;  // 记录Recipe类型
                            string sourceFile = recipeBasePath + unit.Name + "\\" + content.Name + ".json";
                            string destFile = recipeBasePath + unit.Name + "\\" + result + ".json";
                            if (File.Exists(destFile))
                            {
                                Growl.Error(Lang.Resources.FileRenameFailedTip);
                                return;
                            }
                            File.Move(sourceFile, destFile);
                            content.Name = result;
                            RecipeData!.Name = result;
                            FileUtil.WriteFile(destFile, JsonConvert.SerializeObject(RecipeData, Formatting.Indented));
                            isBreak = true;
                            break;
                        }
                    }
                    isBreak = false;
                    foreach (var item in UnitRecipesList?[0].Children!)
                    {
                        if (isBreak) break;
                        if (item == null) continue;
                        foreach (var file in item.Children!)
                        {
                            if (file == null) continue;
                            if (file.Header != treeNode.Header) continue;
                            file.Header = result;
                            isBreak = true;
                        }
                    }

                    treeNode.Header = result;

                    // 发布重命名事件，通知其他组件（Recipe文件级别）
                    if (!string.IsNullOrEmpty(oldRecipeType))
                    {
                        _eventAggregator.GetEvent<RecipeSavedEvent>().Publish(new RecipeSavedEventArgs
                        {
                            RecipeType = oldRecipeType,
                            RecipeName = newRecipeName ?? string.Empty,
                            SavedTime = DateTime.Now
                        });

                        AuditLogService.WriteCurrentUser(
                            AuditActions.RecipeRename,
                            "Success",
                            $"{oldRecipeType}/{oldRecipeName}->{newRecipeName}");
                    }
                    break;
                default:
                    break;
            }
        });
    }

    private void ReFillStepData()
    {
        // data will be shifted when inserting a new step
        if (RecipeData == null || RecipeData!.Data!.Count == 0)
            return;
        foreach (var data in RecipeData!.Data!)
        {
            for (int i = 0; i < data.Steps.Count; i++)
            {
                data.Steps[i].Id = i + 1;
            }
        }

        // Refresh the editor to show the new step
        RefreshEditor();
    }

    /// <summary>
    /// Refreshes the editor by updating the DataGrid columns based on the current recipe data.
    /// </summary>
    public void RefreshEditor()
    {
        if (RecipeData == null || RecipeData!.Data!.Count == 0)
            return;

        var steps = RecipeData!.Data![0].Steps;

        // Remove columns after the 4 th one (the fixed columns)
        while (_dataGrid.Columns.Count > 5)
        {
            _dataGrid.Columns.RemoveAt(5);
        }

        for (int stepIndex = 0; stepIndex < steps.Count; stepIndex++)
        {
            var step = steps[stepIndex];
            // Create a template column for each step
            var templateColumn = new DataGridTemplateColumn
            {
                Header = $"Step {step.Id}",
                CellTemplateSelector = new RecipeCellTemplateSelector(stepIndex),
                CellEditingTemplateSelector = new RecipeCellTemplateSelector(stepIndex),
                CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters =
                    {
                       new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center)
                    }
                }
            };
            _dataGrid.Columns.Add(templateColumn);
        }
    }

    public Dictionary<string, ObservableCollection<Recipe.Content>> ReadAllRepiceJson()
    {
        Dictionary<string, ObservableCollection<Recipe.Content>> valuePairs = [];
        DirectoryInfo directory = Directory.CreateDirectory(recipeBasePath);
        var directors = directory.GetDirectories();
        foreach (var folder in directors)
        {
            ObservableCollection<Recipe.Content> contents = [];
            var jsonFiles = folder.GetFiles();
            foreach (var file in jsonFiles)
            {
                if (file.Extension.ToLower() == ".json")
                {
                    var json = FileUtil.ReadFile(file.FullName);
                    Recipe.Content? content = JsonConvert.DeserializeObject<Recipe.Content>(json)!;
                    if (content == null) continue;
                    if (content.Name != Path.GetFileNameWithoutExtension(file.Name)) content.Name = Path.GetFileNameWithoutExtension(file.Name);
                    Recipe.Content? content1 = new () { Id = content.Id, Name = content.Name, Uuid = content.Uuid };
                    contents.Add(content1);
                }
            }
            ObservableCollection<Recipe.Content> contents2 = new(contents);
            valuePairs.Add(folder.Name, contents2);
        }
        return valuePairs;
    }

    /// <summary>
    /// Save UnitRecipe
    /// </summary>
    private void SaveUnitRecipes()
    {
        if (_recipeConf == null) return;
        CheckRecipeDefualtValue();
        string jsonFile = "";
        Recipe.Content jsonRecipe = new Recipe.Content();
        bool isBreak = false;
        foreach (var Folder in _recipeConf.Units)
        {
            if (isBreak) break;
            string unitFolder = recipeBasePath + Folder.Name + "\\";
            if (!Directory.Exists(unitFolder)) Directory.CreateDirectory(unitFolder);
            foreach (var content in Folder.Contents!)
            {
                if (content!.Name == SelectedUnitRecipe!.Header && content!.Uuid == SelectedUnitRecipe.Tag?.ToString())
                {
                    jsonRecipe = RecipeData!;
                    jsonFile = unitFolder + jsonRecipe?.Name + ".json";
                    isBreak = true;
                    break;
                }
            }
        }
        if (SelectedUnitRecipe!.Level == 2)
        {
            var json = JsonConvert.SerializeObject(jsonRecipe, Formatting.Indented);
            FileUtil.WriteFile(jsonFile, json);
        }
    }

    private void CheckRecipeDefualtValue()
    {
        if (_recipeConf == null) return;
        if (RecipeData == null) return;
        if (RecipeData.Data == null) return;

        foreach (var data in RecipeData.Data!)
        {
            if (data.Type?.ToLower() == "option" || data.Type?.ToLower() == "list")
            {
                if (!data.Options!.Contains(data.Default))
                {
                    data.Default = data.Options![0];
                }
            }
            if (data.Type?.ToLower() == "int")
            {
                if (string.IsNullOrEmpty(data.Default) || data.Default == "")
                {
                    data.Default = "0";
                }
            }
            if (data.Type?.ToLower() == "float")
            {
                if (string.IsNullOrEmpty(data.Default) || data.Default == "")
                {
                    data.Default = "0.00";
                }
            }
            foreach (var step in data.Steps)
            {
                if (data.Type?.ToLower() == "option" || data.Type?.ToLower() == "list")
                {
                    if (!data.Options!.Contains(step.Value))
                    {
                        step.Value = data.Default;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(step.Value) || step.Value.Trim() == "")
                    {
                        step.Value = data.Default;
                    }
                }
            }
        }
    }

}

public class RecipeCellTemplateSelector : DataTemplateSelector
{
    private readonly int _stepIndex;

    public RecipeCellTemplateSelector(int stepIndex)
    {
        _stepIndex = stepIndex;
    }

    /// <summary>
    /// Selects the appropriate template based on the item and container.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        var data = item as Recipe.Data;
        if (data == null) return null;

        var element = container as FrameworkElement;
        if (element == null) return null;

        // Make sure we have enough steps
        if (_stepIndex >= data.Steps.Count || _stepIndex < 0) return null;

        // Select template based on data type
        if (data.Type?.ToLower() == "option" || data.Type?.ToLower() == "list")
        {
            // Create and return a ComboBox template
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(ComboBox));
            factory.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Options"));
            factory.SetBinding(Selector.SelectedItemProperty,
                new Binding($"Steps[{_stepIndex}].Value")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            factory.SetBinding(Control.BackgroundProperty,
            new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridCell), 1)
            });
            template.VisualTree = factory;
            // default value
            // TODO Default Value not right.
            //factory.SetValue(Selector.SelectedValueProperty, data.Options![string.IsNullOrEmpty(data.Default) ? 0 : Convert.ToInt32(data.Default)]);
            return template;
        }
        else if (data.Type?.ToLower() == "int" || data.Type?.ToLower() == "float")
        {
            // Create and return a NumericUpDown template
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(NumericUpDown));
            // 超出 [Min, Max] 的非 0 输入，1 秒后自动恢复为 Min/Max
            factory.AddHandler(
                NumericUpDown.ValueChangedEvent,
                new EventHandler<FunctionEventArgs<double>>(OnNumericUpDownValueChanged));
            factory.AddHandler(
                TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(OnNumericTextChanged));
            // 创建绑定，添加验证规则
            var valueBinding = new Binding($"Steps[{_stepIndex}].Value")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            // 添加自定义验证规则
            var rule = new ZeroOrRangeValidationRule { Min = Convert.ToDouble(data.Min), Max = Convert.ToDouble(data.Max) };
            if (data.Min == data.Max) rule = new ZeroOrRangeValidationRule { Min = Convert.ToDouble(-10000.00), Max = Convert.ToDouble(10000.00) };
            valueBinding.ValidationRules.Add(rule);

            factory.SetBinding(NumericUpDown.ValueProperty, valueBinding);
            //factory.SetBinding(NumericUpDown.ValueProperty,
            //    new Binding($"Steps[{_stepIndex}].Value")
            //    {
            //        Mode = BindingMode.TwoWay,
            //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //    });
            factory.SetBinding(Control.BackgroundProperty,
            new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridCell), 1)
            });
            template.VisualTree = factory;
            // int without decimal
            factory.SetValue(NumericUpDown.DecimalPlacesProperty, data.Type?.ToLower() == "int" ? 0 : 2);
            // 交由 1 秒自动恢复逻辑纠正为 Min（如 Min=50 时输入 -5 → 自动恢复为 50）
            factory.SetValue(NumericUpDown.MinimumProperty, -10000.00);
            factory.SetValue(NumericUpDown.MaximumProperty, 10000.00);
            return template;
        }
        else
        {
            // Create and return a TextBox template
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(TextBox));
            factory.SetBinding(TextBox.TextProperty,
                new Binding($"Steps[{_stepIndex}].Value")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            factory.SetBinding(Control.BackgroundProperty,
            new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridCell), 1)
            });
            template.VisualTree = factory;
            
            return template;
        }
    }

    /// <summary>
    /// 数值参数（int/float）输入超出 [Min, Max] 范围（0 豁免）时，
    /// 启动/重置 1 秒防抖计时器；1 秒后值仍未回到范围内，则自动恢复为 Min 或 Max。
    /// 通过 ValueChanged（微调按钮/滚轮/方向键等）与 TextChanged（键盘输入，编辑中）两条路径共同检测。
    /// </summary>
    private static void OnNumericUpDownValueChanged(object? sender, FunctionEventArgs<double> e)
    {
        if (sender is not NumericUpDown nud) return;
        if (nud.DataContext is not Recipe.Data data) return;
        if (data.Type is not ("int" or "float")) return;

        double value = nud.Value;
        if (data.Min == data.Max) return;  //兼容老的json配置min和max都为0的情况
        if (value == 0) return;                                   // 0 为豁免值
        if (value >= data.Min && value <= data.Max) return;      // 在范围内

        ScheduleClamp(nud);
    }

    /// <summary>
    /// 编辑期间（有键盘焦点）的文本变化检测：
    /// NumericUpDown 在聚焦时只有落在控件 [Minimum, Maximum] 内的输入才会实时更新 Value，
    /// 超出控件上限的输入需在此处兑底检测，统一走 1 秒自动恢复。
    /// </summary>
    private static void OnNumericTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not NumericUpDown nud) return;
        if (nud.DataContext is not Recipe.Data data) return;
        if (data.Type is not ("int" or "float")) return;
        if (e.OriginalSource is not TextBox textBox) return;
        if (!double.TryParse(textBox.Text, out double value)) return;
        if (data.Min == data.Max) return;  //兼容老的json配置min和max都为0的情况
        if (value == 0 || (value >= data.Min && value <= data.Max))
        {
            CancelClamp(nud);
            return;
        }
        ScheduleClamp(nud);
    }

    private static void ScheduleClamp(NumericUpDown nud)
    {
        if (nud.Tag is not DispatcherTimer timer)
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            var weakNud = new WeakReference(nud);
            timer.Tick += (_, _) => OnNumericClampTick(weakNud, timer);
            nud.Tag = timer;
        }
        timer.Stop();
        timer.Start();
    }

    private static void CancelClamp(NumericUpDown nud)
    {
        if (nud.Tag is DispatcherTimer timer)
        {
            timer.Stop();
        }
    }

    private static void OnNumericClampTick(WeakReference weakNud, DispatcherTimer timer)
    {
        timer.Stop();
        if (weakNud.Target is not NumericUpDown nud) return;
        if (nud.DataContext is not Recipe.Data data) return;

        double value = nud.Value;
        if (value == 0) return;
        if (value >= data.Min && value <= data.Max) return;

        // 自动恢复为最小值或最大值
        nud.Value = value < data.Min ? data.Min : data.Max;
        // 控件在获得键盘焦点时不刷新内部文本框（SetText 仅在未聚焦时生效），
        // 强制刷新一次显示，否则要等光标移出输入框才能看到恢复后的值
        var setText = typeof(NumericUpDown).GetMethod("SetText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        setText?.Invoke(nud, [true]);
    }
}