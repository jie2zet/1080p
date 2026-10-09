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
public class DryLiquidControlTests
{
    private sealed record Write(int Socket, KHLDevType Device, uint Address, uint Words, byte[] Data);
    private sealed record Chamber(string Name, Func<GenericDryCleanConfig> Config, uint Handle, int Socket);
    private sealed record CommandGroup(string Project, string Tag, string[] Actions, int[] Codes);

    private static readonly Chamber[] Chambers =
    [
        new("DryA", () => DryCleanConfigFactory.CreateDryAConfig(), Handle.CmdDryA, 101),
        new("DryB", () => DryCleanConfigFactory.CreateDryBConfig(), Handle.CmdDryB, 202)
    ];

    private static readonly CommandGroup[] CommandTable =
    [
        new("WFUpperFaceDIWWashInletValve2", "SV-SDAC104", ["Open", "Close"], [1279, 1280]),
        new("WFUpperFaceDIWWashOutletValve2", "SBV-SDAC104", ["Open", "Close"], [1281, 1282]),
        new("WFUpperFaceDIWWashFlow2", "FC-SDAC104", ["Flowrate", "FlowrateReset", "FCCalibration"], [1283, 1284, 1285]),
        new("IPASupplyValve", "FC-IPAC101", ["Open", "Close"], [1344, 1345]),
        new("WFUpperFaceDIWWashInletValve1", "SV-SDAC103", ["Open", "Close"], [1346, 1347]),
        new("WFUpperFaceDIWWashOutletValve1", "SBV-SDAC103", ["Open", "Close"], [1348, 1349]),
        new("WFLowerFaceDIWWashValve", "SV-SDAC105", ["Open", "Close"], [1350, 1351])
    ];

    private static GenericDryLiquidContentViewModel CreateModel(
        GenericDryCleanConfig config,
        List<Write> writes,
        int result = 0,
        Func<(KVLink DryA, KVLink DryB)>? getLinks = null,
        Action<bool, string>? notify = null) =>
        new(config, getLinks ?? (() =>
            (new KVLink("offline-a") { sock = 101 }, new KVLink("offline-b") { sock = 202 })),
            (socket, device, address, words, data) =>
            {
                writes.Add(new Write(socket, device, address, words, data));
                return result;
            }, notify ?? ((_, _) => { }));

    [WpfTestMethod]
    public void Parameter_MatchesSuppliedMetadataAndWritesFloatToBothPlcs()
    {
        foreach (var chamber in Chambers)
        {
            var config = chamber.Config();
            Assert.AreEqual(chamber.Name == "DryA" ? "CmdDryA" : "CmdDryB", config.PlcHandleTypeName);
            var writes = new List<Write>();
            var model = CreateModel(config, writes);
            var parameter = model.Parameters.Single();
            Assert.AreSame(PlcHandleHelper.GetParamByPath($"{config.PlcHandleTypeName}.Liquid.WFUpperFaceDIWWashFlow2.Param.FlowrateSet"), parameter.Metadata);
            Assert.AreEqual(18100u, parameter.Handle);
            Assert.AreEqual("ZF18100", parameter.Address);
            Assert.AreEqual("FC-SDAC104", parameter.DrawingTag);
            Assert.AreEqual("mL/min", parameter.Unit);
            Assert.AreEqual(100d, parameter.Metadata.NumericDefaultValue);
            Assert.AreEqual(100, parameter.Metadata.DefaultValue);
            Assert.AreEqual(0d, parameter.MinValue);
            Assert.AreEqual(1000d, parameter.MaxValue);
            Assert.AreEqual(2, parameter.DecimalPlaces);
            Assert.AreEqual(2, parameter.WordCount);
            Assert.AreEqual("ZF", parameter.Metadata.ZrZf);
            Assert.AreEqual("Outer Rinse Flowrate", parameter.EnglishName);
            Assert.AreEqual("晶圆上表面外侧清洗流量设定", parameter.ChineseName);
            Assert.AreEqual("100.00", parameter.InputValue);

            foreach (var (input, expected) in new[] { ("0.00", 0f), ("123.456", 123.46f), ("1000.00", 1000f) })
            {
                writes.Clear();
                parameter.InputValue = input;
                parameter.SetCommand.Execute();
                var write = writes.Single();
                Assert.AreEqual(chamber.Socket, write.Socket);
                Assert.AreEqual(KHLDevType.DEV_ZF, write.Device);
                Assert.AreEqual(18100u, write.Address);
                Assert.AreEqual(2u, write.Words);
                Assert.AreEqual(expected, BitConverter.ToSingle(write.Data, 0));
                Assert.AreEqual(expected.ToString("F2", CultureInfo.CurrentCulture), parameter.InputValue);
            }

            foreach (var invalid in new[] { "", "not a number", "NaN", "Infinity", "-0.01", "1000.01" })
            {
                writes.Clear();
                parameter.InputValue = invalid;
                parameter.SetCommand.Execute();
                Assert.AreEqual(0, writes.Count, $"{chamber.Name}: {invalid}");
            }
        }
    }

    [WpfTestMethod]
    public void Commands_MatchAllFifteenSuppliedCodesAndDrawingTags()
    {
        foreach (var chamber in Chambers)
        {
            var typeName = chamber.Name == "DryA" ? "CmdDryA" : "CmdDryB";
            var commands = PlcHandleHelper.GetLiquidCommands(typeName);
            var expected = CommandTable.SelectMany(group => group.Actions.Select((action, index) =>
                (group, action, code: group.Codes[index]))).OrderBy(item => item.code).ToArray();
            Assert.AreEqual(15, commands.Count, chamber.Name);

            foreach (var item in expected)
            {
                var command = commands.Single(c => c.CommandCode == item.code);
                Assert.AreEqual($"{typeName}.Liquid.{item.group.Project}.{item.action}", command.CommandPath);
                Assert.AreEqual(item.group.Project == "WFUpperFaceDIWWashInletValve2"
                    ? "DryWfUpperFaceDiwWashInletValve2"
                    : item.group.Project == "WFUpperFaceDIWWashOutletValve2"
                        ? "DryWfUpperFaceDiwWashOutletValve2"
                        : item.group.Project == "WFUpperFaceDIWWashFlow2"
                            ? "DryWfUpperFaceDiwWashFlow2"
                            : item.group.Project == "IPASupplyValve"
                                ? "DryIpaSupplyValve"
                                : item.group.Project == "WFUpperFaceDIWWashInletValve1"
                                    ? "DryWfUpperFaceDiwWashInletValve1"
                                    : item.group.Project == "WFUpperFaceDIWWashOutletValve1"
                                        ? "DryWfUpperFaceDiwWashOutletValve1"
                                        : "DryWfLowerFaceDiwWashValve", command.ProjectKey);
                Assert.AreEqual(item.group.Tag, command.PidTag);
                Assert.AreEqual(chamber.Handle, command.Handle);
                Assert.AreEqual(item.code, PlcHandleHelper.GetValueByPath(command.CommandPath));
            }

            Assert.AreEqual(7, commands.Select(c => c.ProjectKey).Distinct().Count());
            Assert.AreEqual(18100u, commands.Single(c => c.CommandCode == 1283).ParameterHandle);
            Assert.IsNull(commands.Single(c => c.CommandCode == 1284).ParameterHandle);
        }
    }

    [WpfTestMethod]
    public void Commands_WriteToSelectedPlcAndFlowrateWritesSetpointFirst()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var config = chamber.Config();
            var model = CreateModel(config, writes);
            foreach (var command in model.CommandGroups.SelectMany(g => g.Commands))
            {
                writes.Clear();
                if (command.CommandCode == 1283)
                    model.Parameters[0].InputValue = "321.45";
                command.ExecuteCommand.Execute();

                if (command.CommandCode == 1283)
                {
                    Assert.AreEqual(2, writes.Count);
                    Assert.AreEqual(KHLDevType.DEV_ZF, writes[0].Device);
                    Assert.AreEqual(18100u, writes[0].Address);
                    Assert.AreEqual(2u, writes[0].Words);
                    Assert.AreEqual(321.45f, BitConverter.ToSingle(writes[0].Data, 0));
                    Assert.AreEqual(KHLDevType.DEV_W, writes[1].Device);
                    Assert.AreEqual(chamber.Handle, writes[1].Address);
                    Assert.AreEqual(1u, writes[1].Words);
                    Assert.AreEqual(1283, BitConverter.ToInt32(writes[1].Data, 0));
                }
                else
                {
                    var write = writes.Single();
                    Assert.AreEqual(chamber.Socket, write.Socket);
                    Assert.AreEqual(KHLDevType.DEV_W, write.Device);
                    Assert.AreEqual(chamber.Handle, write.Address);
                    Assert.AreEqual(1u, write.Words);
                    Assert.AreEqual(command.CommandCode, BitConverter.ToInt32(write.Data, 0));
                }
            }
        }

        var failureWrites = new List<Write>();
        var errors = new List<bool>();
        var failingModel = CreateModel(Chambers[0].Config(), failureWrites, result: 1, notify: (success, _) => errors.Add(success));
        failingModel.Parameters[0].InputValue = "100.00";
        failingModel.CommandGroups.SelectMany(g => g.Commands).Single(c => c.CommandCode == 1283).ExecuteCommand.Execute();
        Assert.AreEqual(1, failureWrites.Count);
        CollectionAssert.AreEqual(new[] { false }, errors);

        failureWrites.Clear();
        failingModel.Parameters[0].InputValue = "1000.01";
        failingModel.CommandGroups.SelectMany(g => g.Commands).Single(c => c.CommandCode == 1283).ExecuteCommand.Execute();
        Assert.AreEqual(0, failureWrites.Count);
    }

    [WpfTestMethod]
    public void DryTabs_LoadBothNestedViewsAndLocalizeOnlyVisibleNames()
    {
        EnsureApplicationResources();
        var originalCulture = LocalizeDictionary.Instance.Culture;
        try
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");
            var model = new LiquidContentViewModel();
            var dryModel = model.Tabs[3];
            Assert.IsFalse(dryModel.IsEmpty);
            CollectionAssert.AreEqual(new[] { "DryA", "DryB" }, dryModel.Chambers.Select(c => c.Header).ToArray());
            var view = new LiquidContentView { DataContext = model };
            var categoryTabs = (TabControl)view.Content;
            categoryTabs.SelectedIndex = 3;
            Layout(view);
            var chamberTabs = Descendants<TabControl>(categoryTabs).Single();
            Assert.AreEqual(2, chamberTabs.Items.Count);

            for (var index = 0; index < 2; index++)
            {
                chamberTabs.SelectedIndex = index;
                Layout(view);
                var dryView = Descendants<GenericDryLiquidContentView>(view).Single();
                var chamber = dryModel.Chambers[index];
                var chamberContent = (GenericDryLiquidContentViewModel)chamber.Content;
                Assert.AreSame(chamberContent, dryView.DataContext);
                Assert.AreEqual(1, Descendants<TextBox>(dryView).Count());
                Assert.AreEqual(16, Descendants<Button>(dryView).Count());

                foreach (var language in new[] { "en", "zh", "en" })
                {
                    var culture = CultureInfo.GetCultureInfo(language);
                    LocalizeDictionary.Instance.Culture = culture;
                    Layout(view);
                    var text = Descendants<TextBlock>(dryView).Select(t => t.Text).ToArray();
                    CollectionAssert.Contains(text, Resources.ResourceManager.GetString("DrawingName", culture));
                    CollectionAssert.Contains(text, "FC-SDAC104");
                    CollectionAssert.Contains(text, "mL/min");
                    CollectionAssert.Contains(text, Resources.ResourceManager.GetString("DryOuterRinseFlowrate", culture));
                    foreach (var hidden in new[] { "Address", "Description", "CommandCode", "WordCount" })
                        CollectionAssert.DoesNotContain(text, Resources.ResourceManager.GetString(hidden, culture));
                    foreach (var parameter in chamberContent.Parameters)
                    {
                        CollectionAssert.Contains(text, parameter.Name);
                        CollectionAssert.DoesNotContain(text, language == "zh" ? parameter.EnglishName : parameter.ChineseName);
                        CollectionAssert.DoesNotContain(text, parameter.Address);
                    }
                    foreach (var group in chamberContent.CommandGroups)
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
                }
            }
        }
        finally
        {
            LocalizeDictionary.Instance.Culture = originalCulture;
        }
    }
}
