#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using CMP.Constants.Enums;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.ViewModels.Tabs.MainTab.Content;
using CMP.ViewModels.Tabs.OperationTab;
using CMP.Views.Tabs.MainTab.Content;
using CMP.Views.Tabs.OperationTab;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPFLocalizeExtension.Engine;
using static CMP.Tests.Services.LiquidControlTests;

namespace CMP.Tests.Services;

[TestClass]
[DoNotParallelize]
public class PolishLiquidControlTests
{
    private sealed record Write(int Socket, KHLDevType Device, uint Address, uint Words, byte[] Data);

    private static readonly (WorkstationType Chamber, int Socket)[] Chambers =
    [
        (WorkstationType.PolishA, 101), (WorkstationType.PolishB, 202),
        (WorkstationType.PolishC, 303), (WorkstationType.PolishD, 404)
    ];

    // Independent transcription of the supplied command table, in command-code order.
    private static readonly (string Project, string Tag, int[] Codes)[] CommandTable =
    [
        ("Slurry1.ThreeWayTotalInletValve", "FV-POLA100A", [1189, 1190]),
        ("Slurry1.ThreeWayReturnValve", "FV-POLA100B", [1191, 1192]),
        ("Slurry1.ThreeWayInletValve", "FV-POLA100C", [1193, 1194]),
        ("Slurry1.WaterThreeWayInletValve", "FV-POLA100D", [1195, 1196]),
        ("Slurry1.OutletValve", "SBV-POLA100", [1197, 1198]),
        ("Slurry1.Flow", "FC-POLA100", [1199, 1200, 1201]),
        ("Slurry2.ThreeWayTotalInletValve", "FV-POLA102A", [1204, 1205]),
        ("Slurry2.ThreeWayReturnValve", "FV-POLA102B", [1206, 1207]),
        ("Slurry2.ThreeWayInletValve", "FV-POLA102C", [1208, 1209]),
        ("Slurry2.WaterThreeWayInletValve", "FV-POLA102D", [1210, 1211]),
        ("Slurry2.OutletValve", "SBV-POLA102", [1212, 1213]),
        ("Slurry2.Flow", "FC-POLA102", [1214, 1215, 1216]),
        ("Slurry3.ThreeWayTotalInletValve", "FV-POLA104A", [1219, 1220]),
        ("Slurry3.ThreeWayReturnValve", "FV-POLA104B", [1221, 1222]),
        ("Slurry3.ThreeWayInletValve", "FV-POLA104C", [1223, 1224]),
        ("Slurry3.WaterThreeWayInletValve", "FV-POLA104D", [1225, 1226]),
        ("Slurry3.OutletValve", "SBV-POLA104", [1227, 1228]),
        ("Slurry3.Flow", "FC-POLA104", [1229, 1230, 1231]),
        ("Slurry1", "", [1260, 1261]),
        ("Slurry2", "", [1262, 1263]),
        ("Slurry3", "", [1264, 1265]),
        ("Slurry1.Diw", "", [1266, 1267]),
        ("Slurry2.Diw", "", [1268, 1269]),
        ("Slurry3.Diw", "", [1270, 1271]),
        ("DIWOutletValve", "SBV-POLA106", [1272, 1273]),
        ("PlatenWashValve", "FV-POLA120", [1274, 1275]),
        ("MovingHeadDIWWashValve", "FV-POLA121A", [1276, 1277]),
        ("MovingHeadExternalDIWWashValve", "FV-POLA121B", [1278, 1279]),
        ("HeadChemicalWashValve", "FV-POLA121C", [1280, 1281]),
        ("ConditionerDIWWashValve", "FV-POLA122A", [1282, 1283]),
        ("ConditionerArmDIWWashValve", "FV-POLA122B", [1284, 1285]),
        ("ChemicalArmDIWWashValve", "FV-POLA123", [1286, 1287]),
        ("HeadMiddleWashValve", "FV-POLA124", [1288, 1289]),
        ("HeadExternalWashValve", "FV-POLA126", [1290, 1291]),
        ("WFUpperFaceChemicalWashValve", "FV-POLA127", [1292, 1293]),
        ("WFUpperFaceDIWWashValve", "FV-POLA128", [1294, 1295]),
        ("WFLowerFaceChemicalWashValve", "FV-POLA130", [1296, 1297]),
        ("WFLowerFaceDIWWashValve", "FV-POLA129", [1298, 1299]),
        ("PlatenCoolWaterValve", "FV-POLA165", [1300, 1301]),
        ("PlatenMotorCoolWaterValve", "FV-POLA167", [1302, 1303]),
        ("Zone4DIWValve", "SV-POLA148", [1304, 1305])
    ];

    private static readonly Dictionary<int, uint> Setpoints = new()
    {
        [1199] = 18172, [1214] = 18174, [1229] = 18176
    };

    private static GenericPolishLiquidContentViewModel CreateModel(
        WorkstationType chamber, List<Write> writes, int result = 0,
        Func<(KVLink PolishA, KVLink PolishB, KVLink PolishC, KVLink PolishD)>? getLinks = null,
        Action<bool, string>? notify = null) =>
        new(chamber, getLinks ?? (() =>
            (new KVLink("offline-a") { sock = 101 }, new KVLink("offline-b") { sock = 202 },
                new KVLink("offline-c") { sock = 303 }, new KVLink("offline-d") { sock = 404 })),
            (socket, device, address, words, data) =>
            {
                writes.Add(new Write(socket, device, address, words, data));
                return result;
            }, notify ?? ((_, _) => { }));

    [WpfTestMethod]
    public void ThreeParameters_MatchTableAndPreserveDecimalsOnAllFourPlcs()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var model = CreateModel(chamber.Chamber, writes);
            Assert.AreEqual(3, model.Parameters.Count);
            CollectionAssert.AreEqual(new[] { 18172u, 18174u, 18176u }, model.Parameters.Select(p => p.Handle).ToArray());
            for (var i = 0; i < 3; i++)
            {
                var parameter = model.Parameters[i];
                Assert.AreSame(PlcHandleHelper.GetParamByPath($"CmdPolish.Liquid.Slurry{i + 1}.Param.Flowrate"), parameter.Metadata);
                Assert.AreEqual(100d, parameter.Metadata.NumericDefaultValue);
                Assert.AreEqual(100, parameter.Metadata.DefaultValue);
                Assert.AreEqual(0d, parameter.MinValue);
                Assert.AreEqual(1000d, parameter.MaxValue);
                Assert.AreEqual(string.Empty, parameter.DrawingTag);
                Assert.AreEqual("mL/min", parameter.Unit);
                Assert.AreEqual("ZF", parameter.Metadata.ZrZf);
                Assert.AreEqual(2, parameter.DecimalPlaces);
                Assert.AreEqual(2, parameter.WordCount);
                Assert.AreEqual($"Slurry Line{i + 1}/DIW Flowrate", parameter.EnglishName);
                Assert.AreEqual($"Slurry Line{i + 1}/DIW流量", parameter.ChineseName);
                foreach (var input in new[] { "0.00", "123.456", "1000.00" })
                {
                    writes.Clear();
                    parameter.InputValue = input;
                    parameter.SetCommand.Execute();
                    var write = writes.Single();
                    var expected = input == "123.456" ? 123.46f : float.Parse(input, CultureInfo.InvariantCulture);
                    Assert.AreEqual(chamber.Socket, write.Socket);
                    Assert.AreEqual(KHLDevType.DEV_ZF, write.Device);
                    Assert.AreEqual(parameter.Handle, write.Address);
                    Assert.AreEqual(2u, write.Words);
                    Assert.AreEqual(expected, BitConverter.ToSingle(write.Data, 0));
                    Assert.AreEqual(expected.ToString("F2", CultureInfo.CurrentCulture), parameter.InputValue);
                }
                writes.Clear();
                parameter.InputValue = "123.45";
                parameter.SetCommand.Execute();
                CollectionAssert.AreEqual(new byte[] { 0x66, 0xE6, 0xF6, 0x42 }, writes.Single().Data.Take(4).ToArray());
            }
        }
        // Existing Operation fields and extension methods remain compatible.
        Assert.AreEqual(18172u, PlcHandle.CmdPolish.Liquid.Slurry1.Param.Flowrate.ParamHandle());
        Assert.AreEqual(18174u, PlcHandle.CmdPolish.Liquid.Slurry2.Param.Flowrate.ParamHandle());
        Assert.AreEqual(18176u, PlcHandle.CmdPolish.Liquid.Slurry3.Param.Flowrate.ParamHandle());
    }

    [WpfTestMethod]
    public void All85Commands_MatchTableAndWriteToSelectedPlcW50()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var model = CreateModel(chamber.Chamber, writes);
            var commands = model.CommandGroups.SelectMany(g => g.Commands).ToArray();
            Assert.AreEqual(41, model.CommandGroups.Count);
            Assert.AreEqual(85, commands.Length);
            Assert.AreEqual(85, commands.Select(c => c.CommandKey).Distinct().Count());
            CollectionAssert.AreEqual(CommandTable.SelectMany(g => g.Codes).ToArray(), commands.Select(c => c.CommandCode).ToArray());
            var plcPaths = PlcHandleHelper.GetValuesByHandle(Handle.CmdPolish).Keys
                .Where(p => p.StartsWith("CmdPolish.Liquid.")).ToArray();
            CollectionAssert.AreEquivalent(plcPaths, commands.Select(c => c.CommandPath).ToArray());
            foreach (var parameter in model.Parameters) parameter.InputValue = "234.56";
            for (var i = 0; i < CommandTable.Length; i++)
            {
                var expected = CommandTable[i];
                var group = model.CommandGroups[i];
                Assert.AreEqual(expected.Tag, group.PidTag);
                Assert.AreNotEqual(group.ProjectKey, group.EnglishTitle);
                Assert.AreNotEqual(group.ProjectKey, group.ChineseTitle);
                foreach (var command in group.Commands)
                {
                    var action = expected.Codes.Length == 3
                        ? (command.CommandCode == expected.Codes[0] ? "Flowrate"
                            : command.CommandCode == expected.Codes[1] ? "FlowrateReset" : "MFCCalibration")
                        : command.CommandCode == expected.Codes[0] ? "Open" : "Close";
                    Assert.AreEqual($"CmdPolish.Liquid.{expected.Project}.{action}", command.CommandPath);
                    Assert.AreEqual(0x50u, command.Handle);
                    var hasSetpoint = Setpoints.TryGetValue(command.CommandCode, out var offset);
                    Assert.AreEqual(hasSetpoint ? (uint?)offset : null, command.ParameterHandle);
                    writes.Clear();
                    command.ExecuteCommand.Execute();
                    Assert.AreEqual(hasSetpoint ? 2 : 1, writes.Count);
                    Assert.IsTrue(writes.All(w => w.Socket == chamber.Socket));
                    if (hasSetpoint)
                    {
                        Assert.AreEqual(KHLDevType.DEV_ZF, writes[0].Device);
                        Assert.AreEqual(offset, writes[0].Address);
                        Assert.AreEqual(2u, writes[0].Words);
                        Assert.AreEqual(234.56f, BitConverter.ToSingle(writes[0].Data, 0));
                    }
                    var write = writes.Last();
                    Assert.AreEqual(KHLDevType.DEV_W, write.Device);
                    Assert.AreEqual(0x50u, write.Address);
                    Assert.AreEqual(1u, write.Words);
                    Assert.AreEqual(command.CommandCode, (int)BitConverter.ToUInt16(write.Data, 0));
                    CollectionAssert.AreEqual(command.CommandCode.Value(), write.Data);
                }
            }
        }
    }

    [WpfTestMethod]
    public void InvalidParametersAndWriteFailures_BlockLinkedFlowCommands()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var notifications = new List<bool>();
            var model = CreateModel(chamber.Chamber, writes, notify: (success, _) => notifications.Add(success));
            foreach (var parameter in model.Parameters)
            {
                var flow = model.CommandGroups.SelectMany(g => g.Commands).Single(c => c.ParameterHandle == parameter.Handle);
                foreach (var input in new[] { "", "abc", "NaN", "Infinity", "-Infinity", "1e999", "-0.01", "1000.01" })
                {
                    parameter.InputValue = input;
                    parameter.SetCommand.Execute();
                    flow.ExecuteCommand.Execute();
                    Assert.AreEqual(0, writes.Count, input);
                }
            }
            Assert.IsTrue(notifications.Count > 0 && notifications.All(success => !success));
            model = CreateModel(chamber.Chamber, writes, -1, notify: (success, _) => notifications.Add(success));
            foreach (var command in model.CommandGroups.SelectMany(g => g.Commands))
            {
                writes.Clear();
                command.ExecuteCommand.Execute();
                Assert.AreEqual(1, writes.Count);
                Assert.AreEqual(command.ParameterHandle.HasValue ? KHLDevType.DEV_ZF : KHLDevType.DEV_W, writes[0].Device);
            }
            Assert.IsTrue(notifications.All(success => !success));
        }
    }

    [WpfTestMethod]
    public void ConstructionStaysOfflineAndReconnectionUsesCurrentChamberSocket()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var linkReads = 0;
            var socketShift = 0;
            var model = CreateModel(chamber.Chamber, writes, getLinks: () =>
            {
                linkReads++;
                return (new KVLink("offline-a") { sock = 101 + socketShift },
                    new KVLink("offline-b") { sock = 202 + socketShift },
                    new KVLink("offline-c") { sock = 303 + socketShift },
                    new KVLink("offline-d") { sock = 404 + socketShift });
            });
            Assert.AreEqual(0, linkReads);
            Assert.AreEqual(0, writes.Count);
            model.Parameters[0].SetCommand.Execute();
            Assert.AreEqual(chamber.Socket, writes.Single().Socket);
            socketShift = 500;
            foreach (var flow in model.CommandGroups.SelectMany(g => g.Commands).Where(c => c.ParameterHandle.HasValue))
            {
                writes.Clear();
                flow.ExecuteCommand.Execute();
                Assert.AreEqual(2, writes.Count);
                Assert.IsTrue(writes.All(w => w.Socket == chamber.Socket + 500));
            }
            var errors = new List<bool>();
            model = CreateModel(chamber.Chamber, writes, getLinks: () => throw new InvalidOperationException("offline"),
                notify: (success, _) => errors.Add(success));
            writes.Clear();
            model.Parameters[0].SetCommand.Execute();
            model.CommandGroups[0].Commands[0].ExecuteCommand.Execute();
            Assert.AreEqual(0, writes.Count);
            CollectionAssert.AreEqual(new[] { false, false }, errors);
        }
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => CreateModel(WorkstationType.PenA1, new List<Write>()));
    }

    [WpfTestMethod]
    public void FourTabs_HaveIndependentLocalizedParametersAndCommands()
    {
        var original = LocalizeDictionary.Instance.Culture;
        try
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");
            var polish = new LiquidContentViewModel().Tabs[0];
            Assert.IsFalse(polish.IsEmpty);
            CollectionAssert.AreEqual(new[] { "PolishA", "PolishB", "PolishC", "PolishD" }, polish.Chambers.Select(c => c.HeaderKey).ToArray());
            var models = polish.Chambers.Select(c => (GenericPolishLiquidContentViewModel)c.Content).ToArray();
            Assert.AreEqual(4, models.Distinct().Count());
            models[0].Parameters[0].InputValue = "123.45";
            Assert.IsTrue(models.Skip(1).All(m => m.Parameters[0].InputValue == 100d.ToString("F2", CultureInfo.CurrentCulture)));
            foreach (var language in new[] { "zh", "en", "zh" })
            {
                var culture = CultureInfo.GetCultureInfo(language);
                LocalizeDictionary.Instance.Culture = culture;
                foreach (var tab in polish.Chambers)
                    Assert.AreEqual(Resources.ResourceManager.GetString(tab.HeaderKey, culture), tab.Header);
                foreach (var model in models)
                {
                    foreach (var parameter in model.Parameters)
                        Assert.AreEqual(language == "zh" ? parameter.ChineseName : parameter.EnglishName, parameter.Name);
                    foreach (var group in model.CommandGroups)
                    {
                        Assert.AreEqual(language == "zh" ? group.ChineseTitle : group.EnglishTitle, group.Title);
                        foreach (var command in group.Commands)
                        {
                            Assert.AreEqual(language == "zh" ? command.ChineseActionName : command.EnglishActionName, command.ActionName);
                            Assert.AreNotEqual(command.Definition.ActionKey, command.ActionName);
                        }
                    }
                }
            }
            Assert.AreEqual("123.45", models[0].Parameters[0].InputValue);
            var first = models[0];
            Assert.AreEqual("R.R Cleaning Valve", first.CommandGroups.Single(g => g.Commands[0].CommandCode == 1288).EnglishTitle);
            Assert.AreEqual("Release DIW Wash Valve", first.CommandGroups.Single(g => g.Commands[0].CommandCode == 1290).EnglishTitle);
            Assert.AreEqual("Gap Cleaning Valve", first.CommandGroups.Single(g => g.Commands[0].CommandCode == 1296).EnglishTitle);
            Assert.AreEqual("T.R Cleaning Valve", first.CommandGroups.Single(g => g.Commands[0].CommandCode == 1298).EnglishTitle);
        }
        finally { LocalizeDictionary.Instance.Culture = original; }
    }

    [WpfTestMethod]
    public void ActualNestedViews_LoadAllFourChambersAndSwitchLanguage()
    {
        EnsureApplicationResources();
        var original = LocalizeDictionary.Instance.Culture;
        try
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");
            var model = new LiquidContentViewModel();
            var view = new LiquidContentView { DataContext = model };
            var categories = (TabControl)view.Content;
            categories.SelectedIndex = 0;
            Layout(view);
            var tabs = Descendants<TabControl>(categories).Single();
            Assert.AreEqual(4, tabs.Items.Count);
            for (var i = 0; i < 4; i++)
            {
                tabs.SelectedIndex = i;
                Layout(view);
                var polish = Descendants<GenericPolishLiquidContentView>(view).Single();
                var polishModel = (GenericPolishLiquidContentViewModel)model.Tabs[0].Chambers[i].Content;
                Assert.AreSame(polishModel, polish.DataContext);
                Assert.AreEqual(3, Descendants<TextBox>(polish).Count());
                Assert.AreEqual(88, Descendants<Button>(polish).Count());
                foreach (var language in new[] { "en", "zh", "en" })
                {
                    var culture = CultureInfo.GetCultureInfo(language);
                    LocalizeDictionary.Instance.Culture = culture;
                    Layout(view);
                    var text = Descendants<TextBlock>(polish).Select(t => t.Text).ToArray();
                    CollectionAssert.Contains(text, Resources.ResourceManager.GetString("DrawingName", culture));
                    CollectionAssert.Contains(text, "mL/min");
                    foreach (var hidden in new[] { "Address", "Description", "CommandCode", "WordCount" })
                        CollectionAssert.DoesNotContain(text, Resources.ResourceManager.GetString(hidden, culture));
                    foreach (var parameter in polishModel.Parameters)
                    {
                        CollectionAssert.Contains(text, parameter.Name);
                        CollectionAssert.DoesNotContain(text, language == "zh" ? parameter.EnglishName : parameter.ChineseName);
                        CollectionAssert.DoesNotContain(text, parameter.Address);
                    }
                    foreach (var group in polishModel.CommandGroups)
                    {
                        CollectionAssert.Contains(text, group.Title);
                        if (group.PidTag.Length > 0) CollectionAssert.Contains(text, group.PidTag);
                        CollectionAssert.DoesNotContain(text, language == "zh" ? group.EnglishTitle : group.ChineseTitle);
                        foreach (var command in group.Commands)
                        {
                            CollectionAssert.DoesNotContain(text, command.Address);
                            CollectionAssert.DoesNotContain(text, command.CommandCode.ToString());
                        }
                    }
                    foreach (var button in Descendants<Button>(polish).Where(b => b.DataContext is LiquidCommandViewModel))
                    {
                        var command = (LiquidCommandViewModel)button.DataContext;
                        Assert.AreEqual(command.ActionName, button.Content);
                        var style = command.Definition.ActionKey switch
                        {
                            "ActionClose" => "ButtonDanger",
                            "ActionFlowrateReset" => "ButtonWarning",
                            "ActionMfcCalibration" => "ButtonInfo",
                            _ => "ButtonPrimary"
                        };
                        Assert.AreSame(polish.FindResource(style), button.Style);
                    }
                }
            }
        }
        finally { LocalizeDictionary.Instance.Culture = original; }
    }
}
