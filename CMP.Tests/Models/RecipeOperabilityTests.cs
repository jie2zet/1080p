#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CMP.Tests.Models;

[TestClass]
public class RecipeOperabilityTests
{
    private const string DryFolder = "Dry Recipe";

    // ---------- Task 1: RecipeOptionValue ----------

    [DataTestMethod]
    [DataRow("1: Dry With Arm", "1")]
    [DataRow("3: Dry", "3")]
    [DataRow("-1: Recipe End", "-1")]
    [DataRow("1", "1")]
    [DataRow("10", "10")]
    [DataRow("  3: Dry  ", "3")]
    [DataRow("", null)]
    [DataRow("   ", null)]
    [DataRow(null, null)]
    [DataRow("abc", null)]
    [DataRow("1abc", null)]
    [DataRow(": 1", null)]
    public void NumericPrefixParsesOptionStrings(string? input, string? expected)
    {
        Assert.AreEqual(expected, RecipeOptionValue.NumericPrefix(input));
    }
}
