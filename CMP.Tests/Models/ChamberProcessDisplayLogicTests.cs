using System;
using System.Collections.Generic;
using System.Reflection;
using CMP.Models.Resource;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Models;

[TestClass]
public class ChamberProcessDisplayLogicTests
{
    private static readonly Type ChamberType = typeof(Chamber);

    [TestMethod]
    public void RecipeSelectionKey_MapsAllSupportedChambers()
    {
        var expected = new Dictionary<string, string>
        {
            ["PolishA"] = "RecipePolishA",
            ["PolishB"] = "RecipePolishB",
            ["PolishC"] = "RecipePolishC",
            ["PolishD"] = "RecipePolishD",
            ["RollerA1"] = "RecipeRollerA1",
            ["RollerA2"] = "RecipeRollerA2",
            ["RollerA3"] = "RecipeRollerA3",
            ["RollerB1"] = "RecipeRollerB1",
            ["RollerB2"] = "RecipeRollerB2",
            ["RollerB3"] = "RecipeRollerB3",
            ["PenA1"] = "RecipePenA1",
            ["PenA2"] = "RecipePenA2",
            ["PenB1"] = "RecipePenB1",
            ["PenB2"] = "RecipePenB2",
            ["DryA"] = "RecipeDryA",
            ["DryB"] = "RecipeDryB"
        };

        foreach (var pair in expected)
        {
            Assert.AreEqual(pair.Value, InvokePrivateStatic<string>("GetRecipeSelectionKey", pair.Key));
        }

        Assert.IsNull(InvokePrivateStatic<string>("GetRecipeSelectionKey", "Unknown"));
    }

    [TestMethod]
    public void RecipeNumber_UsesInvariantCompactFormatting()
    {
        Assert.AreEqual("15", InvokePrivateStatic<string>("FormatRecipeNumber", new object[] { "15.00" }));
        Assert.AreEqual("1.25", InvokePrivateStatic<string>("FormatRecipeNumber", new object[] { "1.250" }));
        Assert.AreEqual("not-a-number", InvokePrivateStatic<string>("FormatRecipeNumber", new object[] { "not-a-number" }));
        Assert.AreEqual(string.Empty, InvokePrivateStatic<string>("FormatRecipeNumber", new object[] { null! }));
    }

    [TestMethod]
    public void WaferFormatting_UsesOneBasedCassetteAndSlot()
    {
        Assert.AreEqual("C1W1", FormatWafer(CreateWafer(0, 0)));
        Assert.AreEqual("C4W25", FormatWafer(CreateWafer(3, 24)));
        Assert.IsNull(FormatWafer(CreateWafer(-1, 0)));
        Assert.IsNull(FormatWafer(CreateWafer(4, 0)));
        Assert.IsNull(FormatWafer(CreateWafer(0, 25)));
    }

    [TestMethod]
    public void PolishLogState_ContainsExpectedDisplayValues()
    {
        Assert.AreEqual("PolishInitialized", ((PolishLogState)100).ToString());
        Assert.AreEqual("Polish", ((PolishLogState)101).ToString());
        Assert.AreEqual("PolishLoad", ((PolishLogState)103).ToString());
        Assert.AreEqual("PolishUnload", ((PolishLogState)104).ToString());
        Assert.AreEqual("PolishDechuck", ((PolishLogState)105).ToString());
        Assert.AreEqual("PolishFailure", ((PolishLogState)108).ToString());
        Assert.AreEqual("ExsituCondition", ((PolishLogState)240).ToString());
        Assert.IsFalse(Enum.IsDefined(typeof(PolishLogState), 999));
    }

    private static string FormatWafer(Wafer wafer)
    {
        var method = ChamberType.GetMethod("TryFormatWafer", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);

        var arguments = new object[] { wafer, null! };
        var result = (bool)method!.Invoke(null, arguments)!;
        return result ? (string)arguments[1]! : null;
    }

    private static Wafer CreateWafer(int loadportId, int slotId)
    {
        return new Wafer { LoadportID = loadportId, SlotID = slotId };
    }

    private static T InvokePrivateStatic<T>(string methodName, params object[] arguments)
    {
        var method = ChamberType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        return (T)method!.Invoke(null, arguments)!;
    }
}
