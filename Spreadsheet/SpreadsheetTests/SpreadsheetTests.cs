// <copyright file="SpreadsheetTests.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

namespace SpreadsheetTests;

using Spreadsheets;
using Formula;

[TestClass]
public sealed class SpreadsheetTests
{
    [TestMethod]
    public void NewSpreadsheet_HasNoNonemptyCells()
    {
        Spreadsheet s = new();
        Assert.AreEqual(0, s.GetNamesOfAllNonemptyCells().Count);
    }

    [TestMethod]
    public void GetCellContents_NeverSetCell_ReturnsEmptyString()
    {
        Spreadsheet s = new();
        Assert.AreEqual(string.Empty, s.GetCellContents("A1"));
        Assert.AreEqual(string.Empty, s.GetCellContents("zz99"));
    }

    [TestMethod]
    public void GetNamesOfAllNonemptyCells_ReturnsCopy()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 1.0);

        ISet<string> names = s.GetNamesOfAllNonemptyCells();
        names.Add("B2");
        names.Remove("A1");

        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] { "A1" });
    }


    [TestMethod]
    public void NullName_AllOperationsThrowInvalidName()
    {
        Spreadsheet s = new();

        Assert.Throws<InvalidNameException>(() => { s.GetCellContents(null!); });
        Assert.Throws<InvalidNameException>(() => { s.SetCellContents(null!, 1.0); });
        Assert.Throws<InvalidNameException>(() => { s.SetCellContents(null!, "text"); });
        Assert.Throws<InvalidNameException>(() => { s.SetCellContents(null!, new Formula("1")); });
    }

    public void ValidName_IsAcceptedAndNormalized(string name, string normalized)
    {
        Spreadsheet s = new();
        s.SetCellContents(name, 3.0);

        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] {normalized});
        Assert.AreEqual(3.0, s.GetCellContents(name));
        Assert.AreEqual(3.0, s.GetCellContents(normalized));
    }

    [TestMethod]
    public void CellNames_AreCaseInsensitive()
    {
        Spreadsheet s = new();
        s.SetCellContents("a1", 1.0);
        s.SetCellContents("A1", 2.0);

        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] { "A1" });
        Assert.AreEqual(2.0, s.GetCellContents("a1"));
    }

    [TestMethod]
    public void LeadingZeros_ProduceDistinctCells()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 1.0);
        s.SetCellContents("A01", 2.0);

        Assert.AreEqual(2, s.GetNamesOfAllNonemptyCells().Count);
        Assert.AreEqual(1.0, s.GetCellContents("A1"));
        Assert.AreEqual(2.0, s.GetCellContents("A01"));
    }

    [TestMethod]
    public void SetAndGet_Number()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 5.5);
        Assert.AreEqual(5.5, s.GetCellContents("A1"));
        Assert.IsInstanceOfType<double>(s.GetCellContents("A1"));
    }

    [TestMethod]
    public void SetAndGet_ZeroAndNegative()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 0.0);
        s.SetCellContents("B1", -3.25);

        Assert.AreEqual(0.0, s.GetCellContents("A1"));
        Assert.AreEqual(-3.25, s.GetCellContents("B1"));
        Assert.AreEqual(2, s.GetNamesOfAllNonemptyCells().Count);
    }

    [TestMethod]
    public void SetAndGet_Text()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", "hello");
        Assert.AreEqual("hello", s.GetCellContents("A1"));
    }

    [TestMethod]
    public void SetAndGet_TextThatLooksLikeOtherThings()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", "5");
        s.SetCellContents("B1", "=A1+1");

        Assert.IsInstanceOfType<string>(s.GetCellContents("A1"));
        Assert.AreEqual("=A1+1", s.GetCellContents("B1"));
        CollectionAssert.AreEquivalent(s.SetCellContents("A1", "6").ToList(), new[] { "A1" });
    }

    [TestMethod]
    public void SetText_Whitespace_IsNonempty()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", "   ");

        Assert.AreEqual("   ", s.GetCellContents("A1"));
        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] { "A1" });
    }

    [TestMethod]
    public void SetAndGet_Formula()
    {
        Spreadsheet s = new();
        Formula f = new("A2+11");
        s.SetCellContents("A1", f);

        Assert.IsInstanceOfType<Formula>(s.GetCellContents("A1"));
        Assert.AreSame(f, s.GetCellContents("A1"));
    }

    [TestMethod]
    public void SetFormula_NoVariables()
    {
        Spreadsheet s = new();
        IList<string> result = s.SetCellContents("A1", new Formula("2+5"));

        CollectionAssert.AreEquivalent(result.ToList(), new[] { "A1" });
        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] { "A1" });
    }

    [TestMethod]
    public void SetFormula_ReferencedCellsStayEmpty()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", new Formula("B1+C1"));

        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] { "A1" });
        Assert.AreEqual(string.Empty, s.GetCellContents("B1"));
    }

    [TestMethod]
    public void Overwrite_ChangesTypeAndKeepsOneCell()
    {
        Spreadsheet s = new();
        Formula f = new("1+1");

        s.SetCellContents("A1", 1.0);
        s.SetCellContents("A1", "text");
        Assert.AreEqual("text", s.GetCellContents("A1"));

        s.SetCellContents("A1", f);
        Assert.AreSame(f, s.GetCellContents("A1"));

        s.SetCellContents("A1", 9.0);
        Assert.AreEqual(9.0, s.GetCellContents("A1"));

        Assert.AreEqual(1, s.GetNamesOfAllNonemptyCells().Count);
    }

    [TestMethod]
    public void SetEmptyString_RemovesCell()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 5.0);
        s.SetCellContents("B1", 6.0);

        s.SetCellContents("A1", string.Empty);

        Assert.AreEqual(string.Empty, s.GetCellContents("A1"));
        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] { "B1" });
    }

    [TestMethod]
    public void SetEmptyString_OnNeverSetCell_StaysEmpty()
    {
        Spreadsheet s = new();
        IList<string> result = s.SetCellContents("A1", string.Empty);

        CollectionAssert.AreEquivalent(result.ToList(), new[] { "A1" });
        Assert.AreEqual(0, s.GetNamesOfAllNonemptyCells().Count);
    }

    [TestMethod]
    public void Set_IndependentCell_ReturnsOnlyItself()
    {
        Spreadsheet s = new();
        CollectionAssert.AreEqual(new[] { "A1" }, s.SetCellContents("A1", 1.0).ToList());
        CollectionAssert.AreEqual(new[] { "B1" }, s.SetCellContents("B1", "x").ToList());
        CollectionAssert.AreEqual(new[] { "C1" }, s.SetCellContents("C1", new Formula("5")).ToList());
    }

    [TestMethod]
    public void Set_LowercaseName_ReturnsNormalizedName()
    {
        Spreadsheet s = new();
        s.SetCellContents("b1", new Formula("a1*2"));

        IList<string> result = s.SetCellContents("a1", 1.0);

        CollectionAssert.AreEqual(new[] { "A1", "B1" }, result.ToList());
    }

    [TestMethod]
    public void Set_DocumentationExample_ReturnsA1B1C1()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1*2"));
        s.SetCellContents("C1", new Formula("B1+A1"));

        IList<string> result = s.SetCellContents("A1", 3.0);

        CollectionAssert.AreEqual(new[] { "A1", "B1", "C1" }, result.ToList());
    }

    [TestMethod]
    public void Set_RecalculateDocumentationExample()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 5.0);
        s.SetCellContents("B1", new Formula("A1+2"));
        s.SetCellContents("C1", new Formula("A1+B1"));
        s.SetCellContents("D1", new Formula("A1*7"));
        s.SetCellContents("E1", 15.0);

        IList<string> result = s.SetCellContents("A1", 6.0);

        CollectionAssert.AreEquivalent(result.ToList(), new[] { "A1", "B1", "C1", "D1" });
        Assert.AreEqual("A1", result[0]);
        CollectionAssert.Contains(result.ToList(), "B1");
        CollectionAssert.Contains(result.ToList(), "C1");
        Assert.IsTrue(result.IndexOf("B1") < result.IndexOf("C1"));
    }

    [TestMethod]
    public void Set_DependentCell_DoesNotIncludeDependees()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1*2"));
        s.SetCellContents("C1", new Formula("B1+A1"));

        CollectionAssert.AreEqual(new[] { "C1" }, s.SetCellContents("C1", 1.0).ToList());
    }

    [TestMethod]
    public void Set_IncludesIndirectDependents()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1"));
        s.SetCellContents("C1", new Formula("B1"));
        s.SetCellContents("D1", new Formula("C1"));

        CollectionAssert.AreEqual(new[] { "A1", "B1", "C1", "D1" }, s.SetCellContents("A1", 1.0).ToList());
    }

    [TestMethod]
    public void Set_Diamond_NoDuplicatesAndValidOrder()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1"));
        s.SetCellContents("C1", new Formula("A1"));
        s.SetCellContents("D1", new Formula("B1+C1"));

        IList<string> result = s.SetCellContents("A1", 1.0);

        CollectionAssert.AreEquivalent(result.ToList(),new[] {"A1", "B1", "C1", "D1"});
        Assert.AreEqual("A1", result[0]);
        CollectionAssert.Contains(result.ToList(), "B1");
        CollectionAssert.Contains(result.ToList(), "D1");
        Assert.IsTrue(result.IndexOf("B1") < result.IndexOf("D1"));
        
        CollectionAssert.Contains(result.ToList(), "C1");
        CollectionAssert.Contains(result.ToList(), "D1");
        Assert.IsTrue(result.IndexOf("C1") < result.IndexOf("D1"));
    }

    [TestMethod]
    public void Set_FormulaWithRepeatedVariable_NoDuplicates()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1+A1*A1"));

        CollectionAssert.AreEqual(new[] { "A1", "B1" }, s.SetCellContents("A1", 2.0).ToList());
    }

    [TestMethod]
    public void Set_EmptyCellWithDependents_ReturnsDependents()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1+1"));

        CollectionAssert.AreEqual(new[] { "A1", "B1" }, s.SetCellContents("A1", "text").ToList());
    }

    [TestMethod]
    public void Set_FormulaWithLowercaseVariable_DependsOnNormalizedCell()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("a1+1"));

        CollectionAssert.AreEqual(new[] { "A1", "B1" }, s.SetCellContents("A1", 1.0).ToList());
    }

    [TestMethod]
    public void SetEmptyString_ReturnsDependents()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 1.0);
        s.SetCellContents("B1", new Formula("A1"));

        CollectionAssert.AreEqual(new[] { "A1", "B1" }, s.SetCellContents("A1", string.Empty).ToList());
    }

    [TestMethod]
    public void ReplaceFormulaWithNumber_ClearsDependencies()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1+1"));
        s.SetCellContents("B1", 10.0);

        CollectionAssert.AreEqual(new[] { "A1" }, s.SetCellContents("A1", 1.0).ToList());
    }

    [TestMethod]
    public void ReplaceFormulaWithText_ClearsDependencies()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1+1"));
        s.SetCellContents("B1", "text");

        CollectionAssert.AreEqual(new[] { "A1" }, s.SetCellContents("A1", 1.0).ToList());
    }

    [TestMethod]
    public void ReplaceFormulaWithEmpty_ClearsDependencies()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1+1"));
        s.SetCellContents("B1", string.Empty);

        CollectionAssert.AreEqual(new[] { "A1" }, s.SetCellContents("A1", 1.0).ToList());
    }

    [TestMethod]
    public void ReplaceFormulaWithFormula_MovesDependencies()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1"));
        s.SetCellContents("B1", new Formula("C1"));

        CollectionAssert.AreEqual(new[] { "A1" }, s.SetCellContents("A1", 1.0).ToList());
        CollectionAssert.AreEqual(new[] { "C1", "B1" }, s.SetCellContents("C1", 1.0).ToList());
    }

    [TestMethod]
    public void ReplaceFormulaWithSameFormula_NoDuplicates()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1"));
        s.SetCellContents("B1", new Formula("A1"));

        CollectionAssert.AreEqual(new[] { "A1", "B1" }, s.SetCellContents("A1", 1.0).ToList());
    }

    [TestMethod]
    public void Circular_SelfReference_Throws()
    {
        Spreadsheet s = new();
        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("A1", new Formula("A1+1")); });
    }

    [TestMethod]
    public void Circular_SelfReference_LeavesSpreadsheetEmpty()
    {
        Spreadsheet s = new();
        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("A1", new Formula("A1")); });

        Assert.AreEqual(0, s.GetNamesOfAllNonemptyCells().Count);
        Assert.AreEqual(string.Empty, s.GetCellContents("A1"));
        CollectionAssert.AreEqual(new[] { "A1" }, s.SetCellContents("A1", 1.0).ToList());
    }

    [TestMethod]
    public void Circular_TwoCells_Throws()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", new Formula("B1"));
        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("B1", new Formula("A1")); });
    }

    [TestMethod]
    public void Circular_ThreeCells_Throws()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", new Formula("B1*2"));
        s.SetCellContents("B1", new Formula("C1*2"));
        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("C1", new Formula("A1*2")); });
    }

    [TestMethod]
    public void Circular_MixedCase_Throws()
    {
        Spreadsheet s = new();
        s.SetCellContents("a1", new Formula("b1"));
        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("B1", new Formula("a1")); });
    }

    [TestMethod]
    public void Circular_OneOfSeveralVariables_Throws()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", new Formula("B1+1"));
        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("B1", new Formula("X1+Y1+A1")); });
    }

    [TestMethod]
    public void Diamond_IsNotCircular()
    {
        Spreadsheet s = new();
        s.SetCellContents("B1", new Formula("A1"));
        s.SetCellContents("C1", new Formula("A1"));
        s.SetCellContents("D1", new Formula("B1+C1"));

        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] {"B1", "C1", "D1" });
    }

    [TestMethod]
    public void Circular_FailedSet_KeepsOldContents()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 5.0);

        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("A1", new Formula("A1+1")); });

        Assert.AreEqual(5.0, s.GetCellContents("A1"));
        CollectionAssert.AreEquivalent(s.GetNamesOfAllNonemptyCells().ToList(), new[] { "A1" });
    }

    [TestMethod]
    public void Circular_FailedSet_LeavesDependenciesIntact()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", 1.0);
        s.SetCellContents("B1", new Formula("A1+1"));

        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("A1", new Formula("B1")); });

        Assert.AreEqual(1.0, s.GetCellContents("A1"));
        CollectionAssert.AreEqual(new[] { "A1", "B1" }, s.SetCellContents("A1", 2.0).ToList());
        CollectionAssert.AreEqual(new[] { "B1" }, s.SetCellContents("B1", 2.0).ToList());
    }
    
    [TestMethod]
    public void Circular_FailedSet_RestoresOldFormulaDependencies()
    {
        Spreadsheet s = new();
        Formula original = new("C1+1");
        s.SetCellContents("A1", original);
        s.SetCellContents("B1", new Formula("A1"));

        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("A1", new Formula("B1")); });

        Assert.AreSame(original, s.GetCellContents("A1"));
        IList<string> result = s.SetCellContents("C1", 1.0);
        CollectionAssert.AreEqual(new[] { "C1", "A1", "B1" }, result.ToList());
    }

    [TestMethod]
    public void Circular_SpreadsheetUsableAfterFailure()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", new Formula("B1"));
        Assert.ThrowsExactly<CircularException>(() => { s.SetCellContents("B1", new Formula("A1")); });

        s.SetCellContents("B1", 4.0);

        Assert.AreEqual(4.0, s.GetCellContents("B1"));
        CollectionAssert.AreEqual(new[] { "B1", "A1" }, s.SetCellContents("B1", 5.0).ToList());
    }

    [TestMethod] public void Reverse_DirectionAfterClearing_Valid()
    {
        Spreadsheet s = new();
        s.SetCellContents("A1", new Formula("B1"));
        s.SetCellContents("A1", string.Empty);

        s.SetCellContents("B1", new Formula("A1"));

        CollectionAssert.AreEqual(new[] { "A1", "B1" }, s.SetCellContents("A1", 1.0).ToList());
    }

}