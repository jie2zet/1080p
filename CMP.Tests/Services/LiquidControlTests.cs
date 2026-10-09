#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.Models.Operation.Roller;
using CMP.ViewModels.Tabs.MainTab.Content;
using CMP.Views.Tabs.MainTab.Content;
using CMP.Views.Tabs.OperationTab;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPFLocalizeExtension.Engine;

namespace CMP.Tests.Services;

[TestClass]
[DoNotParallelize]
public class LiquidControlTests
{
    private sealed record Write(int Socket, KHLDevType Device, uint Address, uint Words, byte[] Data);

    private static readonly (string Name, Func<GenericRollConfig> CreateConfig, uint BaseOffset, uint CommandHandle, int Socket)[] Chambers =
    [
        ("RollerA1", () => RollerConfigFactory.CreateRollA1Config(), 18100u, 0x60u, 101),
        ("RollerA2", () => RollerConfigFactory.CreateRollA2Config(), 33100u, 0x63u, 101),
        ("RollerA3", () => RollerConfigFactory.CreateRollA3Config(), 48100u, 0x66u, 101),
        ("RollerB1", () => RollerConfigFactory.CreateRollB1Config(), 18100u, 0x69u, 202),
        ("RollerB2", () => RollerConfigFactory.CreateRollB2Config(), 33100u, 0x6Cu, 202),
        ("RollerB3", () => RollerConfigFactory.CreateRollB3Config(), 48100u, 0x6Fu, 202)
    ];

    private static GenericRollerLiquidContentViewModel CreateModel(
        List<Write> writes, int result = 0, GenericRollConfig? config = null,
        Func<(KVLink RollerA, KVLink RollerB)>? getPlcLinks = null) =>
        new(config ?? RollerConfigFactory.CreateRollA1Config(),
        getPlcLinks ?? (() => (new KVLink("offline-a") { sock = 101 }, new KVLink("offline-b") { sock = 202 })),
        (socket, device, address, words, data) =>
        {
            writes.Add(new Write(socket, device, address, words, data));
            return result;
        }, (_, _) => { });

    [STATestMethod]
    public void Parameters_UseSelectedChamberMetadataEvenWhenOtherChambersReuseOffsets()
    {
        var expected = new[]
        {
            (18u, 100d, 15d, 500d, "FC-ROAC100", "Chemical Enter Flow Set", "药液流量控制(混合前)", "ChemicalEnterFlow"),
            (20u, 1000d, 200d, 9999d, "FC-ROAC101", "DIW Enter Flow Set", "纯水流量控制(混合前)", "DiwEnterFlow"),
            (22u, 1000d, 200d, 9999d, "FC-RO1AC105", "Upper Rinse DIW Wash", "晶圆上表面清洁 流量控制", "WfUpperFaceWashDiwFlow"),
            (24u, 1000d, 200d, 9999d, "FC-RO1AC106", "Lower Rinse DIW Wash", "晶圆下表面清洁 流量控制", "WfLowerFaceDiwWashFlow")
        };

        foreach (var chamber in Chambers)
        {
            var config = chamber.CreateConfig();
            var model = CreateModel([], config: config);
            Assert.AreEqual(expected.Length, model.Parameters.Count, chamber.Name);
            foreach (var (suffix, initial, min, max, tag, english, chinese, project) in expected)
            {
                var offset = chamber.BaseOffset + suffix;
                var parameter = model.Parameters.Single(p => p.Handle == offset);
                Assert.AreSame(PlcHandleHelper.GetParamByPath($"{config.PlcHandleTypeName}.Liquid.{project}.Param.FlowrateSet"), parameter.Metadata);
                Assert.AreEqual(initial, parameter.Metadata.NumericDefaultValue);
                Assert.AreEqual(min, parameter.MinValue);
                Assert.AreEqual(max, parameter.MaxValue);
                Assert.AreEqual(tag, parameter.DrawingTag);
                Assert.AreEqual("mL/min", parameter.Unit);
                Assert.AreEqual($"ZF{offset}", parameter.Address);
                Assert.AreEqual(2, parameter.WordCount);
                Assert.AreEqual(2, parameter.DecimalPlaces);
                Assert.AreEqual(english, parameter.EnglishName);
                Assert.AreEqual(chinese, parameter.ChineseName);
                Assert.AreEqual(initial.ToString("F2", CultureInfo.CurrentCulture), parameter.InputValue);
            }
        }

        var legacy = new PlcHandle.ParamAttribute(100, 10, 0, 20);
        Assert.AreEqual(10, legacy.DefaultValue);
        Assert.AreEqual(10d, legacy.NumericDefaultValue);
        Assert.AreEqual(0, legacy.DecimalPlaces);
    }

    [STATestMethod]
    public void InvalidParameters_BlockBothSetAndFlowCommands()
    {
        var writes = new List<Write>();
        foreach (var chamber in Chambers)
        {
            var model = CreateModel(writes, config: chamber.CreateConfig());
            foreach (var flow in model.CommandGroups.SelectMany(g => g.Commands).Where(c => c.FlowParameterHandle.HasValue))
            {
                var parameter = model.Parameters.Single(p => p.Handle == flow.FlowParameterHandle);
                foreach (var input in new[] { "", "abc", "NaN", "Infinity", "-Infinity", "1e999",
                    (parameter.MinValue - 0.01).ToString(CultureInfo.InvariantCulture),
                    (parameter.MaxValue + 0.01).ToString(CultureInfo.InvariantCulture) })
                {
                    parameter.InputValue = input;
                    parameter.SetCommand.Execute();
                    flow.ExecuteCommand.Execute();
                    Assert.AreEqual(0, writes.Count, $"{chamber.Name}: Invalid value '{input}' must not reach the PLC.");
                }
            }
        }
    }

    [STATestMethod]
    public void ValidParameter_WritesTwoFloatWordsAndFormatsToTwoDecimals()
    {
        var writes = new List<Write>();
        foreach (var chamber in Chambers)
        {
            var model = CreateModel(writes, config: chamber.CreateConfig());
            foreach (var parameter in model.Parameters)
            {
                foreach (var value in new[] { parameter.MinValue, parameter.MinValue + 123.456d, parameter.MaxValue })
                {
                    parameter.InputValue = value.ToString(CultureInfo.InvariantCulture);
                    parameter.SetCommand.Execute();
                    var write = writes.Last();
                    var rounded = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                    Assert.AreEqual(chamber.Socket, write.Socket);
                    Assert.AreEqual(KHLDevType.DEV_ZF, write.Device);
                    Assert.AreEqual(parameter.Handle, write.Address);
                    Assert.AreEqual(2u, write.Words);
                    Assert.AreEqual((float)rounded, BitConverter.ToSingle(write.Data, 0));
                    Assert.AreEqual(rounded.ToString("F2", CultureInfo.CurrentCulture), parameter.InputValue);
                }
            }
        }
        Assert.AreEqual(72, writes.Count);
    }

    [STATestMethod]
    public void Commands_UseEachChambersHandleAndPlcAndWriteItsFlowParameterFirst()
    {
        var writes = new List<Write>();
        var expectedCodes = Enumerable.Range(1210, 21)
            .Concat(Enumerable.Range(1242, 7))
            .Concat(Enumerable.Range(1263, 5))
            .Concat(Enumerable.Range(1293, 19))
            .Concat(new[] { 1400, 1401, 1404, 1405, 1406, 1407 }).ToArray();
        var flowSuffixes = new Dictionary<int, uint>
        {
            [1212] = 18, [1219] = 20, [1246] = 22, [1265] = 24
        };
        foreach (var chamber in Chambers)
        {
            var config = chamber.CreateConfig();
            Assert.AreEqual(chamber.Socket == 101 ? "192.168.2.20" : "192.168.2.23", config.PlcAddress);
            var model = CreateModel(writes, config: config);
            var commands = model.CommandGroups.SelectMany(g => g.Commands).ToArray();
            CollectionAssert.AreEqual(expectedCodes, commands.Select(c => c.CommandCode).ToArray(), chamber.Name);
            Assert.AreEqual(27, model.CommandGroups.Count);
            foreach (var command in commands)
            {
                writes.Clear();
                command.ExecuteCommand.Execute();
                var hasFlow = flowSuffixes.TryGetValue(command.CommandCode, out var flowSuffix);
                Assert.AreEqual(hasFlow ? 2 : 1, writes.Count, $"{chamber.Name}: {command.CommandKey}");
                if (hasFlow)
                {
                    Assert.AreEqual(chamber.Socket, writes[0].Socket);
                    Assert.AreEqual(KHLDevType.DEV_ZF, writes[0].Device);
                    Assert.AreEqual(chamber.BaseOffset + flowSuffix, writes[0].Address);
                    Assert.AreEqual(2u, writes[0].Words);
                }

                var write = writes.Last();
                Assert.AreEqual(chamber.Socket, write.Socket);
                Assert.AreEqual(KHLDevType.DEV_W, write.Device);
                Assert.AreEqual(chamber.CommandHandle, write.Address);
                Assert.AreEqual(1u, write.Words);
                Assert.AreEqual((ushort)command.CommandCode, BitConverter.ToUInt16(write.Data, 0));
                Assert.AreEqual($"W{chamber.CommandHandle:X}", command.Address);
                Assert.AreNotEqual(command.Definition.ActionKey, command.EnglishActionName);
                Assert.AreNotEqual(command.Definition.ActionKey, command.ChineseActionName);

                var fieldPath = command.ProjectKey == "RollerManualDiwWash"
                    ? $"{config.PlcHandleTypeName}.Flush.{command.CommandKey.Split('_')[1]}"
                    : $"{config.PlcHandleTypeName}.Liquid.{command.CommandKey.Replace('_', '.')}";
                Assert.AreEqual(chamber.CommandHandle, PlcHandleHelper.GetHandleByPath(fieldPath));
                Assert.AreEqual(command.CommandCode, PlcHandleHelper.GetValueByPath(fieldPath));
            }

            var valve2 = model.CommandGroups.Single(g => g.ProjectKey == "ChemicalInletMainValve2");
            Assert.AreEqual(1, valve2.Commands.Count);
            Assert.AreEqual("ActionOpen", valve2.Commands[0].Definition.ActionKey);
            Assert.IsNull(PlcHandleHelper.GetHandleByPath($"{config.PlcHandleTypeName}.Liquid.ChemicalInletMainValve2.Close"));
            Assert.ThrowsException<ArgumentException>(() => config.GetLiquidCommand("ChemicalInletMainValve2_Close"));
            Assert.AreEqual(1404, BitConverter.ToInt32(config.GetFlushCommand("Open").value, 0));
            Assert.AreEqual(1405, BitConverter.ToInt32(config.GetFlushCommand("Close").value, 0));
        }
    }

    [STATestMethod]
    public void FailedFlowWrite_DoesNotSendTheCommand()
    {
        var writes = new List<Write>();
        foreach (var chamber in Chambers)
        {
            var model = CreateModel(writes, -1, chamber.CreateConfig());
            foreach (var flow in model.CommandGroups.SelectMany(g => g.Commands).Where(c => c.FlowParameterHandle.HasValue))
            {
                writes.Clear();
                flow.ExecuteCommand.Execute();
                Assert.AreEqual(1, writes.Count);
                Assert.AreEqual(chamber.Socket, writes[0].Socket);
                Assert.AreEqual(KHLDevType.DEV_ZF, writes[0].Device);
                Assert.AreEqual(flow.FlowParameterHandle, writes[0].Address);
            }
        }
    }

    [STATestMethod]
    public void Writes_ResolveLatestPlcSocketWithoutConnectingWhenConstructed()
    {
        foreach (var chamber in Chambers)
        {
            var writes = new List<Write>();
            var rollerA = new KVLink("offline-a") { sock = 101 };
            var rollerB = new KVLink("offline-b") { sock = 202 };
            var linkReads = 0;
            var model = CreateModel(writes, config: chamber.CreateConfig(), getPlcLinks: () =>
            {
                linkReads++;
                return (rollerA, rollerB);
            });
            Assert.AreEqual(0, linkReads);
            Assert.AreEqual(0, writes.Count);
            model.Parameters[0].SetCommand.Execute();
            Assert.AreEqual(chamber.Socket, writes.Single().Socket);

            rollerA = new KVLink("reconnected-a") { sock = 303 };
            rollerB = new KVLink("reconnected-b") { sock = 404 };
            writes.Clear();
            var flow = model.CommandGroups.SelectMany(g => g.Commands).Single(c => c.CommandCode == 1212);
            flow.ExecuteCommand.Execute();
            Assert.AreEqual(2, writes.Count);
            Assert.IsTrue(writes.All(w => w.Socket == (chamber.Socket == 101 ? 303 : 404)));
            Assert.AreEqual(chamber.BaseOffset + 18u, writes[0].Address);
            Assert.AreEqual(chamber.CommandHandle, writes[1].Address);
        }
    }

    [STATestMethod]
    public void TabsAndLocalization_UpdateEmptyStatesNamesAndActions()
    {
        var originalCulture = LocalizeDictionary.Instance.Culture;
        try
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");
            var liquid = new LiquidContentViewModel();
            CollectionAssert.AreEqual(new[] { "Polish", "Roller", "Pen", "Dry" }, liquid.Tabs.Select(t => t.Header).ToArray());
            var empty = liquid.Tabs[0];
            Assert.IsTrue(empty.IsEmpty);
            Assert.IsTrue(liquid.Tabs[2].IsEmpty);
            Assert.IsTrue(liquid.Tabs[3].IsEmpty);
            var roller = liquid.Tabs[1];
            CollectionAssert.AreEqual(Chambers.Select(c => c.Name).ToArray(), roller.Chambers.Select(c => c.Header).ToArray());
            var models = roller.Chambers.Select(c => (GenericRollerLiquidContentViewModel)c.Content).ToArray();
            Assert.AreEqual(6, models.Distinct().Count());
            Assert.IsTrue(models.All(m => m.Parameters[0].Name == "Chemical Enter Flow Set"));
            models[0].Parameters[0].InputValue = "123.45";
            Assert.IsTrue(models.Skip(1).All(m => m.Parameters[0].InputValue == 100d.ToString("F2", CultureInfo.CurrentCulture)));
            var notified = new List<string?>();
            empty.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh");
            Assert.AreEqual("抛光", empty.Header);
            Assert.AreEqual("此区域暂未配置液路控制。", empty.EmptyState);
            CollectionAssert.Contains(notified, nameof(empty.EmptyState));
            foreach (var model in models)
            {
                Assert.AreEqual("药液流量控制(混合前)", model.Parameters[0].Name);
                Assert.AreEqual("打开", model.CommandGroups[0].Commands[0].ActionName);
                Assert.AreEqual("FV-ROAC100A", model.CommandGroups[0].PidTag);
                foreach (var group in model.CommandGroups)
                {
                    Assert.AreNotEqual(group.ProjectKey, group.EnglishTitle);
                    Assert.AreNotEqual(group.ProjectKey, group.ChineseTitle);
                }
            }
            Assert.AreEqual("液路控制", Resources.ResourceManager.GetString("LiquidControl", CultureInfo.GetCultureInfo("zh")));
        }
        finally
        {
            LocalizeDictionary.Instance.Culture = originalCulture;
        }
    }

    [STATestMethod]
    public void View_LoadsNestedRollerTabAndRendersParametersAndCommandsWithoutPlc()
    {
        var application = Application.Current;
        if (application == null)
        {
            application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            // Load application resources only. Do not start CMP or its PLC services.
            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Core;component/Resources/Theme/Basic/SkinLocal.xaml", UriKind.Relative)
            });
            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/HandyControl;component/Themes/Theme.xaml", UriKind.Relative)
            });
            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Core;component/Controls/Styles/TabControl/SecondaryTabStyle.xaml", UriKind.Relative)
            });
        }

        var originalCulture = LocalizeDictionary.Instance.Culture;
        try
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en");
            var model = new LiquidContentViewModel();
            var view = new LiquidContentView { DataContext = model };
            var categories = (TabControl)view.Content;
            categories.SelectedIndex = 1;
            Layout(view);
            var chamberTabs = Descendants<TabControl>(categories).Single();
            Assert.AreEqual(6, chamberTabs.Items.Count);
            for (var chamberIndex = 0; chamberIndex < Chambers.Length; chamberIndex++)
            {
                chamberTabs.SelectedIndex = chamberIndex;
                Layout(view);
                var roller = Descendants<GenericRollerLiquidContentView>(view).Single();
                var rollerModel = (GenericRollerLiquidContentViewModel)model.Tabs[1].Chambers[chamberIndex].Content;
                Assert.AreSame(rollerModel, roller.DataContext);
                Assert.AreEqual(4, Descendants<TextBox>(roller).Count());
                Assert.AreEqual(62, Descendants<Button>(roller).Count());

                foreach (var language in new[] { "en", "zh", "en" })
                {
                    var culture = CultureInfo.GetCultureInfo(language);
                    LocalizeDictionary.Instance.Culture = culture;
                    Layout(view);
                    var text = Descendants<TextBlock>(roller).Select(t => t.Text).ToArray();
                    CollectionAssert.Contains(text, "FC-RO1AC105");
                    CollectionAssert.Contains(text, "mL/min");
                    CollectionAssert.Contains(text, "FV-ROAC100A");
                    CollectionAssert.Contains(text, Resources.ResourceManager.GetString("DrawingName", culture));
                    CollectionAssert.DoesNotContain(text, Resources.ResourceManager.GetString("Address", culture));
                    CollectionAssert.DoesNotContain(text, Resources.ResourceManager.GetString("Description", culture));
                    CollectionAssert.DoesNotContain(text, Resources.ResourceManager.GetString("CommandCode", culture));
                    CollectionAssert.DoesNotContain(text, Resources.ResourceManager.GetString("WordCount", culture));
                    foreach (var parameter in rollerModel.Parameters)
                    {
                        CollectionAssert.Contains(text, parameter.Name);
                        CollectionAssert.DoesNotContain(text, language == "zh" ? parameter.EnglishName : parameter.ChineseName);
                        CollectionAssert.DoesNotContain(text, parameter.Address);
                    }
                    foreach (var group in rollerModel.CommandGroups)
                    {
                        CollectionAssert.Contains(text, group.Title);
                        CollectionAssert.DoesNotContain(text, language == "zh" ? group.EnglishTitle : group.ChineseTitle);
                        foreach (var command in group.Commands)
                        {
                            CollectionAssert.DoesNotContain(text, command.Address);
                            CollectionAssert.DoesNotContain(text, command.CommandCode.ToString());
                        }
                    }
                    foreach (var button in Descendants<Button>(roller).Where(b => b.DataContext is LiquidCommandViewModel))
                    {
                        var command = (LiquidCommandViewModel)button.DataContext;
                        Assert.AreEqual(command.ActionName, button.Content);
                        var styleKey = command.Definition.ActionKey switch
                        {
                            "ActionClose" => "ButtonDanger",
                            "ActionFlowrateReset" => "ButtonWarning",
                            "ActionClfcCalibration" => "ButtonInfo",
                            _ => "ButtonPrimary"
                        };
                        Assert.AreSame(roller.FindResource(styleKey), button.Style);
                    }
                }
            }
        }
        finally
        {
            LocalizeDictionary.Instance.Culture = originalCulture;
        }
    }

    private static void Layout(FrameworkElement view)
    {
        view.Measure(new Size(1200, 800));
        view.Arrange(new Rect(0, 0, 1200, 800));
        view.UpdateLayout();
        view.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        view.UpdateLayout();
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) yield return match;
            foreach (var descendant in Descendants<T>(child)) yield return descendant;
        }
    }
}
