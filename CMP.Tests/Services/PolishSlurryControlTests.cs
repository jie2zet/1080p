#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CMP.Constants.Plc;
using CMP.Lang;
using CMP.Models.kvsPlc;
using CMP.ViewModels.Tabs.OperationTab.PolishUnitTab;
using CMP.Views.Tabs.OperationTab;
using CMP.Views.Tabs.OperationTab.PolishUnitTab;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPFLocalizeExtension.Engine;
using static CMP.Tests.Services.LiquidControlTests;

namespace CMP.Tests.Services;

[TestClass]
[DoNotParallelize]
public class PolishSlurryControlTests
{
    private sealed record Write(int Socket, KHLDevType Device, uint Address, uint Words, byte[] Data);

    // Independent transcription of the new Cylinder command table.
    private static readonly (string Path, int Open, int Close, uint? Setpoint)[] Switches =
    [
        ("SlurryAndPlatenWash", 1692, 1693, null),
        ("Slurry1.Slurry", 1710, 1711, 18172),
        ("Slurry1.Diw", 1716, 1717, 18172),
        ("Slurry1", 1722, 1723, 18172),
        ("Slurry2.Slurry", 1712, 1713, 18174),
        ("Slurry2.Diw", 1718, 1719, 18174),
        ("Slurry2", 1724, 1725, 18174),
        ("Slurry3.Slurry", 1714, 1715, 18176),
        ("Slurry3.Diw", 1720, 1721, 18176),
        ("Slurry3", 1726, 1727, 18176)
    ];

    private static PolishSlurryContentViewModel CreateModel(List<Write> writes,
        Func<int>? socket = null, Func<Write, int>? result = null, Action<bool, string>? notify = null) =>
        new(socket ?? (() => 101), (sock, device, address, words, data) =>
        {
            var write = new Write(sock, device, address, words, data.ToArray());
            writes.Add(write);
            return result?.Invoke(write) ?? 0;
        }, notify ?? ((_, _) => { }), () => "Polish offline");

    [WpfTestMethod]
    public void All22Codes_AreDefinedInPlcHandleAndAll20SlurryActionsRouteToEachChamber()
    {
        Assert.AreEqual(1670, PlcHandleHelper.GetValueByPath("CmdPolish.Cylinder.ConditionHead.Up"));
        Assert.AreEqual(1671, PlcHandleHelper.GetValueByPath("CmdPolish.Cylinder.ConditionHead.Down"));
        Assert.AreEqual(0x50u, PlcHandleHelper.GetHandleByPath("CmdPolish.Cylinder.ConditionHead.Up"));
        Assert.AreEqual(0x50u, PlcHandleHelper.GetHandleByPath("CmdPolish.Cylinder.ConditionHead.Down"));
        foreach (var socket in new[] { 101, 202, 303, 404 })
        {
            var writes = new List<Write>();
            var model = CreateModel(writes, () => socket);
            var controls = new[] { model.TotalControl }.Concat(model.Lines.SelectMany(l => l.Controls)).ToArray();
            Assert.AreEqual(10, controls.Length);
            for (var i = 0; i < Switches.Length; i++)
            {
                var expected = Switches[i];
                var definition = controls[i].Definition;
                Assert.AreEqual(expected.Setpoint, definition.ParameterHandle);
                for (var action = 0; action < 2; action++)
                {
                    var open = action == 0;
                    var path = $"CmdPolish.Cylinder.{expected.Path}.{(open ? "Open" : "Close")}";
                    var code = open ? expected.Open : expected.Close;
                    Assert.AreEqual(path, open ? definition.OpenPath : definition.ClosePath);
                    Assert.AreEqual(code, PlcHandleHelper.GetValueByPath(path));
                    Assert.AreEqual(0x50u, PlcHandleHelper.GetHandleByPath(path));
                    writes.Clear();
                    (open ? controls[i].OpenCommand : controls[i].CloseCommand).Execute();
                    Assert.AreEqual(open ? (i == 0 ? 4 : 2) : 1, writes.Count, path);
                    Assert.IsTrue(writes.All(w => w.Socket == socket), path);
                    if (open)
                    {
                        var addresses = i == 0 ? new uint[] { 18172, 18174, 18176 } : [expected.Setpoint!.Value];
                        CollectionAssert.AreEqual(addresses, writes.SkipLast(1).Select(w => w.Address).ToArray());
                        Assert.IsTrue(writes.SkipLast(1).All(w => w.Device == KHLDevType.DEV_ZF && w.Words == 2));
                    }
                    var final = writes.Last();
                    Assert.AreEqual(KHLDevType.DEV_W, final.Device);
                    Assert.AreEqual(0x50u, final.Address);
                    Assert.AreEqual(1u, final.Words);
                    Assert.AreEqual(code, (int)BitConverter.ToUInt16(final.Data, 0));
                }
            }
        }
    }

    [WpfTestMethod]
    public void SetAndOpen_PreserveOriginalSetpointsAndFloatDecimals()
    {
        var writes = new List<Write>();
        var model = CreateModel(writes);
        for (var i = 0; i < 3; i++)
        {
            var line = model.Lines[i];
            var parameter = line.Parameter;
            Assert.AreEqual((uint)(18172 + i * 2), parameter.Handle);
            Assert.AreEqual(100d, parameter.Metadata.NumericDefaultValue);
            Assert.AreEqual(0d, parameter.MinValue);
            Assert.AreEqual(1000d, parameter.MaxValue);
            Assert.AreEqual(2, parameter.DecimalPlaces);
            Assert.AreEqual(2, parameter.WordCount);
            Assert.AreEqual("mL/min", parameter.Unit);
            foreach (var input in new[] { "0.00", "123.45", "123.456", "1000.00" })
            {
                writes.Clear();
                parameter.InputValue = input;
                line.SetCommand.Execute();
                var write = writes.Single();
                var rounded = Math.Round(double.Parse(input, CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
                Assert.AreEqual(KHLDevType.DEV_ZF, write.Device);
                Assert.AreEqual(parameter.Handle, write.Address);
                Assert.AreEqual(2u, write.Words);
                Assert.AreEqual((float)rounded, BitConverter.ToSingle(write.Data, 0));
                Assert.AreEqual(rounded.ToString("F2", CultureInfo.CurrentCulture), parameter.InputValue);
                if (input == "123.45")
                    CollectionAssert.AreEqual(new byte[] { 0x66, 0xE6, 0xF6, 0x42 }, write.Data.Take(4).ToArray());
            }
        }
    }

    [WpfTestMethod]
    public void InvalidInputs_BlockSetAndOpenButNeverBlockClose()
    {
        var writes = new List<Write>();
        var model = CreateModel(writes);
        foreach (var line in model.Lines)
        {
            foreach (var input in new[] { "", "abc", "NaN", "Infinity", "-Infinity", "1e999", "-0.01", "1000.01" })
            {
                line.Parameter.InputValue = input;
                writes.Clear();
                line.SetCommand.Execute();
                foreach (var control in line.Controls) control.OpenCommand.Execute();
                model.TotalControl.OpenCommand.Execute();
                Assert.AreEqual(0, writes.Count, input);
                foreach (var control in line.Controls) control.CloseCommand.Execute();
                model.TotalControl.CloseCommand.Execute();
                Assert.AreEqual(4, writes.Count, input);
                Assert.IsTrue(writes.All(w => w.Device == KHLDevType.DEV_W));
            }
            line.Parameter.InputValue = "0.00";
            writes.Clear();
            line.Controls[2].CloseCommand.Execute();
            Assert.AreEqual(1, writes.Count, "Closing a zero-flow line must still send its command.");
            line.Parameter.InputValue = "100.00";
        }
    }

    [WpfTestMethod]
    public void CombinedOpen_ValidatesAllLinesBeforeWritingAndStopsOnParameterWriteFailure()
    {
        var writes = new List<Write>();
        var notifications = new List<bool>();
        var model = CreateModel(writes, notify: (success, _) => notifications.Add(success));
        model.Parameters[0].InputValue = "11.11";
        model.Parameters[1].InputValue = "22.22";
        model.Parameters[2].InputValue = "33.33";
        model.TotalControl.OpenCommand.Execute();
        CollectionAssert.AreEqual(new uint[] { 18172, 18174, 18176, 0x50 }, writes.Select(w => w.Address).ToArray());
        CollectionAssert.AreEqual(new[] { 11.11f, 22.22f, 33.33f }, writes.Take(3).Select(w => BitConverter.ToSingle(w.Data, 0)).ToArray());
        CollectionAssert.AreEqual(new[] { true }, notifications);
        foreach (var failedAddress in new uint[] { 18172, 18174, 18176 })
        {
            writes.Clear();
            notifications.Clear();
            model = CreateModel(writes, result: w => w.Address == failedAddress ? -1 : 0,
                notify: (success, _) => notifications.Add(success));
            model.TotalControl.OpenCommand.Execute();
            Assert.AreEqual((int)((failedAddress - 18172) / 2 + 1), writes.Count);
            Assert.IsTrue(writes.All(w => w.Device == KHLDevType.DEV_ZF));
            CollectionAssert.AreEqual(new[] { false }, notifications);
        }
        foreach (var i in Enumerable.Range(0, 3))
        {
            writes.Clear();
            model = CreateModel(writes, result: _ => -1);
            model.Lines[i].Controls[0].OpenCommand.Execute();
            Assert.AreEqual(1, writes.Count);
            Assert.AreEqual(KHLDevType.DEV_ZF, writes[0].Device);
        }
    }

    [WpfTestMethod]
    public void Writes_UseLatestSocketAndReportOfflineExceptionsWithoutCrashing()
    {
        var writes = new List<Write>();
        var reads = 0;
        var socket = 101;
        var model = CreateModel(writes, () => { reads++; return socket; });
        Assert.AreEqual(0, reads);
        Assert.AreEqual(0, writes.Count);
        socket = 909;
        model.TotalControl.OpenCommand.Execute();
        Assert.AreEqual(4, reads);
        Assert.IsTrue(writes.All(w => w.Socket == 909));
        foreach (var failure in new[] { "socket", "writer", "command" })
        {
            writes.Clear();
            var notifications = new List<bool>();
            model = CreateModel(writes,
                () => failure == "socket" ? throw new InvalidOperationException("offline socket") : 101,
                w => failure == "writer" ? throw new InvalidOperationException("offline writer")
                    : failure == "command" && w.Device == KHLDevType.DEV_W ? -1 : 0,
                (success, _) => notifications.Add(success));
            model.TotalControl.OpenCommand.Execute();
            CollectionAssert.AreEqual(new[] { false }, notifications, failure);
            Assert.AreEqual(failure == "socket" ? 0 : failure == "writer" ? 1 : 4, writes.Count, failure);
        }
    }

    [WpfTestMethod]
    public void CompactTabs_SwitchLinesPreserveInputAndUpdateLanguageAndActualFlow()
    {
        EnsureApplicationResources();
        var original = LocalizeDictionary.Instance.Culture;
        try
        {
            var writes = new List<Write>();
            var model = CreateModel(writes);
            var view = new PolishSlurryContentView { DataContext = model };
            foreach (var width in new[] { 380d, 430d })
            {
                foreach (var language in new[] { "en", "zh", "en" })
                {
                    var culture = CultureInfo.GetCultureInfo(language);
                    LocalizeDictionary.Instance.Culture = culture;
                    CompactLayout(view, width);
                    var tabs = Descendants<TabControl>(view).Single();
                    Assert.AreEqual(3, tabs.Items.Count);
                    Assert.AreSame(view.FindResource("SecondaryTabControlStyle"), tabs.Style);
                    for (var i = 0; i < 3; i++)
                    {
                        tabs.SelectedIndex = i;
                        model.Lines[i].ActualFlowrate = 111.11f + i;
                        CompactLayout(view, width);
                        Assert.AreSame(model.Lines[i], tabs.SelectedItem);
                        Assert.AreEqual(1, Descendants<TextBox>(view).Count());
                        Assert.AreEqual(9, Descendants<Button>(view).Count());
                        Assert.AreEqual(0, Descendants<RadioButton>(view).Count());
                        Assert.IsTrue(view.DesiredSize.Height < 340, $"Tab layout is too tall: {view.DesiredSize.Height}");
                        var input = Descendants<TextBox>(view).Single();
                        Assert.AreSame(model.Lines[i], input.DataContext);
                        input.Text = $"{120 + i}.45";
                        input.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
                        Assert.AreEqual($"{120 + i}.45", model.Parameters[i].InputValue);
                        var text = Descendants<TextBlock>(view).Select(t => t.Text).ToArray();
                        foreach (var key in new[] { "PolishSlurryAndPlatenWash", "PolishSlurrySetFlowrate", "PolishSlurryActualFlowrate", "Unit" })
                            CollectionAssert.Contains(text, Resources.ResourceManager.GetString(key, culture));
                        CollectionAssert.Contains(text, model.Lines[i].ActualFlowrate.ToString("F2", CultureInfo.CurrentCulture));
                        CollectionAssert.Contains(text, "mL/min");
                        foreach (var control in model.Lines[i].Controls)
                        {
                            Assert.AreEqual(Resources.ResourceManager.GetString(control.Definition.DisplayNameKey, culture), control.Title);
                            CollectionAssert.Contains(text, control.Title);
                            foreach (var path in new[] { control.Definition.OpenPath, control.Definition.ClosePath })
                            {
                                CollectionAssert.DoesNotContain(text, $"W{PlcHandleHelper.GetHandleByPath(path):X}");
                                CollectionAssert.DoesNotContain(text, PlcHandleHelper.GetValueByPath(path)!.Value.ToString());
                            }
                        }
                        foreach (var button in Descendants<Button>(view)) Assert.IsNotNull(button.Command);
                        foreach (var element in Descendants<FrameworkElement>(view).Where(e => e is Button or TextBox))
                        {
                            var bounds = element.TransformToAncestor(view).TransformBounds(new Rect(element.RenderSize));
                            Assert.IsTrue(bounds.Left >= -1 && bounds.Right <= width + 1 && bounds.Bottom <= view.ActualHeight + 1,
                                $"{language}: {element.GetType().Name} extends outside the compact view: {bounds}");
                        }
                    }
                    tabs.SelectedIndex = 0;
                    CompactLayout(view, width);
                    Assert.AreEqual("120.45", Descendants<TextBox>(view).Single().Text);
                    SavePreview(view, $"slurry-{language}-{width}.png");
                }
            }
            Assert.AreEqual(0, writes.Count, "Switching tabs, cultures and readbacks must never write to the PLC.");
            var parent = new PolishUnitTabContentView { DataContext = new { SlurryControl = model } };
            Layout(parent);
            var embedded = Descendants<PolishSlurryContentView>(parent).Single();
            Assert.AreSame(model, embedded.DataContext);
            Assert.AreEqual(3, Descendants<TabControl>(embedded).Single().Items.Count);
        }
        finally { LocalizeDictionary.Instance.Culture = original; }
    }

    private static void CompactLayout(FrameworkElement view, double width)
    {
        view.Measure(new Size(width, double.PositiveInfinity));
        view.Arrange(new Rect(0, 0, width, view.DesiredSize.Height));
        view.UpdateLayout();
        view.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        view.UpdateLayout();
    }

    private static void SavePreview(FrameworkElement view, string filename)
    {
        var directory = Environment.GetEnvironmentVariable("CMP_SLURRY_PREVIEW_DIR");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(view.ActualWidth), (int)Math.Ceiling(view.ActualHeight),
            96, 96, PixelFormats.Pbgra32);
        var background = new DrawingVisual();
        using (var drawing = background.RenderOpen())
            drawing.DrawRectangle(Brushes.White, null, new Rect(view.RenderSize));
        bitmap.Render(background);
        bitmap.Render(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(directory, filename));
        encoder.Save(stream);
    }
}
