// <copyright file="SpreadsheetTests.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

namespace SpreadsheetTests;

using Formula;
using Spreadsheets;

[TestClass]
public class SpreadsheetTests
{
    // ---------- constructor / basic state ----------

    [TestMethod]
    public void Constructor_Default_IsEmptyAndUnchanged()
    {
        Spreadsheet ss = new();
        Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count);
        Assert.IsFalse(ss.Changed);
    }

    [TestMethod]
    public void GetCellContents_EmptyCell_ReturnsEmptyString()
    {
        Spreadsheet ss = new();
        Assert.AreEqual(string.Empty, ss.GetCellContents("A1"));
    }

    [TestMethod]
    public void SetContentsOfCell_LowerCaseName_StoredAsUpperCase()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("a1", "5");
        Assert.AreEqual(5.0, ss.GetCellContents("A1"));
        Assert.AreEqual(5.0, ss.GetCellContents("a1"));
        Assert.IsTrue(ss.GetNamesOfAllNonemptyCells().Contains("A1"));
    }

    [TestMethod]
    public void AnyMethod_InvalidName_ThrowsInvalidNameException()
    {
        Spreadsheet ss = new();
        Assert.ThrowsExactly<InvalidNameException>(() => ss.GetCellContents("1A"));
        Assert.ThrowsExactly<InvalidNameException>(() => ss.GetCellValue("A"));
        Assert.ThrowsExactly<InvalidNameException>(() => ss["A-1"]);
        Assert.ThrowsExactly<InvalidNameException>(() => ss.SetContentsOfCell("", "5"));
        Assert.ThrowsExactly<InvalidNameException>(() => ss.SetContentsOfCell(null!, "5"));
    }

    [TestMethod]
    public void SetContentsOfCell_NullContent_ThrowsArgumentNullException()
    {
        Spreadsheet ss = new();
        Assert.ThrowsExactly<ArgumentNullException>(() => ss.SetContentsOfCell("A1", null!));
    }

    // ---------- SetContentsOfCell parsing ----------

    [TestMethod]
    public void SetContentsOfCell_Number_ContentsIsDouble()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "2.5");
        Assert.AreEqual(2.5, ss.GetCellContents("A1"));
        Assert.AreEqual(2.5, ss.GetCellValue("A1"));
    }

    [TestMethod]
    public void SetContentsOfCell_ScientificNotation_ContentsIsDouble()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "1e3");
        Assert.AreEqual(1000.0, ss.GetCellContents("A1"));
    }


    [TestMethod]
    public void SetContentsOfCell_Text_ContentsIsString()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "hello");
        Assert.AreEqual("hello", ss.GetCellContents("A1"));
        Assert.AreEqual("hello", ss.GetCellValue("A1"));
    }

    [TestMethod]
    public void SetContentsOfCell_NonFiniteText_ContentsIsString()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "Infinity");
        ss.SetContentsOfCell("A2", "NaN");
        Assert.AreEqual("Infinity", ss.GetCellContents("A1"));
        Assert.AreEqual("NaN", ss.GetCellContents("A2"));
    }

    [TestMethod]
    public void SetContentsOfCell_Formula_ContentsIsFormula()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("B1", "=A1+2");
        Assert.IsInstanceOfType(ss.GetCellContents("B1"), typeof(Formula));
        Assert.AreEqual("A1+2", ss.GetCellContents("B1").ToString());
    }

    [TestMethod]
    public void SetContentsOfCell_BadFormula_ThrowsFormulaFormatException()
    {
        Spreadsheet ss = new();
        Assert.ThrowsExactly<FormulaFormatException>(() => ss.SetContentsOfCell("A1", "=1+"));
        Assert.AreEqual(string.Empty, ss.GetCellContents("A1"));
        Assert.IsFalse(ss.Changed);
    }

    [TestMethod]
    public void SetContentsOfCell_EmptyString_RemovesCell()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        ss.SetContentsOfCell("A1", string.Empty);
        Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count);
        Assert.AreEqual(string.Empty, ss.GetCellValue("A1"));
    }

    [TestMethod]
    public void SetContentsOfCell_FormulaReplacedByNumber_DropsDependencies()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("B1", "=A1+1");
        ss.SetContentsOfCell("B1", "10");
        IList<string> result = ss.SetContentsOfCell("A1", "3");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(10.0, ss.GetCellValue("B1"));
    }

    // ---------- returned recalculation list ----------

    [TestMethod]
    public void SetContentsOfCell_DependentCells_ReturnsDependencyOrder()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("B1", "=A1*2");
        ss.SetContentsOfCell("C1", "=B1+A1");
        IList<string> result = ss.SetContentsOfCell("A1", "3");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("A1", result[0]);
        Assert.AreEqual("B1", result[1]);
        Assert.AreEqual("C1", result[2]);
    }
    
    [TestMethod]
    public void SetContentsOfCell_NoDependents_ReturnsOnlyItself()
    {
        Spreadsheet ss = new();
        IList<string> result = ss.SetContentsOfCell("A1", "hello");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("A1", result[0]);
    }

    // ---------- values ----------

    [TestMethod]
    public void GetCellValue_Formula_ReturnsEvaluatedValue()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        ss.SetContentsOfCell("B1", "=A1*2");
        Assert.AreEqual(10.0, ss.GetCellValue("B1"));
        Assert.AreEqual(10.0, ss["B1"]);
    }

    [TestMethod]
    public void GetCellValue_DependeeChanged_ValueUpdates()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        ss.SetContentsOfCell("B1", "=A1*2");
        ss.SetContentsOfCell("C1", "=B1+A1");
        Assert.AreEqual(15.0, ss.GetCellValue("C1"));

        ss.SetContentsOfCell("A1", "1");
        Assert.AreEqual(2.0, ss.GetCellValue("B1"));
        Assert.AreEqual(3.0, ss.GetCellValue("C1"));
    }

    [TestMethod]
    public void GetCellValue_FormulaSetBeforeDependee_ValueFixedLater()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("B1", "=A1+1");
        Assert.IsTrue(ss.GetCellValue("B1") is FormulaError);

        ss.SetContentsOfCell("A1", "1");
        Assert.AreEqual(2.0, ss.GetCellValue("B1"));
    }
    
    [TestMethod]
    public void GetCellValue_EmptyCell_ReturnsEmptyString()
    {
        Spreadsheet ss = new();
        Assert.AreEqual(string.Empty, ss.GetCellValue("Z99"));
    }
    
    [TestMethod]
    public void GetCellValue_FormulaUsesStringCell_ReturnsFormulaError()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "hello");
        ss.SetContentsOfCell("B1", "=A1+1");
        Assert.IsTrue(ss.GetCellValue("B1") is FormulaError);
    }
    
    [TestMethod]
    public void GetCellValue_DivideByZero_ReturnsFormulaError()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "=1/0");
        Assert.IsTrue(ss.GetCellValue("A1") is FormulaError);
    }


    [TestMethod]
    public void GetCellValue_FormulaUsesErrorCell_ReturnsFormulaError()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "=1/0");
        ss.SetContentsOfCell("B1", "=A1+1");
        Assert.IsTrue(ss.GetCellValue("B1") is FormulaError);
    }

    [TestMethod]
    public void SetContentsOfCell_DependeeEmptied_DependentBecomesFormulaError()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        ss.SetContentsOfCell("B1", "=A1+1");
        Assert.AreEqual(6.0, ss.GetCellValue("B1"));

        ss.SetContentsOfCell("A1", string.Empty);
        Assert.IsTrue(ss.GetCellValue("B1") is FormulaError);
    }

    [TestMethod]
    public void GetCellValue_DiamondDependency_ValuesCorrect()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "1");
        ss.SetContentsOfCell("B1", "=A1+1");
        ss.SetContentsOfCell("C1", "=A1+1");
        ss.SetContentsOfCell("D1", "=B1+C1");
        Assert.AreEqual(4.0, ss.GetCellValue("D1"));

        ss.SetContentsOfCell("A1", "2");
        Assert.AreEqual(6.0, ss.GetCellValue("D1"));
    }

    // ---------- circular dependencies ----------

    [TestMethod]
    public void SetContentsOfCell_SelfReference_ThrowsCircularException()
    {
        Spreadsheet ss = new();
        Assert.ThrowsExactly<CircularException>(() => ss.SetContentsOfCell("A1", "=A1+1"));
        Assert.AreEqual(string.Empty, ss.GetCellContents("A1"));
    }

    [TestMethod]
    public void SetContentsOfCell_Circular_NoChangeMade()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "=B1");
        ss.SetContentsOfCell("B1", "=C1");
        Assert.ThrowsExactly<CircularException>(() => ss.SetContentsOfCell("C1", "=A1"));
        Assert.AreEqual(string.Empty, ss.GetCellContents("C1"));

        // old dependencies must still be intact
        IList<string> result = ss.SetContentsOfCell("C1", "5");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(5.0, ss.GetCellValue("A1"));
    }

    [TestMethod]
    public void SetContentsOfCell_Circular_ChangedStaysFalse()
    {
        Spreadsheet ss = new();
        Assert.ThrowsExactly<CircularException>(() => ss.SetContentsOfCell("A1", "=A1"));
        Assert.IsFalse(ss.Changed);
    }

    // ---------- Changed ----------

    [TestMethod]
    public void Changed_AfterSet_IsTrue()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        Assert.IsTrue(ss.Changed);
    }
    
    [TestMethod]
    public void Changed_AfterSave_IsFalseUntilNextChange()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        ss.Save("changed_test.txt");
        Assert.IsFalse(ss.Changed);

        ss.SetContentsOfCell("A1", "6");
        Assert.IsTrue(ss.Changed);
    }

    // ---------- Save ----------

    [TestMethod]
    public void Save_EmptySpreadsheet_WritesEmptyCells()
    {
        Spreadsheet ss = new();
        ss.Save("save_empty.txt");
        string text = File.ReadAllText("save_empty.txt");
        Assert.IsTrue(text.Contains("\"Cells\": {}"));
    }

    [TestMethod]
    public void Save_ExistingFile_IsOverwritten()
    {
        File.WriteAllText("save_overwrite.txt", "old junk");

        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "7");
        ss.Save("save_overwrite.txt");

        Spreadsheet loaded = new("save_overwrite.txt");
        Assert.AreEqual(7.0, loaded.GetCellContents("A1"));
    }

    [TestMethod]
    public void Save_BadPath_ThrowsSpreadsheetReadWriteException()
    {
        Spreadsheet ss = new();
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => ss.Save("/some/nonsense/path.txt"));
    }

    [TestMethod]
    public void Save_BadPath_ChangedStaysTrue()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => ss.Save("/some/nonsense/path.txt"));
        Assert.IsTrue(ss.Changed);
    }

    // ---------- load from file ----------

    [TestMethod]
    public void Constructor_SaveThenLoad_SameSpreadsheet()
    {
        Spreadsheet ss = new();
        ss.SetContentsOfCell("A1", "5");
        ss.SetContentsOfCell("B3", "=A1+2");
        ss.SetContentsOfCell("C4", "hello");
        ss.Save("roundtrip.txt");

        Spreadsheet loaded = new("roundtrip.txt");
        Assert.AreEqual(3, loaded.GetNamesOfAllNonemptyCells().Count);
        Assert.AreEqual(5.0, loaded.GetCellContents("A1"));
        Assert.AreEqual("A1+2", loaded.GetCellContents("B3").ToString());
        Assert.AreEqual("hello", loaded.GetCellContents("C4"));
        Assert.AreEqual(7.0, loaded.GetCellValue("B3"));
        Assert.IsFalse(loaded.Changed);
    }

    [TestMethod]
    public void Constructor_ValidFile_LoadsCells()
    {
        string sheet = """
        {
          "Cells": {
            "A1": { "StringForm": "5" },
            "B1": { "StringForm": "=A1*2" }
          }
        }
        """;
        File.WriteAllText("load_valid.txt", sheet);

        Spreadsheet ss = new("load_valid.txt");
        Assert.AreEqual(10.0, ss.GetCellValue("B1"));
    }

    [TestMethod]
    public void Constructor_NoCells_LoadsEmptySpreadsheet()
    {
        File.WriteAllText("load_empty.txt", "{ \"Cells\" : {} }");

        Spreadsheet ss = new("load_empty.txt");
        Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count);
    }

    [TestMethod]
    public void Constructor_MissingFile_ThrowsSpreadsheetReadWriteException()
    {
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("/some/nonsense/missing.txt"));
    }

    [TestMethod]
    public void Constructor_NotJson_ThrowsSpreadsheetReadWriteException()
    {
        File.WriteAllText("load_notjson.txt", "this is not json");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_notjson.txt"));
    }

    [TestMethod]
    public void Constructor_NullJson_ThrowsSpreadsheetReadWriteException()
    {
        File.WriteAllText("load_null.txt", "null");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_null.txt"));
    }

    [TestMethod]
    public void Constructor_NullCells_ThrowsSpreadsheetReadWriteException()
    {
        File.WriteAllText("load_nullcells.txt", "{ \"Cells\" : null }");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_nullcells.txt"));
    }

    [TestMethod]
    public void Constructor_NullCell_ThrowsSpreadsheetReadWriteException()
    {
        File.WriteAllText("load_nullcell.txt", "{ \"Cells\" : { \"A1\" : null } }");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_nullcell.txt"));
    }

    [TestMethod]
    public void Constructor_NullStringForm_ThrowsSpreadsheetReadWriteException()
    {
        File.WriteAllText("load_nullform.txt", "{ \"Cells\" : { \"A1\" : { \"StringForm\" : null } } }");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_nullform.txt"));
    }

    [TestMethod]
    public void Constructor_InvalidCellName_ThrowsSpreadsheetReadWriteException()
    {
        File.WriteAllText("load_badname.txt", "{ \"Cells\" : { \"1A\" : { \"StringForm\" : \"5\" } } }");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_badname.txt"));
    }

    [TestMethod]
    public void Constructor_InvalidFormula_ThrowsSpreadsheetReadWriteException()
    {
        File.WriteAllText("load_badformula.txt", "{ \"Cells\" : { \"A1\" : { \"StringForm\" : \"=1+\" } } }");
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_badformula.txt"));
    }


    [TestMethod]
    public void Constructor_CircularDependency_ThrowsSpreadsheetReadWriteException()
    {
        string sheet = """
        {
          "Cells": {
            "A1": { "StringForm": "=B1" },
            "B1": { "StringForm": "=A1" }
          }
        }
        """;
        File.WriteAllText("load_circular.txt", sheet);
        Assert.ThrowsExactly<SpreadsheetReadWriteException>(() => new Spreadsheet("load_circular.txt"));
    }

    // ---------- stress tests ----------


    [TestMethod]
    [Timeout(10000)]
    public void Stress_LongDependencyChain_ValuesCorrect()
    {
        Spreadsheet ss = new();
        int size = 2000;

        ss.SetContentsOfCell("A1", "1");
        for (int i = 2; i <= size; i++)
        {
            ss.SetContentsOfCell("A" + i, "=A" + (i - 1) + "+1");
        }

        Assert.AreEqual((double)size, ss.GetCellValue("A" + size));

        // changing the start of the chain has to update the whole chain
        ss.SetContentsOfCell("A1", "2");
        Assert.AreEqual((double)(size + 1), ss.GetCellValue("A" + size));
    }
    
    [TestMethod]
    [Timeout(10000)]
    public void Stress_ManyDependents_ValuesCorrect()
    {
        Spreadsheet ss = new();
        int size = 5000;

        ss.SetContentsOfCell("A1", "1");
        for (int i = 1; i <= size; i++)
        {
            ss.SetContentsOfCell("B" + i, "=A1+" + i);
        }

        IList<string> result = ss.SetContentsOfCell("A1", "10");
        Assert.AreEqual(size + 1, result.Count);
        Assert.AreEqual(10.0 + size, ss.GetCellValue("B" + size));
    }

    [TestMethod]
    [Timeout(10000)]
    public void Stress_ManyCells_SaveAndLoad()
    {
        Spreadsheet ss = new();
        int size = 5000;

        for (int i = 1; i <= size; i++)
        {
            ss.SetContentsOfCell("A" + i, i.ToString());
        }

        ss.Save("stress_save.txt");
        Spreadsheet loaded = new("stress_save.txt");

        Assert.AreEqual(size, loaded.GetNamesOfAllNonemptyCells().Count);
        Assert.AreEqual((double)size, loaded.GetCellValue("A" + size));
    }
}