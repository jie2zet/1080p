using System.Runtime.Serialization;

namespace CMP.Models.Recipe.Sequence;

/// <summary>
/// Data contract for sequence recipe configuration
/// Contains all settings for polish and clean operations
/// </summary>
[DataContract]
public class SequenceRecipeData
{
    [DataMember] public string? Name { get; set; }

    // Polish main operation settings
    [DataMember] public bool PolishMainASideChecked { get; set; }

    [DataMember] public SequenceType PolishMainType { get; set; }

    [DataMember] public bool PolishMainBSideChecked { get; set; }

    // Polish A-side operation settings
    [DataMember] public bool PolishASideOperationAChecked { get; set; }

    [DataMember] public SequenceType PolishASideOperationType { get; set; }

    [DataMember] public bool PolishASideOperationBChecked { get; set; }

    // Polish B-side operation settings
    [DataMember] public bool PolishBSideOperationCChecked { get; set; }

    [DataMember] public SequenceType PolishBSideOperationType { get; set; }

    [DataMember] public bool PolishBSideOperationDChecked { get; set; }

    // Clean main operation settings
    [DataMember] public bool CleanMainASideChecked { get; set; }

    [DataMember] public SequenceType CleanMainType { get; set; }

    [DataMember] public bool CleanMainBSideChecked { get; set; }

    // Clean A-side roller operation settings
    [DataMember] public bool CleanASideRollerOp1Checked { get; set; }

    [DataMember] public SequenceType CleanASideRollerOp1Type { get; set; }

    [DataMember] public bool CleanASideRollerOp2Checked { get; set; }

    [DataMember] public SequenceType CleanASideRollerOp2Type { get; set; }

    [DataMember] public bool CleanASideRollerOp3Checked { get; set; }

    // Clean A-side pen operation settings
    [DataMember] public bool CleanASidePenOp1Checked { get; set; }

    //[DataMember] public SequenceType CleanASidePenOp1Type { get; set; }

    //[DataMember] public bool CleanASidePenOp2Checked { get; set; }

    // Clean A-side dry operation setting
    [DataMember] public bool CleanASideDryOpChecked { get; set; }

    // Clean B-side roller operation settings
    [DataMember] public bool CleanBSideRollerOp1Checked { get; set; }

    [DataMember] public SequenceType CleanBSideRollerOp1Type { get; set; }

    [DataMember] public bool CleanBSideRollerOp2Checked { get; set; }

    [DataMember] public SequenceType CleanBSideRollerOp2Type { get; set; }

    [DataMember] public bool CleanBSideRollerOp3Checked { get; set; }

    // Clean B-side pen operation settings
    [DataMember] public bool CleanBSidePenOp1Checked { get; set; }

    //[DataMember] public SequenceType CleanBSidePenOp1Type { get; set; }

    //[DataMember] public bool CleanBSidePenOp2Checked { get; set; }

    // Clean B-side dry operation setting
    [DataMember] public bool CleanBSideDryOpChecked { get; set; }

    // Recipe selections for future implementation
    [DataMember] public List<RecipeSelection?>? RecipeSelections { get; set; }
}