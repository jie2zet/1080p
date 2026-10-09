#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Clean;
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
public class PenLiquidControlTests
{
    private sealed record Write(int Socket, KHLDevType Device, uint Address, uint Words, byte[] Data);

    private static readonly (string Name, Func<GenericPenCleanConfig> Config, uint Handle, int Socket, string Ip)[] Chambers =
    [
        ("PenA1", () => PenCleanConfigFactory.CreatePenA1Config(), 0x72u, 101, "192.168.2.30"),
        ("PenB1", () => PenCleanConfigFactory.CreatePenB1Config(), 0x78u, 202, "192.168.2.32")
    ];

    private static readonly (uint Offset, double Initial, double Min, double Max, string Tag,
        string Unit, string Path, string English, string Chinese)[] ParameterTable =
    [
        (18120, 2000, 0, 3500, "FC-PEAC103", "mL/min", "Liquid.DIWFlow.Param.FlowrateSet", "DIW Inlet Flowrate Set", "纯水流量设定(混合前)"),
        (18122, 600, 0, 2000, "FC-PEAC104", "mL/min", "Liquid.WFUpperFaceDIWFlow.Param.FlowrateSet", "WF Upper Face DIW Flowrate Set", "晶圆上表面纯水流量设定"),
        (18124, 300, 50, 500, "EV-FJ102", "kPa", "Liquid.DIWPressure.Param.PressureSet", "DIW Pressure Set", "碳酸水混合前纯水压力设定"),
        (18126, 600, 0, 2000, "FC-PEAC106", "mL/min", "Liquid.WFLowerFaceDIWFlow.Param.FlowrateSet", "WF Lower Face DIW Flowrate Set", "晶圆下表面纯水流量设定"),
        (18128, 150, 15, 300, "FC-PEAC102", "mL/min", "Liquid.ChemicalFlow.Param.FlowrateSet", "Chemical Flowrate Set", "药液流量设定(混合前)"),
        (18150, 450, 5, 900, "EV-FJ103", "kPa", "Gas.CO2Pressure.Param.PressureSet", "CO2 Pressure Set", "CO2压力设定"),
        (18152, 200, 0, 500, "MFC-FJAC101", "L/min", "Gas.N2Flow.Param.FlowrateSet", "WF Upper Face N2 Purge Flowrate Set", "晶圆上表面氮气流量设定")
    ];

    private static readonly (string Project, string Tag, int[] Codes)[] CommandTable =
    [
        ("ChemicalInletValve", "SV-PEAC102A", [1279, 1280]),
        ("ChemicalOutletValve", "SV-PEAC102B", [1281, 1282]),
        ("ChemicalFlow", "FC-PEAC102", [1283, 1284, 1285]),
        ("DIWInletValve", "SV-PReg103", [1288, 1289]),
        ("DIWOutletValve", "SV-PEAC103", [1290, 1291]),
        ("DIWFlow", "FC-PEAC103", [1292, 1293, 1294]),
        ("WFUpperFaceDIWWashValve", "SV-PEAC104", [1297, 1298]),
        ("WFUpperFaceDIWFlow", "FC-PEAC104", [1299, 1300, 1301]),
        ("WFLowerFaceDIWWashValve", "SV-PEAC106", [1304, 1305]),
        ("WFLowerFaceDIWFlow", "FC-PEAC106", [1306, 1307, 1308]),
        ("DIWSupplyValve", "SV-FJ102", [1311, 1312]),
        ("DIWPressure", "EV-FJ102", [1313, 1314, 1315]),
        ("ChemicalInletValve2", "SV-PEAC102F", [1318, 1319]),
        ("ChemicalInletValve3", "SV-PE1AC102Aa", [1320, 1321]),
        ("PenDIWWashValve", "SV-PEAC100", [1346, 1347]),
        ("QuartzDIWWashValve", "SV-PEAC101A", [1348, 1349]),
        ("ChemicalPipelineWashValve", "SV-PEAC102Ab", [1350, 1351]),
        ("QuartzChemicalWashValve", "SV-PEAC102C", [1352, 1353]),
        ("WFUpperLowerFacesChemicalWashValve", "SV-PEAC102D", [1354, 1355]),
        ("WFUpperFacesChemicalWashValve", "SV-PEAC102E", [1356, 1357]),
        ("DIWChamberValve", "SV-PEANC120", [1358, 1359]),
        ("WFUpperFaceDICO2WashValve", "SV-FJAC102", [1360, 1361]),
        ("Flush", "", [1370, 1371])
    ];

    private static readonly Dictionary<int, uint> LinkedParameters = new()
    {
        [1283] = 18128, [1292] = 18120, [1299] = 18122, [1306] = 18126, [1313] = 18124
    };

    private static GenericPenLiquidContentViewModel CreateModel(
        GenericPenCleanConfig config, List<Write> writes, int result = 0,
        Func<(KVLink PenA, KVLink PenB)>? getLinks = null, Action<bool, string>? notify = null) =>
        new(config, getLinks ?? (() =>
            (new KVLink("offline-a") { sock = 101 }, new KVLink("offline-b") { sock = 202 })),
            (socket, device, address, words, data) =>
            {
                writes.Add(new Write(socket, device, address, words, data));
                return result;
            }, notify ?? ((_, _) => { }));

    [WpfTestMethod]
    public void Parameters_MatchSuppliedTableAndWriteFloatsOnEachPlc()
    {
        foreach (var chamber in Chambers)
        {
            var config = chamber.Config();
            var writes = new List<Write>();
            var model = CreateModel(config, writes);
            Assert.AreEqual(7, model.Parameters.Count);
            CollectionAssert.AreEqual(ParameterTable.Select(p => p.Offset).ToArray(), model.Parameters.Select(p => p.Handle).ToArray());
            foreach (var expected in ParameterTable)
            {
                var parameter = model.Parameters.Single(p => p.Handle == expected.Offset);
                Assert.AreSame(PlcHandleHelper.GetParamByPath($"{config.PlcHandleTypeName}.{expected.Path}"), parameter.Metadata);
                Assert.AreEqual(expected.Initial, parameter.Metadata.NumericDefaultValue);
                Assert.AreEqual((int)expected.Initial, parameter.Metadata.DefaultValue);
                Assert.AreEqual(expected.Min, parameter.MinValue);
                Assert.AreEqual(expected.Max, parameter.MaxValue);
                Assert.AreEqual(expected.Tag, parameter.DrawingTag);
                Assert.AreEqual(expected.Unit, parameter.Unit);
                Assert.AreEqual(expected.English, parameter.EnglishName);
                Assert.AreEqual(expected.Chinese, parameter.ChineseName);
                Assert.AreEqual("ZF", parameter.Metadata.ZrZf);
                Assert.AreEqual(2, parameter.WordCount);
                Assert.AreEqual(2, parameter.DecimalPlaces);
                Assert.AreEqual(expected.Initial.ToString("F2", CultureInfo.CurrentCulture), parameter.InputValue);

                foreach (var value in new[] { expected.Min, expected.Initial + 0.456, expected.Max })
                {
                    writes.Clear();
                    parameter.InputValue = value.ToString(CultureInfo.InvariantCulture);
                    parameter.SetCommand.Execute();
                    var write = writes.Single();
                    var rounded = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                    Assert.AreEqual(chamber.Socket, write.Socket);
                    Assert.AreEqual(KHLDevType.DEV_ZF, write.Device);
                    Assert.AreEqual(expected.Offset, write.Address);
                    Assert.AreEqual(2u, write.Words);
                    Assert.AreEqual((float)rounded, BitConverter.ToSingle(write.Data, 0));
                    Assert.AreEqual(rounded.ToString("F2", CultureInfo.CurrentCulture), parameter.InputValue);
                }
            }
            Assert.AreEqual(600, config.Params.Liquid.WFLowerFaceDIWFlow.DefaultValue);
            Assert.AreEqual(0, config.Params.Liquid.DIWFlow.MinValue);
        }
    }

    [WpfTestMethod]
    public void All51Commands_UseOwnHandleAndPlcWithCorrectSetpointFirst()
    {
        foreach (var chamber in Chambers)
        {
            var config = chamber.Config();
            Assert.AreEqual(chamber.Ip, config.PlcAddress);
            var writes = new List<Write>();
            var model = CreateModel(config, writes);
            var commands = model.CommandGroups.SelectMany(g => g.Commands).ToArray();
            Assert.AreEqual(51, commands.Length);
            Assert.AreEqual(23, model.CommandGroups.Count);
            CollectionAssert.AreEqual(CommandTable.SelectMany(g => g.Codes).ToArray(), commands.Select(c => c.CommandCode).ToArray());
            var plcCommandPaths = PlcHandleHelper.GetValuesByHandle(chamber.Handle).Keys
                .Where(path => path.StartsWith($"{config.PlcHandleTypeName}.Liquid.")
                    || path.StartsWith($"{config.PlcHandleTypeName}.Flush.")).ToArray();
            CollectionAssert.AreEquivalent(plcCommandPaths, commands.Select(c => c.CommandPath).ToArray(), chamber.Name);
            for (var i = 0; i < CommandTable.Length; i++)
            {
                var expected = CommandTable[i];
                var group = model.CommandGroups[i];
                Assert.AreEqual(expected.Tag, group.PidTag);
                Assert.AreNotEqual(group.ProjectKey, group.EnglishTitle);
                Assert.AreNotEqual(group.ProjectKey, group.ChineseTitle);
                foreach (var command in group.Commands)
                {
                    writes.Clear();
                    var linked = LinkedParameters.TryGetValue(command.CommandCode, out var offset);
                    Assert.AreEqual(linked ? (uint?)offset : null, command.ParameterHandle);
                    command.ExecuteCommand.Execute();
                    Assert.AreEqual(linked ? 2 : 1, writes.Count, command.CommandKey);
                    if (linked)
                    {
                        Assert.AreEqual(chamber.Socket, writes[0].Socket);
                        Assert.AreEqual(KHLDevType.DEV_ZF, writes[0].Device);
                        Assert.AreEqual(offset, writes[0].Address);
                        Assert.AreEqual(2u, writes[0].Words);
                        Assert.AreEqual((float)model.Parameters.Single(p => p.Handle == offset).Metadata.NumericDefaultValue,
                            BitConverter.ToSingle(writes[0].Data, 0));
                    }
                    var write = writes.Last();
                    Assert.AreEqual(chamber.Socket, write.Socket);
                    Assert.AreEqual(KHLDevType.DEV_W, write.Device);
                    Assert.AreEqual(chamber.Handle, write.Address);
                    Assert.AreEqual(1u, write.Words);
                    Assert.AreEqual((ushort)command.CommandCode, BitConverter.ToUInt16(write.Data, 0));
                    Assert.AreEqual(chamber.Handle, command.Handle);
                    Assert.AreEqual(expected.Tag, command.PidTag);
                    Assert.AreNotEqual(command.Definition.ActionKey, command.EnglishActionName);
                    Assert.AreNotEqual(command.Definition.ActionKey, command.ChineseActionName);
                    var path = $"{config.PlcHandleTypeName}." +
                        (expected.Project == "Flush" ? "" : "Liquid.") + command.CommandKey.Replace('_', '.');
                    Assert.AreEqual(path, command.CommandPath);
                    Assert.AreEqual(chamber.Handle, PlcHandleHelper.GetHandleByPath(path));
                    Assert.AreEqual(command.CommandCode, PlcHandleHelper.GetValueByPath(path));
                    var legacyCommand = config.GetLiquidCommand(command.CommandKey);
                    Assert.AreEqual(command.Handle, legacyCommand.handle);
                    CollectionAssert.AreEqual(legacyCommand.value, write.Data);
                }
            }
            Assert.AreEqual(1370, BitConverter.ToInt32(config.GetFlushCommand("Open").value, 0));
            Assert.AreEqual(1314, BitConverter.ToInt32(config.GetPressureCommand("DIWPressure_Reset").value, 0));
            Assert.ThrowsException<ArgumentException>(() => config.GetLiquidCommand("ChemicalInletValve4_Open"));
        }
    }

    [WpfTestMethod]
    public void InvalidValuesAndFailedSetpointWrites_BlockExecution()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var notifications = new List<bool>();
            var model = CreateModel(chamber.Config(), writes, notify: (success, _) => notifications.Add(success));
            foreach (var parameter in model.Parameters)
            {
                var linked = model.CommandGroups.SelectMany(g => g.Commands).SingleOrDefault(c => c.ParameterHandle == parameter.Handle);
                foreach (var input in new[] { "", "abc", "NaN", "Infinity", "-Infinity", "1e999",
                    (parameter.MinValue - 0.01).ToString(CultureInfo.InvariantCulture),
                    (parameter.MaxValue + 0.01).ToString(CultureInfo.InvariantCulture) })
                {
                    parameter.InputValue = input;
                    parameter.SetCommand.Execute();
                    linked?.ExecuteCommand.Execute();
                    Assert.AreEqual(0, writes.Count, $"Invalid value {input} at {parameter.Address}");
                }
            }
            Assert.IsTrue(notifications.Count > 0 && notifications.All(success => !success));

            model = CreateModel(chamber.Config(), writes, -1, notify: (success, _) => notifications.Add(success));
            foreach (var parameter in model.Parameters)
            {
                writes.Clear();
                parameter.SetCommand.Execute();
                Assert.AreEqual(KHLDevType.DEV_ZF, writes.Single().Device);
            }
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
    public void ConstructionStaysOfflineAndWritesUseReconnectedSocket()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var a = new KVLink("offline-a") { sock = 101 };
            var b = new KVLink("offline-b") { sock = 202 };
            var reads = 0;
            var model = CreateModel(chamber.Config(), writes, getLinks: () =>
            {
                reads++;
                return (a, b);
            });
            Assert.AreEqual(0, reads);
            Assert.AreEqual(0, writes.Count);
            model.Parameters[0].SetCommand.Execute();
            Assert.AreEqual(chamber.Socket, writes.Single().Socket);
            a = new KVLink("reconnected-a") { sock = 303 };
            b = new KVLink("reconnected-b") { sock = 404 };
            foreach (var command in model.CommandGroups.SelectMany(g => g.Commands).Where(c => c.ParameterHandle.HasValue))
            {
                writes.Clear();
                command.ExecuteCommand.Execute();
                Assert.AreEqual(2, writes.Count);
                Assert.IsTrue(writes.All(w => w.Socket == (chamber.Socket == 101 ? 303 : 404)));
            }
            var errors = new List<bool>();
            model = CreateModel(chamber.Config(), writes,
                getLinks: () => throw new InvalidOperationException("offline"),
                notify: (success, _) => errors.Add(success));
            writes.Clear();
            model.Parameters[0].SetCommand.Execute();
            model.CommandGroups[0].Commands[0].ExecuteCommand.Execute();
            Assert.AreEqual(0, writes.Count);
            CollectionAssert.AreEqual(new[] { false, false }, errors);
        }
    }

    [WpfTestMethod]
    public void PenTabs_ContainOnlyA1B1WithIndependentLocalizedState()
    {
        var culture = LocalizeDictionary.Instance.Culture;
        try
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");
            var pen = new LiquidContentViewModel().Tabs[2];
            Assert.IsTrue(pen.HasChambers);
            CollectionAssert.AreEqual(new[] { "PenA1", "PenB1" }, pen.Chambers.Select(c => c.Header).ToArray());
            var models = pen.Chambers.Select(c => (GenericPenLiquidContentViewModel)c.Content).ToArray();
            models[0].Parameters[0].InputValue = "123.45";
            Assert.AreEqual(2000d.ToString("F2", CultureInfo.CurrentCulture), models[1].Parameters[0].InputValue);
            foreach (var language in new[] { "zh", "en", "zh" })
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(language);
                foreach (var model in models)
                {
                    foreach (var parameter in model.Parameters)
                        Assert.AreEqual(language == "zh" ? parameter.ChineseName : parameter.EnglishName, parameter.Name);
                    foreach (var group in model.CommandGroups)
                    {
                        Assert.AreEqual(language == "zh" ? group.ChineseTitle : group.EnglishTitle, group.Title);
                        foreach (var command in group.Commands)
                            Assert.AreEqual(language == "zh" ? command.ChineseActionName : command.EnglishActionName, command.ActionName);
                    }
                }
            }
            Assert.AreEqual("123.45", models[0].Parameters[0].InputValue);
        }
        finally { LocalizeDictionary.Instance.Culture = culture; }
    }

    [WpfTestMethod]
    public void View_LoadsBothNestedPenTabsAndSwitchesLanguageWithoutPlc()
    {
        EnsureApplicationResources();
        var original = LocalizeDictionary.Instance.Culture;
        try
        {
            var model = new LiquidContentViewModel();
            var view = new LiquidContentView { DataContext = model };
            var categories = (TabControl)view.Content;
            categories.SelectedIndex = 2;
            Layout(view);
            var tabs = Descendants<TabControl>(categories).Single();
            Assert.AreEqual(2, tabs.Items.Count);
            for (var i = 0; i < 2; i++)
            {
                tabs.SelectedIndex = i;
                Layout(view);
                var pen = Descendants<GenericPenLiquidContentView>(view).Single();
                var penModel = (GenericPenLiquidContentViewModel)model.Tabs[2].Chambers[i].Content;
                Assert.AreSame(penModel, pen.DataContext);
                Assert.AreEqual(7, Descendants<TextBox>(pen).Count());
                Assert.AreEqual(58, Descendants<Button>(pen).Count());
                foreach (var language in new[] { "en", "zh", "en" })
                {
                    var culture = CultureInfo.GetCultureInfo(language);
                    LocalizeDictionary.Instance.Culture = culture;
                    Layout(view);
                    var text = Descendants<TextBlock>(pen).Select(t => t.Text).ToArray();
                    CollectionAssert.Contains(text, Resources.ResourceManager.GetString("DrawingName", culture));
                    foreach (var hidden in new[] { "Address", "Description", "CommandCode", "WordCount" })
                        CollectionAssert.DoesNotContain(text, Resources.ResourceManager.GetString(hidden, culture));
                    foreach (var parameter in penModel.Parameters)
                    {
                        CollectionAssert.Contains(text, parameter.Name);
                        CollectionAssert.Contains(text, parameter.DrawingTag);
                        CollectionAssert.Contains(text, parameter.Unit);
                        CollectionAssert.DoesNotContain(text, language == "zh" ? parameter.EnglishName : parameter.ChineseName);
                        CollectionAssert.DoesNotContain(text, parameter.Address);
                    }
                    foreach (var group in penModel.CommandGroups)
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
                    foreach (var button in Descendants<Button>(pen).Where(b => b.DataContext is LiquidCommandViewModel))
                    {
                        var command = (LiquidCommandViewModel)button.DataContext;
                        Assert.AreEqual(command.ActionName, button.Content);
                        var style = command.Definition.ActionKey switch
                        {
                            "ActionClose" => "ButtonDanger",
                            "ActionFlowrateReset" or "ActionPressureReset" => "ButtonWarning",
                            "ActionFcCalibration" or "ActionEvCalibration" => "ButtonInfo",
                            _ => "ButtonPrimary"
                        };
                        Assert.AreSame(pen.FindResource(style), button.Style);
                    }
                }
            }
        }
        finally { LocalizeDictionary.Instance.Culture = original; }
    }
}
