#nullable enable

using System.Globalization;
using System.IO;
using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using CMP.Models.Resource;
using CMP.Services.Log;
using CMP.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Services;

[TestClass]
public class DataLogFormattingTests
{
    [TestMethod]
    public void TryBuildWaferLogName_UsesWaferNameAndOneBasedWaferId()
    {
        var wafer = new Wafer
        {
            WaferName = "LotA_2",
            LoadportID = 0,
            SlotID = 1
        };

        Assert.IsTrue(DataLogFormatting.TryBuildWaferLogName(wafer, out var waferLogName));
        Assert.AreEqual("LotA_2(C1W2)", waferLogName);

        wafer.WaferName = string.Empty;
        Assert.IsTrue(DataLogFormatting.TryBuildWaferLogName(wafer, out var waferIdOnlyLogName));
        Assert.AreEqual("(C1W2)", waferIdOnlyLogName);

        Assert.IsFalse(DataLogFormatting.TryBuildWaferLogName(null, out var unknownWaferLogName));
        Assert.AreEqual("Unknown(Unknown)", unknownWaferLogName);
    }

    [TestMethod]
    public void FormatSvHeader_UsesExactCommentNameAndUnitShape()
    {
        Assert.AreEqual(
            "FLC-POLA100_Slurry1 Or DIW Flowrate Act(mL/min)",
            DataLogFormatting.FormatSvHeader(new SV
            {
                Comment = "FLC-POLA100",
                Name = "Slurry1 Or DIW Flowrate Act",
                Unit = "mL/min"
            }));
        Assert.AreEqual(
            "_Recipe Time Act(s)",
            DataLogFormatting.FormatSvHeader(new SV { Name = "Recipe Time Act", Unit = "s" }));
        Assert.AreEqual(
            "Comment_Process Step Act()",
            DataLogFormatting.FormatSvHeader(new SV { Comment = "Comment", Name = "Process Step Act" }));
        Assert.AreEqual("_", DataLogFormatting.FormatSvHeader(null));
    }

    [TestMethod]
    public void ResolveSvMetadata_MatchesAllChamberNamingPatternsFromMemory()
    {
        var polish = new SV { SVID = 10004, Name = "Head Z-Axis Speed Act" };
        var roller = new SV { SVID = 12000, Name = "RollerA1 Process Step Act" };
        var pen = new SV { SVID = 15000, Name = "PenA1 Process Step Act" };
        var dry = new SV { SVID = 17000, Name = "DryA Process Step Act" };
        var index = DataLogFormatting.BuildMetadataIndex([polish, roller, pen, dry]);

        Assert.AreSame(polish, DataLogFormatting.ResolveSvMetadata(index, "PolishAHeadZ_AxisSpeedAct", "PolishA"));
        Assert.AreSame(roller, DataLogFormatting.ResolveSvMetadata(index, "RollerA_1ProcessStepAct", "RollerA1"));
        Assert.AreSame(pen, DataLogFormatting.ResolveSvMetadata(index, "PenA_1ProcessStepAct", "PenA1"));
        Assert.AreSame(dry, DataLogFormatting.ResolveSvMetadata(index, "DryAProcessStepAct", "DryA"));
        Assert.IsNull(DataLogFormatting.ResolveSvMetadata(index, "DryAUnconfiguredField", "DryA"));
    }

    [TestMethod]
    public void Polish_InitializesRuntimeKeysFromConfiguredValues()
    {
        var polish = new Polish("PolishD", new KVLink("127.0.0.1"));
        var configured = new SV
        {
            SVID = 11500,
            Name = "Process Step Act",
            Value = "0"
        };
        polish.ListSV[configured.Name] = configured;

        //polish.InitializeRuntimeValues();

        Assert.AreSame(configured, polish.GetSv("PolishDProcess Step Act"));
        Assert.AreEqual("0", polish.GetSv("PolishDProcess Step Act")?.Value);
    }

    [TestMethod]
    public void WaferNameValidationRule_RejectsEveryInvalidFileNameCharacter()
    {
        var rule = new WaferNameValidationRule();
        Assert.IsTrue(rule.Validate("LotA_2", CultureInfo.InvariantCulture).IsValid);
        Assert.IsTrue(rule.Validate(string.Empty, CultureInfo.InvariantCulture).IsValid);

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            Assert.IsFalse(
                rule.Validate($"Lot{invalidCharacter}A", CultureInfo.InvariantCulture).IsValid,
                $"Expected character code {(int)invalidCharacter} to be rejected.");
        }
    }
}
