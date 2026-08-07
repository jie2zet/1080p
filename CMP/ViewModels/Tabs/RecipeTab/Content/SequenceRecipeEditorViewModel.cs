using CMP.Controls.Basic.InteractiveDialog;
using CMP.Extensions;
using CMP.Services.Auth;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;
using CMP.Services.Log;
using Dm.util;
using DocumentFormat.OpenXml.EMMA;
using DryIoc.ImTools;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Dialog = HandyControl.Controls.Dialog;

namespace CMP.ViewModels.Tabs.RecipeTab.Content;

/// <summary>
/// ViewModel for managing sequence recipes in a manufacturing process.
/// Handles polish and clean operations for A-side and B-side components.
/// </summary>
public class SequenceRecipeEditorViewModel : BindableBase
{
    #region Private Fields

    private readonly IEventAggregator _eventAggregator;

    private bool _dirty;
    private bool _isApplying;
    private SequenceRecipeData? _selectedSequence;
    private bool _isLoading;

    // Polish operation fields
    private bool _polishMainASideChecked;
    private SequenceType _polishMainType;
    private bool _polishMainBSideChecked;
    private bool _polishASideOperationAChecked;
    private SequenceType _polishASideOperationAType;
    private bool _polishASideOperationBChecked;
    private bool _polishBSideOperationCChecked;
    private SequenceType _polishBSideOperationCType;
    private bool _polishBSideOperationDChecked;

    // Clean operation fields
    private bool _cleanMainASideChecked;
    private SequenceType _cleanMainType;
    private bool _cleanMainBSideChecked;

    // Clean A-side roller operations
    private bool _cleanASideRollerOp1Checked;
    private SequenceType _cleanASideRollerOp1Type;
    private bool _cleanASideRollerOp2Checked;
    private SequenceType _cleanASideRollerOp2Type;
    private bool _cleanASideRollerOp3Checked;

    // Clean A-side pen operations
    private bool _cleanASidePenOp1Checked;
    private SequenceType _cleanASidePenOp1Type;
    private bool _cleanASidePenOp2Checked;
    private bool _cleanASideDryOpChecked;

    // Clean B-side roller operations
    private bool _cleanBSideRollerOp1Checked;
    private SequenceType _cleanBSideRollerOp1Type;
    private bool _cleanBSideRollerOp2Checked;
    private SequenceType _cleanBSideRollerOp2Type;
    private bool _cleanBSideRollerOp3Checked;

    // Clean B-side pen operations
    private bool _cleanBSidePenOp1Checked;
    private SequenceType _cleanBSidePenOp1Type;
    private bool _cleanBSidePenOp2Checked;
    private bool _cleanBSideDryOpChecked;

    private ObservableCollection<RecipeItem> _polishRecipeItems = new();
    private ObservableCollection<RecipeItem> _rollerRecipeItems = new();
    private ObservableCollection<RecipeItem> _penRecipeItems = new();
    private ObservableCollection<RecipeItem> _dryRecipeItems = new();
    private ObservableCollection<RecipeItem> _conditionRecipeItems = new();

    public ICollectionView FilteredSequences { get; }
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterSequences();
        }
    }
    private void FilterSequences()
    {
        FilteredSequences.Refresh();
    }
    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates whether the current sequence has unsaved changes
    /// </summary>
    public bool Dirty
    {
        get => _dirty;
        set => SetProperty(ref _dirty, value);
    }

    /// <summary>
    /// Available sequence operation types for UI binding
    /// </summary>
    public ObservableCollection<SequenceOperationType> TypeItems { get; set; } =
    [
        new() { Value = SequenceType.Serial, Name = "Serial" }, // Serial execution
        new() { Value = SequenceType.Parallel, Name = "Parallel" }, // Parallel execution
        //new() { Value = SequenceType.None, Name = "/" } // No operation
    ];

    /// <summary>
    /// Collection of all sequence recipes stored in JSON file
    /// </summary>
    public ObservableCollection<SequenceRecipeData> Sequences { get; } = [];

    /// <summary>
    /// Currently selected sequence recipe for editing
    /// </summary>
    public SequenceRecipeData? SelectedSequence
    {
        get => _selectedSequence;
        set
        {
            if (_selectedSequence == value) return;
            SetProperty(ref _selectedSequence, value);
            Dirty = false;
            ApplyState(value);
        }
    }

    public ObservableCollection<RecipeItem> PolishRecipeItems
    {
        get => _polishRecipeItems;
        set => SetProperty(ref _polishRecipeItems, value);
    }

    public ObservableCollection<RecipeItem> RollerRecipeItems
    {
        get { return _rollerRecipeItems; }
        set
        {
            SetProperty(ref _rollerRecipeItems, value);
        }
    }

    public ObservableCollection<RecipeItem> PenRecipeItems
    {
        get { return _penRecipeItems; }
        set
        {
            SetProperty(ref _penRecipeItems, value);
        }
    }

    public ObservableCollection<RecipeItem> DryRecipeItems
    {
        get { return _dryRecipeItems; }
        set
        {
            SetProperty(ref _dryRecipeItems, value);
        }
    }
    
    public ObservableCollection<RecipeItem> ConditionRecipeItems
    {
        get { return _conditionRecipeItems; }
        set
        {
            SetProperty(ref _conditionRecipeItems, value);
        }
    }

    private string? _polishARecipe;
    private string? _polishBRecipe;
    private string? _polishCRecipe;
    private string? _polishDRecipe;
    private string? _polishAExsituRecipe;
    private string? _polishBExsituRecipe;
    private string? _polishCExsituRecipe;
    private string? _polishDExsituRecipe;
    private string? _cleanARoller1Recipe;
    private string? _cleanARoller2Recipe;
    private string? _cleanARoller3Recipe;
    private string? _cleanBRoller1Recipe;
    private string? _cleanBRoller2Recipe;
    private string? _cleanBRoller3Recipe;
    private string? _cleanAPen1Recipe;
    private string? _cleanAPen2Recipe;
    private string? _cleanBPen1Recipe;
    private string? _cleanBPen2Recipe;
    private string? _cleanADryRecipe;
    private string? _cleanBDryRecipe;

    public string? PolishARecipe
    {
        get => _polishARecipe;
        set => SetProperty(ref _polishARecipe, value);
    }

    public string? PolishBRecipe
    {
        get => _polishBRecipe;
        set => SetProperty(ref _polishBRecipe, value);
    }

    public string? PolishCRecipe
    {
        get => _polishCRecipe;
        set => SetProperty(ref _polishCRecipe, value);
    }

    public string? PolishDRecipe
    {
        get => _polishDRecipe;
        set => SetProperty(ref _polishDRecipe, value);
    }

    public string? PolishAExsituRecipe
    {
        get => _polishAExsituRecipe;
        set => SetProperty(ref _polishAExsituRecipe, value);
    }
    
    public string? PolishBExsituRecipe
    {
        get => _polishBExsituRecipe;
        set => SetProperty(ref _polishBExsituRecipe, value);
    }
    
    public string? PolishCExsituRecipe
    {
        get => _polishCExsituRecipe;
        set => SetProperty(ref _polishCExsituRecipe, value);
    }
    
    public string? PolishDExsituRecipe
    {
        get => _polishDExsituRecipe;
        set => SetProperty(ref _polishDExsituRecipe, value);
    }

    public string? CleanARoller1Recipe
    {
        get => _cleanARoller1Recipe;
        set => SetProperty(ref _cleanARoller1Recipe, value);
    }

    public string? CleanARoller2Recipe
    {
        get => _cleanARoller2Recipe;
        set => SetProperty(ref _cleanARoller2Recipe, value);
    }

    public string? CleanARoller3Recipe
    {
        get => _cleanARoller3Recipe;
        set => SetProperty(ref _cleanARoller3Recipe, value);
    }

    public string? CleanBRoller1Recipe
    {
        get => _cleanBRoller1Recipe;
        set => SetProperty(ref _cleanBRoller1Recipe, value);
    }

    public string? CleanBRoller2Recipe
    {
        get => _cleanBRoller2Recipe;
        set => SetProperty(ref _cleanBRoller2Recipe, value);
    }

    public string? CleanBRoller3Recipe
    {
        get => _cleanBRoller3Recipe;
        set => SetProperty(ref _cleanBRoller3Recipe, value);
    }

    public string? CleanAPen1Recipe
    {
        get => _cleanAPen1Recipe;
        set => SetProperty(ref _cleanAPen1Recipe, value);
    }

    public string? CleanAPen2Recipe
    {
        get => _cleanAPen2Recipe;
        set => SetProperty(ref _cleanAPen2Recipe, value);
    }

    public string? CleanBPen1Recipe
    {
        get => _cleanBPen1Recipe;
        set => SetProperty(ref _cleanBPen1Recipe, value);
    }

    public string? CleanBPen2Recipe
    {
        get => _cleanBPen2Recipe;
        set => SetProperty(ref _cleanBPen2Recipe, value);
    }

    public string? CleanADryRecipe
    {
        get => _cleanADryRecipe;
        set => SetProperty(ref _cleanADryRecipe, value);
    }

    public string? CleanBDryRecipe
    {
        get => _cleanBDryRecipe;
        set => SetProperty(ref _cleanBDryRecipe, value);
    }


    #endregion

    #region Commands

    /// <summary>
    /// Command to create a new sequence recipe
    /// </summary>
    public DelegateCommand NewCommand { get; }

    /// <summary>
    /// Command to delete the selected sequence recipe
    /// </summary>
    public DelegateCommand DeleteCommand { get; }

    /// <summary>
    /// Command to save changes to the current sequence recipe
    /// </summary>
    public DelegateCommand SaveCommand { get; }

    /// <summary>
    /// Command to save the current sequence as a new recipe
    /// </summary>
    public DelegateCommand SaveAsCommand { get; }

    /// <summary>
    /// Command to rename the selected sequence recipe
    /// </summary>
    public DelegateCommand RenameCommand { get; }

    #endregion

    #region Polish Operation Properties

    /// <summary>
    /// Whether polish main A-side operation is enabled
    /// </summary>
    public bool PolishMainASideChecked
    {
        get => _polishMainASideChecked;
        set => SetProperty(ref _polishMainASideChecked, value);
    }

    /// <summary>
    /// Execution type for polish main operation
    /// </summary>
    public SequenceType PolishMainType
    {
        get => _polishMainType;
        set => SetProperty(ref _polishMainType, value);
    }

    /// <summary>
    /// Whether polish main B-side operation is enabled
    /// </summary>
    public bool PolishMainBSideChecked
    {
        get => _polishMainBSideChecked;
        set => SetProperty(ref _polishMainBSideChecked, value);
    }

    /// <summary>
    /// Whether polish A-side operation A is enabled
    /// </summary>
    public bool PolishASideOperationAChecked
    {
        get => _polishASideOperationAChecked;
        set => SetProperty(ref _polishASideOperationAChecked, value);
    }

    /// <summary>
    /// Execution type for polish A-side operation A
    /// </summary>
    public SequenceType PolishASideOperationAType
    {
        get => _polishASideOperationAType;
        set => SetProperty(ref _polishASideOperationAType, value);
    }

    /// <summary>
    /// Whether polish A-side operation B is enabled
    /// </summary>
    public bool PolishASideOperationBChecked
    {
        get => _polishASideOperationBChecked;
        set => SetProperty(ref _polishASideOperationBChecked, value);
    }

    /// <summary>
    /// Whether polish B-side operation C is enabled
    /// </summary>
    public bool PolishBSideOperationCChecked
    {
        get => _polishBSideOperationCChecked;
        set => SetProperty(ref _polishBSideOperationCChecked, value);
    }

    /// <summary>
    /// Execution type for polish B-side operation C
    /// </summary>
    public SequenceType PolishBSideOperationCType
    {
        get => _polishBSideOperationCType;
        set => SetProperty(ref _polishBSideOperationCType, value);
    }

    /// <summary>
    /// Whether polish B-side operation D is enabled
    /// </summary>
    public bool PolishBSideOperationDChecked
    {
        get => _polishBSideOperationDChecked;
        set => SetProperty(ref _polishBSideOperationDChecked, value);
    }

    #endregion

    #region Clean Operation Properties

    /// <summary>
    /// Whether clean main A-side operation is enabled
    /// </summary>
    public bool CleanMainASideChecked
    {
        get => _cleanMainASideChecked;
        set => SetProperty(ref _cleanMainASideChecked, value);
    }

    /// <summary>
    /// Execution type for clean main operation
    /// </summary>
    public SequenceType CleanMainType
    {
        get => _cleanMainType;
        set => SetProperty(ref _cleanMainType, value);
    }

    /// <summary>
    /// Whether clean main B-side operation is enabled
    /// </summary>
    public bool CleanMainBSideChecked
    {
        get => _cleanMainBSideChecked;
        set => SetProperty(ref _cleanMainBSideChecked, value);
    }

    // Clean A-side roller operation properties
    public bool CleanASideRollerOp1Checked
    {
        get => _cleanASideRollerOp1Checked;
        set => SetProperty(ref _cleanASideRollerOp1Checked, value);
    }

    public SequenceType CleanASideRollerOp1Type
    {
        get => _cleanASideRollerOp1Type;
        set => SetProperty(ref _cleanASideRollerOp1Type, value);
    }

    public bool CleanASideRollerOp2Checked
    {
        get => _cleanASideRollerOp2Checked;
        set => SetProperty(ref _cleanASideRollerOp2Checked, value);
    }

    public SequenceType CleanASideRollerOp2Type
    {
        get => _cleanASideRollerOp2Type;
        set => SetProperty(ref _cleanASideRollerOp2Type, value);
    }

    public bool CleanASideRollerOp3Checked
    {
        get => _cleanASideRollerOp3Checked;
        set => SetProperty(ref _cleanASideRollerOp3Checked, value);
    }

    // Clean A-side pen operation properties
    public bool CleanASidePenOp1Checked
    {
        get => _cleanASidePenOp1Checked;
        set => SetProperty(ref _cleanASidePenOp1Checked, value);
    }

    public SequenceType CleanASidePenOp1Type
    {
        get => _cleanASidePenOp1Type;
        set => SetProperty(ref _cleanASidePenOp1Type, value);
    }

    public bool CleanASidePenOp2Checked
    {
        get => _cleanASidePenOp2Checked;
        set => SetProperty(ref _cleanASidePenOp2Checked, value);
    }

    public bool CleanASideDryOpChecked
    {
        get => _cleanASideDryOpChecked;
        set => SetProperty(ref _cleanASideDryOpChecked, value);
    }

    // Clean B-side roller operation properties
    public bool CleanBSideRollerOp1Checked
    {
        get => _cleanBSideRollerOp1Checked;
        set => SetProperty(ref _cleanBSideRollerOp1Checked, value);
    }

    public SequenceType CleanBSideRollerOp1Type
    {
        get => _cleanBSideRollerOp1Type;
        set => SetProperty(ref _cleanBSideRollerOp1Type, value);
    }

    public bool CleanBSideRollerOp2Checked
    {
        get => _cleanBSideRollerOp2Checked;
        set => SetProperty(ref _cleanBSideRollerOp2Checked, value);
    }

    public SequenceType CleanBSideRollerOp2Type
    {
        get => _cleanBSideRollerOp2Type;
        set => SetProperty(ref _cleanBSideRollerOp2Type, value);
    }

    public bool CleanBSideRollerOp3Checked
    {
        get => _cleanBSideRollerOp3Checked;
        set => SetProperty(ref _cleanBSideRollerOp3Checked, value);
    }

    // Clean B-side pen operation properties
    public bool CleanBSidePenOp1Checked
    {
        get => _cleanBSidePenOp1Checked;
        set => SetProperty(ref _cleanBSidePenOp1Checked, value);
    }

    public SequenceType CleanBSidePenOp1Type
    {
        get => _cleanBSidePenOp1Type;
        set => SetProperty(ref _cleanBSidePenOp1Type, value);
    }

    public bool CleanBSidePenOp2Checked
    {
        get => _cleanBSidePenOp2Checked;
        set => SetProperty(ref _cleanBSidePenOp2Checked, value);
    }

    public bool CleanBSideDryOpChecked
    {
        get => _cleanBSideDryOpChecked;
        set => SetProperty(ref _cleanBSideDryOpChecked, value);
    }

    #endregion

    #region Constructor

    public SequenceRecipeEditorViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        // Track property changes to mark as dirty (except for specific properties)
        PropertyChanged += (_, e) =>
        {
            if (_isApplying) return;
            if (e.PropertyName is nameof(Dirty) or nameof(SelectedSequence)) return;
            Dirty = true;
        };

        // Initialize sequence directory and load existing sequences
        var dir = GetSequenceDirectory();
        Directory.CreateDirectory(dir);

        // Initialize commands with appropriate conditions
        NewCommand = new DelegateCommand(NewSequence);
        DeleteCommand = new DelegateCommand(DeleteSequence).ObservesProperty(() => SelectedSequence);
        SaveCommand = new DelegateCommand(SaveSequence).ObservesProperty(() => SelectedSequence);
        SaveAsCommand = new DelegateCommand(SaveAsSequence);
        RenameCommand = new DelegateCommand(RenameSequence);

        PolishRecipeItems = new ObservableCollection<RecipeItem>();
        RollerRecipeItems = new ObservableCollection<RecipeItem>();
        PenRecipeItems = new ObservableCollection<RecipeItem>();
        DryRecipeItems = new ObservableCollection<RecipeItem>();
        ConditionRecipeItems = new ObservableCollection<RecipeItem>();

        // 订阅Recipe保存事件，当Recipe编辑器保存时自动重新加载Unit Recipes
        _eventAggregator.GetEvent<RecipeSavedEvent>().Subscribe(OnRecipeSaved, ThreadOption.UIThread);


        FilteredSequences = CollectionViewSource.GetDefaultView(Sequences);
        FilteredSequences.Filter = FilterPredicate;

        _ = InitializeAsync();
    }
    private bool FilterPredicate(object obj)
    {
        if (obj is SequenceRecipeData seq)
        {
            return string.IsNullOrEmpty(SearchText) || seq.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
    #endregion

    #region Private Methods

    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var sequenceItems = await Task.Run(LoadSequenceData).ConfigureAwait(false);
            var polishItems = await Task.Run(() => LoadUnitRecipeItems(_polishRecipeDirectory)).ConfigureAwait(false);
            var conditionItems = await Task.Run(() => LoadUnitRecipeItems(_conditionRecipeDirectory)).ConfigureAwait(false);
            var rollerItems = await Task.Run(() => LoadUnitRecipeItems(_rollerRecipeDirectory)).ConfigureAwait(false);
            var penItems = await Task.Run(() => LoadUnitRecipeItems(_penRecipeDirectory)).ConfigureAwait(false);
            var dryItems = await Task.Run(() => LoadUnitRecipeItems(_dryRecipeDirectory)).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Sequences.Clear();
                foreach (var seq in sequenceItems)
                {
                    Sequences.Add(seq);
                }

                ReplaceItems(PolishRecipeItems, polishItems);
                ReplaceItems(ConditionRecipeItems, conditionItems);
                ReplaceItems(RollerRecipeItems, rollerItems);
                ReplaceItems(PenRecipeItems, penItems);
                ReplaceItems(DryRecipeItems, dryItems);

                FilteredSequences.Refresh();
            });
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to initialize sequence editor data.", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static void ReplaceItems(ObservableCollection<RecipeItem> target, List<RecipeItem> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    /// <summary>
    /// Gets the directory path where sequence files are stored
    /// </summary>
    private string GetSequenceDirectory() =>
        Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence");

    private string _polishRecipeDirectory = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\Polish Recipe";
    private string _conditionRecipeDirectory = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\Condition Recipe";
    private string _rollerRecipeDirectory = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\Roll Recipe";
    private string _penRecipeDirectory = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\Pen Recipe";
    private string _dryRecipeDirectory = "D:\\CMP\\Configuration\\Recipes\\UnitRecipe\\Dry Recipe";

    /// <summary>
    /// Loads all sequences from individual JSON files
    /// </summary>
    private void LoadAllSequences()
    {
        var dir = GetSequenceDirectory();
        if (!Directory.Exists(dir))
        {
            Logger.Warn("Sequence directory not found.");
            return;
        }

        try
        {
            var jsonFiles = Directory.GetFiles(dir, "*.json");
            Sequences.Clear();

            foreach (var file in jsonFiles)
            {
                try
                {
                    var text = File.ReadAllText(file);
                    var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);

                    if (data != null && !string.IsNullOrWhiteSpace(data.Name))
                    {
                        Sequences.Add(data);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to load sequence file: {file}", ex);
                }
            }

            if (Sequences.Count > 0)
            {
                Logger.Info($"Loaded {Sequences.Count} sequence recipes.");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to load sequences.", ex);
        }
    }

    private List<SequenceRecipeData> LoadSequenceData()
    {
        var result = new List<SequenceRecipeData>();
        var dir = GetSequenceDirectory();
        if (!Directory.Exists(dir))
        {
            Logger.Warn("Sequence directory not found.");
            return result;
        }

        try
        {
            var jsonFiles = Directory.GetFiles(dir, "*.json");

            foreach (var file in jsonFiles)
            {
                try
                {
                    var text = File.ReadAllText(file);
                    var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
                    if (data != null && !string.IsNullOrWhiteSpace(data.Name))
                    {
                        result.Add(data);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to load sequence file: {file}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to load sequences.", ex);
        }

        return result;
    }

    private static List<RecipeItem> LoadUnitRecipeItems(string recipeDirectory)
    {
        var result = new List<RecipeItem>();
        if (!Directory.Exists(recipeDirectory))
            return result;

        var dir = new DirectoryInfo(recipeDirectory);
        foreach (var file in dir.GetFiles("*.json"))
        {
            result.Add(new RecipeItem
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                Value = file.FullName
            });
        }

        return result;
    }

    private void LoadAllUnitRecipeItems()
    {
        LoadPolishRecipeItems();
        LoadConditionRecipeItems();
        LoadRollerRecipeItems();
        LoadPenRecipeItems();
        LoadDryRecipeItems();
    }

    private void LoadPolishRecipeItems()
    {
        PolishRecipeItems.Clear();
        if (!Directory.Exists(_polishRecipeDirectory))
            return;

        var dir = new DirectoryInfo(_polishRecipeDirectory);
        foreach (var file in dir.GetFiles("*.json"))
        {
            PolishRecipeItems.Add(new RecipeItem
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                Value = file.FullName
            });
        }
    }
    
    private void LoadConditionRecipeItems()
    {
        ConditionRecipeItems.Clear();
        if (!Directory.Exists(_conditionRecipeDirectory))
            return;

        var dir = new DirectoryInfo(_conditionRecipeDirectory);
        foreach (var file in dir.GetFiles("*.json"))
        {
            ConditionRecipeItems.Add(new RecipeItem
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                Value = file.FullName
            });
        }
    }

    private void LoadRollerRecipeItems()
    {
        RollerRecipeItems.Clear();
        if (!Directory.Exists(_rollerRecipeDirectory))
            return;

        var dir = new DirectoryInfo(_rollerRecipeDirectory);
        foreach (var file in dir.GetFiles("*.json"))
        {
            RollerRecipeItems.Add(new RecipeItem
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                Value = file.FullName
            });
        }
    }

    private void LoadPenRecipeItems()
    {
        PenRecipeItems.Clear();
        if (!Directory.Exists(_penRecipeDirectory))
            return;

        var dir = new DirectoryInfo(_penRecipeDirectory);
        foreach (var file in dir.GetFiles("*.json"))
        {
            PenRecipeItems.Add(new RecipeItem
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                Value = file.FullName
            });
        }
    }

    private void LoadDryRecipeItems()
    {
        DryRecipeItems.Clear();
        if (!Directory.Exists(_dryRecipeDirectory))
            return;

        var dir = new DirectoryInfo(_dryRecipeDirectory);
        foreach (var file in dir.GetFiles("*.json"))
        {
            DryRecipeItems.Add(new RecipeItem
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                Value = file.FullName
            });
        }
    }

    /// <summary>
    /// Saves a single sequence to its own JSON file
    /// </summary>
    private void SaveSequence(SequenceRecipeData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name)) return;

        var dir = GetSequenceDirectory();
        Directory.CreateDirectory(dir);

        var fileName = GetSafeFileName(data.Name);
        var filePath = Path.Combine(dir, $"{fileName}.json");

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Creates a new sequence recipe with user-provided name
    /// </summary>
    private async void NewSequence()
    {
        var name = await Dialog.Show<InteractiveDialog>()
            .Initialize<InteractiveDialogViewModel>(vm => vm.Message = Resources.TypeNewPlanName)
            .GetResultAsync<string>();

        if (string.IsNullOrWhiteSpace(name) || "Cancel".equals(name)) return;

        // Ensure unique name
        var uniqueName = GetUniqueName(name.Trim());

        var data = new SequenceRecipeData { Name = uniqueName };
        Sequences.Add(data);
        SelectedSequence = data;
        SaveSequence(data);
        LoadAllSequences();
        AuditLogService.WriteCurrentUser(AuditActions.RecipeCreate, "Success", uniqueName);
    }

    /// <summary>
    /// Renames the selected sequence recipe
    /// </summary>
    private async void RenameSequence()
    {
        if (SelectedSequence == null) return;

        var newName = await Dialog.Show<InteractiveDialog>()
            .Initialize<InteractiveDialogViewModel>(vm =>
            {
                vm.Message = Resources.TypeNewPlanName;
                vm.Result = SelectedSequence.Name;
            })
            .GetResultAsync<string>();

        if (string.IsNullOrWhiteSpace(newName) || newName.Trim() == SelectedSequence.Name)
            return;

        var trimmedName = newName.Trim();

        // Check for duplicate names
        if (IsNameExists(trimmedName, SelectedSequence))
        {
            Growl.ErrorGlobal(Resources.NameExists);
            return;
        }

        // Delete old file
        DeleteSequenceFile(SelectedSequence);

        // Update name and save new file
        var oldName = SelectedSequence.Name;
        SelectedSequence.Name = trimmedName;

        var idx = Sequences.IndexOf(SelectedSequence);
        if (idx >= 0)
        {
            // Force collection change notification
            var temp = SelectedSequence;
            Sequences.RemoveAt(idx);
            Sequences.Insert(idx, temp);

            SaveSequence(temp);
            Growl.SuccessGlobal($"{Resources.Renamed}: {oldName} → {trimmedName}");
            AuditLogService.WriteCurrentUser(
                AuditActions.RecipeRename,
                "Success",
                $"Sequence:{oldName}->{trimmedName}");
        }
    }

    /// <summary>
    /// Deletes the selected sequence recipe
    /// </summary>
    private void DeleteSequence()
    {
        if (SelectedSequence == null) return;

        var deletedName = SelectedSequence.Name;
        DeleteSequenceFile(SelectedSequence);
        Sequences.Remove(SelectedSequence);
        SelectedSequence = null;
        LoadAllSequences();
        AuditLogService.WriteCurrentUser(AuditActions.RecipeDelete, "Success", $"Sequence:{deletedName}");
    }

    /// <summary>
    /// Saves the current sequence state to the selected recipe
    /// </summary>
    private void SaveSequence()
    {
        if (SelectedSequence == null) 
            return;

        // Collect current state and preserve existing metadata
        var data = CollectState();
        data.Name = SelectedSequence.Name;

        var idx = Sequences.IndexOf(SelectedSequence);
        Sequences[idx] = data;
        SelectedSequence = data;
        SaveSequence(data);
        Growl.SuccessGlobal(Resources.SaveSuccessful);
        AuditLogService.WriteCurrentUser(AuditActions.RecipeSave, "Success", $"Sequence:{data.Name}");

        ////仅供测试使用
        //Wafer wafer = new Wafer();
        //wafer.SequenceRecipe = SelectedSequence?.Name!;
        //SetSequenceToWafer(wafer);
        //TestWriteStep("ZF89000", wafer, GlobalService.kvHostLinkInit.kvLinkPolishA, "PolishA");
        //TestWriteStep("ZF89000", wafer, GlobalService.kvHostLinkInit.kvLinkPolishB, "PolishB");
        //TestWriteStep("ZF89000", wafer, GlobalService.kvHostLinkInit.kvLinkPolishC, "PolishC");
        //TestWriteStep("ZF89000", wafer, GlobalService.kvHostLinkInit.kvLinkPolishD, "PolishD");
        //TestWriteStep("ZF74500", wafer, GlobalService.kvHostLinkInit.kvLinkRollerA, "PolishA1");
        //TestWriteStep("ZF94500", wafer, GlobalService.kvHostLinkInit.kvLinkRollerA, "PolishA2");
        //TestWriteStep("ZF114500", wafer, GlobalService.kvHostLinkInit.kvLinkRollerA, "PolishA3");
        //TestWriteStep("ZF74500", wafer, GlobalService.kvHostLinkInit.kvLinkRollerB, "PolishA1");
        //TestWriteStep("ZF94500", wafer, GlobalService.kvHostLinkInit.kvLinkRollerB, "PolishA2");
        //TestWriteStep("ZF114500", wafer, GlobalService.kvHostLinkInit.kvLinkRollerB, "PolishA3");
        //TestWriteStep("ZF74500", wafer, GlobalService.kvHostLinkInit.kvLinkPenA, "PenA1");
        //TestWriteStep("ZF94500", wafer, GlobalService.kvHostLinkInit.kvLinkPenA, "PenA2");
        //TestWriteStep("ZF74500", wafer, GlobalService.kvHostLinkInit.kvLinkPenB, "PenB1");
        //TestWriteStep("ZF94500", wafer, GlobalService.kvHostLinkInit.kvLinkPenB, "PenB2");
        //TestWriteStep("ZF74500", wafer, GlobalService.kvHostLinkInit.kvLinkDryA, "DryA");
        //TestWriteStep("ZF74500", wafer, GlobalService.kvHostLinkInit.kvLinkDryB, "DryB");
    }

    /// <summary>
    /// Saves the current sequence state as a new recipe
    /// </summary>
    private async void SaveAsSequence()
    {
        if (SelectedSequence == null) return;

        var name = await Dialog.Show<InteractiveDialog>()
            .Initialize<InteractiveDialogViewModel>(vm => vm.Message = Resources.TypeNewPlanName)
            .GetResultAsync<string>();

        if (string.IsNullOrWhiteSpace(name)) return;

        var trimmedName = name.Trim();

        // Check for duplicate names and show error instead of auto-adjusting
        if (IsNameExists(trimmedName))
        {
            Growl.ErrorGlobal(Resources.NameExists);
            return;
        }

        var data = CollectState();
        data.Name = trimmedName;
        Sequences.Add(data);
        SelectedSequence = data;
        SaveSequence(data);
        Growl.SuccessGlobal(Resources.SaveSuccessful);
        AuditLogService.WriteCurrentUser(AuditActions.RecipeSaveAs, "Success", $"Sequence:{trimmedName}");
    }

    /// <summary>
    /// Deletes the sequence file from disk
    /// </summary>
    private void DeleteSequenceFile(SequenceRecipeData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name)) return;

        var dir = GetSequenceDirectory();
        var fileName = GetSafeFileName(data.Name);
        var filePath = Path.Combine(dir, $"{fileName}.json");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Converts sequence name to safe filename
    /// </summary>
    private string GetSafeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(safeName) ? "unnamed" : safeName;
    }

    /// <summary>
    /// Checks if a sequence name already exists (case-insensitive)
    /// </summary>
    private bool IsNameExists(string name, SequenceRecipeData? excludeItem = null)
    {
        return Sequences.Any(s => s != excludeItem &&
                                  string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Generates a unique name by appending a number suffix
    /// </summary>
    private string GetUniqueName(string baseName)
    {
        if (!IsNameExists(baseName))
            return baseName;

        var counter = 1;
        string uniqueName;
        do
        {
            uniqueName = $"{baseName}_{counter}";
            counter++;
        } while (IsNameExists(uniqueName));

        return uniqueName;
    }

    /// <summary>
    /// Collects the current UI state into a SequenceRecipeData object
    /// </summary>
    private SequenceRecipeData CollectState()
    {
        var data = new SequenceRecipeData
        {
            Name = SelectedSequence?.Name ?? string.Empty,

            // Polish main operations
            PolishMainASideChecked = PolishMainASideChecked,
            PolishMainType = PolishMainType,
            PolishMainBSideChecked = PolishMainBSideChecked,

            // Polish A-side operations
            PolishASideOperationAChecked = PolishASideOperationAChecked,
            PolishASideOperationType = PolishASideOperationAType,
            PolishASideOperationBChecked = PolishASideOperationBChecked,

            // Polish B-side operations
            PolishBSideOperationCChecked = PolishBSideOperationCChecked,
            PolishBSideOperationType = PolishBSideOperationCType,
            PolishBSideOperationDChecked = PolishBSideOperationDChecked,

            // Clean main operations
            CleanMainASideChecked = CleanMainASideChecked,
            CleanMainType = CleanMainType,
            CleanMainBSideChecked = CleanMainBSideChecked,

            // Clean A-side roller operations
            CleanASideRollerOp1Checked = CleanASideRollerOp1Checked,
            CleanASideRollerOp1Type = CleanASideRollerOp1Type,
            CleanASideRollerOp2Checked = CleanASideRollerOp2Checked,
            CleanASideRollerOp2Type = CleanASideRollerOp2Type,
            CleanASideRollerOp3Checked = CleanASideRollerOp3Checked,

            // Clean A-side pen operations
            CleanASidePenOp1Checked = CleanASidePenOp1Checked,
            CleanASidePenOp1Type = CleanASidePenOp1Type,
            CleanASidePenOp2Checked = CleanASidePenOp2Checked,
            CleanASideDryOpChecked = CleanASideDryOpChecked,

            // Clean B-side roller operations
            CleanBSideRollerOp1Checked = CleanBSideRollerOp1Checked,
            CleanBSideRollerOp1Type = CleanBSideRollerOp1Type,
            CleanBSideRollerOp2Checked = CleanBSideRollerOp2Checked,
            CleanBSideRollerOp2Type = CleanBSideRollerOp2Type,
            CleanBSideRollerOp3Checked = CleanBSideRollerOp3Checked,

            // Clean B-side pen operations
            CleanBSidePenOp1Checked = CleanBSidePenOp1Checked,
            CleanBSidePenOp1Type = CleanBSidePenOp1Type,
            CleanBSidePenOp2Checked = CleanBSidePenOp2Checked,
            CleanBSideDryOpChecked = CleanBSideDryOpChecked,

            // Recipe selection stubs for future implementation
            RecipeSelections =
            [
                new RecipeSelection { Key = "RecipePolishA", Value = PolishASideOperationAChecked ? PolishARecipe : null },
                new RecipeSelection { Key = "RecipePolishB", Value = PolishASideOperationBChecked ? PolishBRecipe : null },
                new RecipeSelection { Key = "RecipePolishC", Value = PolishBSideOperationCChecked ? PolishCRecipe : null },
                new RecipeSelection { Key = "RecipePolishD", Value = PolishBSideOperationDChecked ? PolishDRecipe : null },
                new RecipeSelection { Key = "RecipePolishAExsitu", Value = PolishASideOperationAChecked ? PolishAExsituRecipe : null },
                new RecipeSelection { Key = "RecipePolishBExsitu", Value = PolishASideOperationBChecked ? PolishBExsituRecipe : null },
                new RecipeSelection { Key = "RecipePolishCExsitu", Value = PolishBSideOperationCChecked ? PolishCExsituRecipe : null },
                new RecipeSelection { Key = "RecipePolishDExsitu", Value = PolishBSideOperationDChecked ? PolishDExsituRecipe : null },
                new RecipeSelection { Key = "RecipeRollerA1", Value = CleanASideRollerOp1Checked ? CleanARoller1Recipe : null },
                new RecipeSelection { Key = "RecipeRollerA2", Value = CleanASideRollerOp2Checked ? CleanARoller2Recipe : null },
                new RecipeSelection { Key = "RecipeRollerA3", Value = CleanASideRollerOp3Checked ? CleanARoller3Recipe : null },
                new RecipeSelection { Key = "RecipeRollerB1", Value = CleanBSideRollerOp1Checked ? CleanBRoller1Recipe : null },
                new RecipeSelection { Key = "RecipeRollerB2", Value = CleanBSideRollerOp2Checked ? CleanBRoller2Recipe : null },
                new RecipeSelection { Key = "RecipeRollerB3", Value = CleanBSideRollerOp3Checked ? CleanBRoller3Recipe : null },
                new RecipeSelection { Key = "RecipePenA1", Value = CleanASidePenOp1Checked ? CleanAPen1Recipe : null },
                new RecipeSelection { Key = "RecipePenA2", Value = CleanASidePenOp2Checked ? CleanAPen2Recipe : null },
                new RecipeSelection { Key = "RecipePenB1", Value = CleanBSidePenOp1Checked ? CleanBPen1Recipe : null },
                new RecipeSelection { Key = "RecipePenB2", Value = CleanBSidePenOp2Checked ? CleanBPen2Recipe : null },
                new RecipeSelection { Key = "RecipeDryA", Value = CleanASideDryOpChecked ? CleanADryRecipe : null },
                new RecipeSelection { Key = "RecipeDryB", Value = CleanBSideDryOpChecked ? CleanBDryRecipe : null }
            ]
        };
        return data;
    }

    /// <summary>
    /// Applies the loaded sequence data to the UI properties
    /// </summary>
    private void ApplyState(SequenceRecipeData? data)
    {
        _isApplying = true;

        if (data == null)
        {
            // Clear all properties when no sequence is selected
            ResetAllProperties();
            _isApplying = false;
            return;
        }

        // Apply polish operations
        PolishMainASideChecked = data.PolishMainASideChecked;
        PolishMainType = data.PolishMainType;
        PolishMainBSideChecked = data.PolishMainBSideChecked;
        PolishASideOperationAChecked = data.PolishASideOperationAChecked;
        PolishASideOperationAType = data.PolishASideOperationType;
        PolishASideOperationBChecked = data.PolishASideOperationBChecked;
        PolishBSideOperationCChecked = data.PolishBSideOperationCChecked;
        PolishBSideOperationCType = data.PolishBSideOperationType;
        PolishBSideOperationDChecked = data.PolishBSideOperationDChecked;

        // Apply clean operations
        CleanMainASideChecked = data.CleanMainASideChecked;
        CleanMainType = data.CleanMainType;
        CleanMainBSideChecked = data.CleanMainBSideChecked;

        // Apply clean A-side operations
        CleanASideRollerOp1Checked = data.CleanASideRollerOp1Checked;
        CleanASideRollerOp1Type = data.CleanASideRollerOp1Type;
        CleanASideRollerOp2Checked = data.CleanASideRollerOp2Checked;
        CleanASideRollerOp2Type = data.CleanASideRollerOp2Type;
        CleanASideRollerOp3Checked = data.CleanASideRollerOp3Checked;
        CleanASidePenOp1Checked = data.CleanASidePenOp1Checked;
        CleanASidePenOp1Type = data.CleanASidePenOp1Type;
        CleanASidePenOp2Checked = data.CleanASidePenOp2Checked;
        CleanASideDryOpChecked = data.CleanASideDryOpChecked;

        // Apply clean B-side operations
        CleanBSideRollerOp1Checked = data.CleanBSideRollerOp1Checked;
        CleanBSideRollerOp1Type = data.CleanBSideRollerOp1Type;
        CleanBSideRollerOp2Checked = data.CleanBSideRollerOp2Checked;
        CleanBSideRollerOp2Type = data.CleanBSideRollerOp2Type;
        CleanBSideRollerOp3Checked = data.CleanBSideRollerOp3Checked;
        CleanBSidePenOp1Checked = data.CleanBSidePenOp1Checked;
        CleanBSidePenOp1Type = data.CleanBSidePenOp1Type;
        CleanBSidePenOp2Checked = data.CleanBSidePenOp2Checked;
        CleanBSideDryOpChecked = data.CleanBSideDryOpChecked;

        // Apply recipe selections
        if (data.RecipeSelections != null)
        {
            foreach (var selection in data.RecipeSelections)
            {
                if (selection == null) continue;
                
                var recipeValue = selection.Value?.ToString();
                switch (selection.Key)
                {
                    case "RecipePolishA":
                        PolishARecipe = recipeValue;
                        break;
                    case "RecipePolishB":
                        PolishBRecipe = recipeValue;
                        break;
                    case "RecipePolishC":
                        PolishCRecipe = recipeValue;
                        break;
                    case "RecipePolishD":
                        PolishDRecipe = recipeValue;
                        break;
                    case "RecipePolishAExsitu":
                        PolishAExsituRecipe = recipeValue;
                        break;
                    case "RecipePolishBExsitu":
                        PolishBExsituRecipe = recipeValue;
                        break;
                    case "RecipePolishCExsitu":
                        PolishCExsituRecipe = recipeValue;
                        break;
                    case "RecipePolishDExsitu":
                        PolishDExsituRecipe = recipeValue;
                        break;
                    case "RecipeRollerA1":
                        CleanARoller1Recipe = recipeValue;
                        break;
                    case "RecipeRollerA2":
                        CleanARoller2Recipe = recipeValue;
                        break;
                    case "RecipeRollerA3":
                        CleanARoller3Recipe = recipeValue;
                        break;
                    case "RecipeRollerB1":
                        CleanBRoller1Recipe = recipeValue;
                        break;
                    case "RecipeRollerB2":
                        CleanBRoller2Recipe = recipeValue;
                        break;
                    case "RecipeRollerB3":
                        CleanBRoller3Recipe = recipeValue;
                        break;
                    case "RecipePenA1":
                        CleanAPen1Recipe = recipeValue;
                        break;
                    case "RecipePenA2":
                        CleanAPen2Recipe = recipeValue;
                        break;
                    case "RecipePenB1":
                        CleanBPen1Recipe = recipeValue;
                        break;
                    case "RecipePenB2":
                        CleanBPen2Recipe = recipeValue;
                        break;
                    case "RecipeDryA":
                        CleanADryRecipe = recipeValue;
                        break;
                    case "RecipeDryB":
                        CleanBDryRecipe = recipeValue;
                        break;
                }
            }
        }

        _isApplying = false;
    }

    /// <summary>
    /// Resets all operation properties to their default values
    /// </summary>
    private void ResetAllProperties()
    {
        // Reset polish operations
        PolishMainASideChecked = false;
        PolishMainType = SequenceType.None;
        PolishMainBSideChecked = false;
        PolishASideOperationAChecked = false;
        PolishASideOperationAType = SequenceType.None;
        PolishASideOperationBChecked = false;
        PolishBSideOperationCChecked = false;
        PolishBSideOperationCType = SequenceType.None;
        PolishBSideOperationDChecked = false;

        // Reset clean operations
        CleanMainASideChecked = false;
        CleanMainType = SequenceType.None;
        CleanMainBSideChecked = false;

        // Reset clean A-side operations
        CleanASideRollerOp1Checked = false;
        CleanASideRollerOp1Type = SequenceType.None;
        CleanASideRollerOp2Checked = false;
        CleanASideRollerOp2Type = SequenceType.None;
        CleanASideRollerOp3Checked = false;
        CleanASidePenOp1Checked = false;
        CleanASidePenOp1Type = SequenceType.None;
        CleanASidePenOp2Checked = false;
        CleanASideDryOpChecked = false;

        // Reset clean B-side operations
        CleanBSideRollerOp1Checked = false;
        CleanBSideRollerOp1Type = SequenceType.None;
        CleanBSideRollerOp2Checked = false;
        CleanBSideRollerOp2Type = SequenceType.None;
        CleanBSideRollerOp3Checked = false;
        CleanBSidePenOp1Checked = false;
        CleanBSidePenOp1Type = SequenceType.None;
        CleanBSidePenOp2Checked = false;
        CleanBSideDryOpChecked = false;

        // Reset recipe selections
        PolishARecipe = null;
        PolishBRecipe = null;
        PolishCRecipe = null;
        PolishDRecipe = null;
        PolishAExsituRecipe = null;
        PolishBExsituRecipe = null;
        PolishCExsituRecipe = null;
        PolishDExsituRecipe = null;
        CleanARoller1Recipe = null;
        CleanARoller2Recipe = null;
        CleanARoller3Recipe = null;
        CleanBRoller1Recipe = null;
        CleanBRoller2Recipe = null;
        CleanBRoller3Recipe = null;
        CleanAPen1Recipe = null;
        CleanAPen2Recipe = null;
        CleanBPen1Recipe = null;
        CleanBPen2Recipe = null;
        CleanADryRecipe = null;
        CleanBDryRecipe = null;
    }

    private string ProcessRecipePath(string fullPath)
    {
        if (fullPath != null)
        {
            string[] pasts = fullPath.Split('\\');
            string last = pasts[pasts.Length - 1];
            string lastWithOutEx = last.Substring(0, last.Length - 5);
            return lastWithOutEx;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Recipe 保存事件处理：当任意 Unit Recipe 被保存时，刷新 UnitRecipe 列表
    /// </summary>
    private void OnRecipeSaved(RecipeSavedEventArgs args)
    {
        try
        {
            if (args == null || string.IsNullOrEmpty(args.RecipeType))
                return;

            // 只在与 UnitRecipe 相关的 RecipeType 时刷新；如果希望所有类型都刷新，可以去掉判断
            if (args.RecipeType.Contains("Recipe"))
            {
                LoadAllUnitRecipeItems();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to reload unit recipes after save: {ex.Message}", ex);
        }
    }

    #endregion

    #region Model

    public class RecipeItem
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    #endregion

    #region Wafer的Sequnce逻辑算法
    private void SetSequenceToWafer(Wafer wafer)
    {
        var path = Path.Combine("D:\\CMP\\Configuration", "Recipes", "Sequence", $"{wafer.SequenceRecipe}.json");
        try
        {
            wafer.WaferSequenceModels = [];
            var text = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<SequenceRecipeData>(text);
            wafer.SequnceValue = new int[16];

            if (data != null && !string.IsNullOrWhiteSpace(data.Name))
            {
                #region 确定每个chamber的执行顺序的步骤
                int currentMaxStep = -1;
                //Polish
                if (data.PolishMainType == SequenceType.Parallel)
                {
                    if (data.PolishMainASideChecked)
                    {
                        if (data.PolishASideOperationType == SequenceType.Parallel)
                        {
                            if (data.PolishASideOperationBChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = 0 });
                                SetCurrentStep(ref currentMaxStep, 0);
                            }
                            if (data.PolishASideOperationAChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = 0 });
                                SetCurrentStep(ref currentMaxStep, 0);
                            }
                        }
                        else if (data.PolishASideOperationType == SequenceType.Serial)
                        {
                            int currentStep = -1;
                            if (data.PolishASideOperationBChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.PolishASideOperationAChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    if (data.PolishMainBSideChecked)
                    {
                        if (data.PolishBSideOperationType == SequenceType.Parallel)
                        {
                            if (data.PolishBSideOperationDChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = 0 });
                                SetCurrentStep(ref currentMaxStep, 0);
                            }
                            if (data.PolishBSideOperationCChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = 0 });
                                SetCurrentStep(ref currentMaxStep, 0);
                            }
                        }
                        else if (data.PolishBSideOperationType == SequenceType.Serial)
                        {
                            int currentStep = -1;
                            if (data.PolishBSideOperationDChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.PolishBSideOperationCChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);

                            }
                        }
                    }
                }
                else if (data.PolishMainType == SequenceType.Serial)
                {
                    if (data.PolishMainASideChecked)
                    {
                        if (data.PolishASideOperationType == SequenceType.Parallel)
                        {
                            if (data.PolishASideOperationBChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = 0 });
                                SetCurrentStep(ref currentMaxStep, 0);
                            }
                            if (data.PolishASideOperationAChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = 0 });
                                SetCurrentStep(ref currentMaxStep, 0);
                            }
                        }
                        else if (data.PolishASideOperationType == SequenceType.Serial)
                        {
                            int currentStep = -1;
                            if (data.PolishASideOperationBChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishB", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.PolishASideOperationAChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishA", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    if (data.PolishMainBSideChecked)
                    {
                        if (data.PolishBSideOperationType == SequenceType.Parallel)
                        {
                            int currentStep = currentMaxStep + 1;
                            if (data.PolishBSideOperationDChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.PolishBSideOperationCChecked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        else if (data.PolishBSideOperationType == SequenceType.Serial)
                        {
                            int currentStep = currentMaxStep;
                            if (data.PolishBSideOperationDChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishD", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.PolishBSideOperationCChecked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PolishC", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                }
                //Roller
                if (data.CleanMainType == SequenceType.Parallel)
                {
                    int tempCurrentMaxStep = currentMaxStep;
                    if (data.CleanMainASideChecked)
                    {
                        if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                            }
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                        if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    if (data.CleanMainBSideChecked)
                    {
                        if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                            }
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                        if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                }
                else if (data.CleanMainType == SequenceType.Serial)
                {
                    int tempCurrentMaxStep = currentMaxStep;
                    if (data.CleanMainASideChecked)
                    {
                        if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                            }
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                        if (data.CleanASideRollerOp1Type == SequenceType.Parallel && data.CleanASideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanASideRollerOp1Type == SequenceType.Serial && data.CleanASideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanASideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerA3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    if (data.CleanMainBSideChecked)
                    {
                        int temCurrentMaxStep2 = currentMaxStep;
                        if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = temCurrentMaxStep2 + 1;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = temCurrentMaxStep2;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                            }
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                        if (data.CleanBSideRollerOp1Type == SequenceType.Parallel && data.CleanBSideRollerOp2Type == SequenceType.Serial)
                        {
                            int currentStep = temCurrentMaxStep2 + 1;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        if (data.CleanBSideRollerOp1Type == SequenceType.Serial && data.CleanBSideRollerOp2Type == SequenceType.Parallel)
                        {
                            int currentStep = temCurrentMaxStep2;
                            if (data.CleanBSideRollerOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSideRollerOp3Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "RollerB3", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                }
                //Pen
                if (data.CleanMainType == SequenceType.Parallel)
                {
                    int tempCurrentMaxStep = currentMaxStep;
                    if (data.CleanMainASideChecked)
                    {
                        if (data.CleanASidePenOp1Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanASidePenOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASidePenOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        else if (data.CleanASidePenOp1Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanASidePenOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASidePenOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    if (data.CleanMainBSideChecked)
                    {
                        if (data.CleanBSidePenOp1Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanBSidePenOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSidePenOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        else if (data.CleanBSidePenOp1Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanBSidePenOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSidePenOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                }
                else if (data.CleanMainType == SequenceType.Serial)
                {
                    int tempCurrentMaxStep = currentMaxStep;
                    if (data.CleanMainASideChecked)
                    {
                        if (data.CleanASidePenOp1Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            if (data.CleanASidePenOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASidePenOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        else if (data.CleanASidePenOp1Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep;
                            if (data.CleanASidePenOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanASidePenOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenA2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                    if (data.CleanMainBSideChecked)
                    {
                        int tempCurrentMaxStep2 = currentMaxStep;
                        if (data.CleanBSidePenOp1Type == SequenceType.Parallel)
                        {
                            int currentStep = tempCurrentMaxStep2 + 1;
                            if (data.CleanBSidePenOp1Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSidePenOp2Checked)
                            {
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                        else if (data.CleanBSidePenOp1Type == SequenceType.Serial)
                        {
                            int currentStep = tempCurrentMaxStep2;
                            if (data.CleanBSidePenOp1Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB1", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                            if (data.CleanBSidePenOp2Checked)
                            {
                                currentStep += 1;
                                wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "PenB2", ChamberSequnceStep = currentStep });
                                SetCurrentStep(ref currentMaxStep, currentStep);
                            }
                        }
                    }
                }
                //Dry
                if (data.CleanMainType == SequenceType.Parallel)
                {
                    int tempCurrentMaxStep = currentMaxStep;
                    if (data.CleanMainASideChecked)
                    {
                        if (data.CleanASideDryOpChecked)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryA", ChamberSequnceStep = currentStep });
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                    }
                    if (data.CleanMainBSideChecked)
                    {
                        if (data.CleanBSideDryOpChecked)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryB", ChamberSequnceStep = currentStep });
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                    }
                }
                else if (data.CleanMainType == SequenceType.Serial)
                {
                    int tempCurrentMaxStep = currentMaxStep;
                    if (data.CleanMainASideChecked)
                    {
                        if (data.CleanASideDryOpChecked)
                        {
                            int currentStep = tempCurrentMaxStep + 1;
                            wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryA", ChamberSequnceStep = currentStep });
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                    }
                    if (data.CleanMainBSideChecked)
                    {
                        int tempCurrentMaxStep2 = currentMaxStep;
                        if (data.CleanBSideDryOpChecked)
                        {
                            int currentStep = tempCurrentMaxStep2 + 1;
                            wafer.WaferSequenceModels.Add(new WaferSequenceModel() { ChamberName = "DryB", ChamberSequnceStep = currentStep });
                            SetCurrentStep(ref currentMaxStep, currentStep);
                        }
                    }
                }
                #endregion

                #region 确定每个chamber步骤需要写入的值
                for (int i = 0; i < wafer.SequnceValue.Length; i++)
                {
                    var list = wafer.WaferSequenceModels.Where(x => x.ChamberSequnceStep == i).ToList();
                    if (list.Count == 0)
                    {
                        wafer.SequnceValue[i] = 0;
                        continue;
                    }
                    //多个值表示并行，值相加
                    int val = 0;
                    foreach (var item in list)
                    {
                        val += item.ChamberStepValue;
                    }
                    wafer.SequnceValue[i] = val;
                }
                #endregion
            }
            else
            {
                Growl.ErrorGlobal("sequence Data is null ");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            throw;
        }
    }

    private void SetCurrentStep(ref int current, int step)
    {
        if (step > current)
        {
            current = step;
        }
    }

    private void TestWriteStep(string baseAddress, Wafer wafer, KVLink link, string workPlace)
    {
        //向地址中写入wafer的sequence数值
        for (int i = 0; i < wafer.SequnceValue.Length; i++)
        {
            byte[] data = new byte[256];
            uint devTopNo = (uint)(Convert.ToUInt32(baseAddress?.Replace("ZF", "")) + 10 + 5 * i);
            CMP.Models.kvsPlc.Polish.KvStruct.IntToByte(ref data, wafer.SequnceValue[i]);
            int error = KvHostLink.KHLWriteDevicesAsWords(link.sock, KHLDevType.DEV_ZF, devTopNo, 2, data);
            if (error != (int)KHLUserError.KHL_ERR_NONE)
            {
                Logger.For("SequenceTest").Debug($"向{workPlace}地址ZF{devTopNo}中写入数值{wafer.SequnceValue[i]}失败，错误码返回码{error}");
            }
            else
            {
                Logger.For("SequenceTest").Debug($"向{workPlace}地址ZF{devTopNo}中写入数值{wafer.SequnceValue[i]}成功");
            }
        }
    }
    #endregion

}

