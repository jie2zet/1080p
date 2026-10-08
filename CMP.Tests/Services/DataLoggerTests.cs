#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CMP.Services.Log;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMP.Tests.Services;

[TestClass]
public class DataLoggerTests
{
    private string _basePath = null!;

    [TestInitialize]
    public void Initialize()
    {
        _basePath = Path.Combine(Path.GetTempPath(), $"CMP_DataLoggerTests_{Guid.NewGuid():N}");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("Exsitu")]
    public void RenameWaferFile_PreservesCsvAndUsesFinalWaferName(string? subcategory)
    {
        var logger = new DataLogger("PolishA", _basePath);
        const string temporaryWaferName = "Temporary_PolishA_test";
        var data = new Dictionary<string, string?>
        {
            ["Time"] = "2026-07-22 12:00:00:000",
            ["Pressure"] = "12.3"
        };

        if (subcategory == "Exsitu")
        {
            logger.AppendExsituData(temporaryWaferName, data);
        }
        else
        {
            logger.AppendData(temporaryWaferName, data);
        }

        var sourcePath = logger.GetFilePath(temporaryWaferName, subcategory);
        var sourceContent = File.ReadAllText(sourcePath);

        const string finalWaferLogName = "LotA_2(C1W2)";
        Assert.IsTrue(logger.RenameWaferFile(temporaryWaferName, finalWaferLogName, subcategory));

        var destinationFiles = Directory.GetFiles(_basePath, $"{finalWaferLogName}_*.csv", SearchOption.AllDirectories);
        Assert.IsFalse(File.Exists(sourcePath));
        Assert.AreEqual(1, destinationFiles.Length);
        Assert.AreEqual(sourceContent, File.ReadAllText(destinationFiles[0]));
        Assert.IsTrue(Regex.IsMatch(
            Path.GetFileName(destinationFiles[0]),
            @"^LotA_2\(C1W2\)_\d{4}-\d{2}-\d{2}_\d{2}\.\d{2}\.\d{2}\.\d{3}\.csv$"));
    }

    [TestMethod]
    public void RenameWaferFile_DoesNotOverwriteExistingFile()
    {
        var logger = new DataLogger("PolishA", _basePath);
        var data = new Dictionary<string, string?> { ["Time"] = "2026-07-22 12:00:00:000" };

        logger.AppendData("Temporary_First", data);
        logger.AppendData("Temporary_Second", data);
        const string finalWaferLogName = "LotA_2(C1W2)";
        Assert.IsTrue(logger.RenameWaferFile("Temporary_First", finalWaferLogName));
        Assert.IsTrue(logger.RenameWaferFile("Temporary_Second", finalWaferLogName));

        var destinationFiles = Directory.GetFiles(_basePath, $"{finalWaferLogName}_*.csv", SearchOption.AllDirectories);
        Assert.AreEqual(2, destinationFiles.Length);
    }

    [DataTestMethod]
    [DataRow(null, "PolishRecipeA")]
    [DataRow("Exsitu", "PolishRecipeA")]
    public void RenameWaferFile_IncludesPolishRecipeName(string? subcategory, string recipeName)
    {
        var logger = new DataLogger("PolishA", _basePath);
        const string temporaryWaferName = "Temporary_PolishA_recipe";
        var data = new Dictionary<string, string?> { ["Time"] = "2026-07-22 12:00:00:000" };

        if (subcategory == "Exsitu")
        {
            logger.AppendExsituData(temporaryWaferName, data);
        }
        else
        {
            logger.AppendData(temporaryWaferName, data);
        }

        const string finalWaferLogName = "LotA_2(C1W1)";
        Assert.IsTrue(logger.RenameWaferFile(temporaryWaferName, finalWaferLogName, subcategory, recipeName));

        var destinationFiles = Directory.GetFiles(_basePath, $"{finalWaferLogName}_{recipeName}_*.csv", SearchOption.AllDirectories);
        Assert.AreEqual(1, destinationFiles.Length);
    }

    [DataTestMethod]
    [DataRow("Polish")]
    [DataRow("Load")]
    [DataRow("Unload")]
    [DataRow("Dechuck")]
    public void AppendData_WritesPolishStepIntoItsOwnFolder(string subcategory)
    {
        var logger = new DataLogger("PolishA", _basePath);
        logger.AppendData("LotA_2(C1W1)", new Dictionary<string, string?> { ["Time"] = "T1" }, subcategory);

        var path = logger.GetFilePath("LotA_2(C1W1)", subcategory);
        Assert.IsTrue(File.Exists(path));
        Assert.IsTrue(path.Contains(Path.Combine("PolishA", subcategory) + Path.DirectorySeparatorChar));
    }

    [TestMethod]
    public void AppendData_PreservesDuplicateHeadersAndValuesWhenColumnsExpand()
    {
        var logger = new DataLogger("DryA", _basePath);
        const string waferLogName = "LotA_2(C1W2)";

        logger.AppendData(waferLogName,
        [
            new DataLogField("Time", "Time", "T1"),
            new DataLogField("DryAField1", "_", "A1"),
            new DataLogField("DryAField2", "_", "B1")
        ]);

        logger.AppendData(waferLogName,
        [
            new DataLogField("Time", "Time", "T2"),
            new DataLogField("DryAField1", "_", "A2"),
            new DataLogField("DryAField2", "_", "B2"),
            new DataLogField("DryAField3", "FLC_Name(unit)", "C2")
        ]);

        var filePath = logger.GetFilePath(waferLogName);
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        Assert.IsTrue(csv.Read());
        csv.ReadHeader();
        CollectionAssert.AreEqual(
            new[] { "Time", "_", "_", "FLC_Name(unit)" },
            csv.HeaderRecord);

        var rows = new List<string[]>();
        while (csv.Read())
        {
            rows.Add(Enumerable.Range(0, 4).Select(csv.GetField).Select(value => value ?? string.Empty).ToArray());
        }

        Assert.AreEqual(2, rows.Count);
        CollectionAssert.AreEqual(new[] { "T1", "A1", "B1", "" }, rows[0]);
        CollectionAssert.AreEqual(new[] { "T2", "A2", "B2", "C2" }, rows[1]);
    }
}
