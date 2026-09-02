using CMP.Models.Resource;
using CMP.Models.Warn;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Models;

[TestClass]
public class ChamberAvailabilityTests
{
    [TestMethod]
    public void PenA2AndPenB2AreDisabledWithoutChangingOtherChambers()
    {
        Assert.IsFalse(ChamberAvailability.IsEnabled("PenA2"));
        Assert.IsFalse(ChamberAvailability.IsEnabled("PenB2"));
        Assert.IsTrue(ChamberAvailability.IsEnabled("PenA1"));
        Assert.IsTrue(ChamberAvailability.IsEnabled("PenB1"));
    }

    [TestMethod]
    public void OnlyPenA2AndPenB2AlarmUnitsAreFiltered()
    {
        Assert.IsFalse(ChamberAvailability.IsEnabled(Unit.PenA2));
        Assert.IsFalse(ChamberAvailability.IsEnabled(Unit.PenB2));
        Assert.IsTrue(ChamberAvailability.IsEnabled(Unit.PenA1));
        Assert.IsTrue(ChamberAvailability.IsEnabled(Unit.PenB1));
        Assert.IsTrue(ChamberAvailability.IsEnabled(Unit.None));
    }
}
