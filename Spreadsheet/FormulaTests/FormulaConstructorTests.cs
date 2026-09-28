// <copyright file="FormulaSyntaxTests.cs" company="UofU-CS3500">
//   Copyright 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <authors> Pridhvi Subgani </authors>
// <date> September 3, 2026 </date>

namespace FormulaTests;

using Formula; // Change this using statement to use different formula implementations.

/// <summary>
///   <para>
///     The following class shows the basics of how to use the MSTest framework,
///     including:
///   </para>
///   <list type="number">
///     <item> How to catch exceptions. </item>
///     <item> How a test of valid code should look. </item>
///   </list>
/// </summary>
[TestClass]
public class FormulaSyntaxTests {
    // --- Tests for One Token Rule ---
    /// <summary>
    ///   <para>
    ///     This test makes sure the right kind of exception is thrown
    ///     when trying to create a formula with no tokens.
    ///   </para>
    ///   <remarks>
    ///     <list type="bullet">
    ///       <item>
    ///         We use the _ (discard) notation because the formula object
    ///         is not used after that point in the method.  Note: you can also
    ///         use _ when a method must match an interface but does not use
    ///         some of the required arguments to that method.
    ///       </item>
    ///       <item>
    ///         string.Empty is often considered best practice (rather than using "") because it
    ///         is explicit in intent (e.g., perhaps the coder forgot to but something in "").
    ///       </item>
    ///       <item>
    ///         The name of a test method should follow the MS standard:
    ///         https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
    ///       </item>
    ///       <item>
    ///         All methods should be documented, but perhaps not to the same extent
    ///         as this one.  The remarks here are for your educational
    ///         purposes (i.e., a developer would assume another developer would know these
    ///         items) and would be superfluous in your code.
    ///       </item>
    ///       <item>
    ///         Notice the use of the attribute tag [ExpectedException] which tells the test
    ///         that the code should throw an exception, and if it doesn't an error has occurred;
    ///         i.e., the correct implementation of the constructor should result
    ///         in this exception being thrown based on the given poorly formed formula.
    ///       </item>
    ///     </list>
    ///   </remarks>
    ///   <example>
    ///     <code>
    ///        // here is how we call the formula constructor with a string representing the formula
    ///        _ = new Formula( "5+5" );
    ///     </code>
    ///   </example>
    /// </summary>
    
    // --- Test for One Token Rule ---
    
    [TestMethod]
    public void FormulaConstructor_TestNoTokens_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(string.Empty));
    }
    
    // --- Tests for Valid Token Rule ---

    [TestMethod]
    public void FormulaConstructor_TestTokens_Valid() {
        _ = new Formula("(8+2)/4*2-D4");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestSymbolTokens_Invalid() {
        // initialize array of invalid tokens
        string[] arr = {"`","~","!","@","#","$","%","^","&", "_", "<", ">", ",","\""};
        
        // boolean to keep track of whether this test should fail
        bool fail = false;
        
        // iterate through the entire array
            foreach (var i in arr) {
                try
                {
                    // add each symbol to the statement, if it catches an expected FormulaFormatException
                    // it will continue
                    _ = new Formula("2+A2-D4" + i);
                }
                catch (FormulaFormatException)
                {
                    continue;
                }
                
                // if it doesn't catch, the character which has failed the check will be printed
                // and the boolean will change so we can manually fail the test
                Console.WriteLine(i + " is an invalid character, but has not thrown an exception");
                fail = true;
            }
            
            if (fail)
                throw new FormulaFormatException("");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestVariableTokens_Invalid() {
        // using the same concept from the previous test
        
        // array of invalid variable tokens
        string[] arr = {"a", "abc", "1b", "1b2", "a1b", "a1b2"};
        bool fail = false;
        
        // iterate through the entire array
        foreach (var i in arr) {
            try
            {
                // add each variable to the statement, if it catches an expected FormulaFormatException
                // it will continue
                _ = new Formula("2+A2-D4" + i);
            }
            catch (FormulaFormatException)
            {
                continue;
            }
                
            // if it doesn't catch, the token which has failed the check will be printed
            // and the boolean will change so we can manually fail the test
            Console.WriteLine(i + " is an invalid token, but has not thrown an exception");
            fail = true;
        }
            
        if (fail)
            throw new FormulaFormatException("");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestScientificNotation_Valid() {
        _ = new Formula("E2+10.3E4-E11*2E-3");
    }
    
    // --- Tests for Closing Parenthesis Rule

    [TestMethod]
    public void FormulaConstructor_ClosingParentheses_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(2 + 3)*4-3)+D3"));
    }
    
    [TestMethod]
    public void FormulaConstructor_ClosingParentheses_Valid() {
        _ = new Formula("(2+3)*(4-3)+D3");
    }
    
    // --- Tests for Balanced Parentheses Rule ---
    
    [TestMethod]
    public void FormulaConstructor_BalancedParentheses_Valid() {
        _ = new Formula("(A4*3+22)");
        _ = new Formula("(2+3)*(4)-(3)+D3");
    }

    [TestMethod]
    public void FormulaConstructor_BalancedParentheses_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("((2 + 3)"));
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(2 + 3))"));
    }

    [TestMethod]
    public void FormulaConstructor_MultipleBalancedParentheses_Valid() {
        _ = new Formula("(2+3)-(2+3)+(2+3)");
    }

    // --- Tests for First Token Rule

    /// <summary>
    ///   <para>
    ///     Make sure a simple well-formed formula is accepted by the constructor (the constructor
    ///     should not throw an exception).
    ///   </para>
    ///   <remarks>
    ///     This is an example of a test that is not expected to throw an exception, i.e., it succeeds.
    ///     In other words, the formula "1+1" is a valid formula which should not cause any errors.
    ///   </remarks>
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenNumber_Valid() {
        _ = new Formula("1+1");
    }
    
    
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenParenthesis_Valid() {
        _ = new Formula("(1+1)");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenVariable_Valid() {
        _ = new Formula("a11*2e2");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenSpace_Valid() {
        _ = new Formula(" a11");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenClosingParenthesis_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(")2+a11"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenOperators_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("+123"));
    }

    // --- Tests for  Last Token Rule ---
    [TestMethod]
    public void FormulaConstructor_TestLastTokenNumber_Valid() {
        _ = new Formula("b44/11.2e4");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestLastTokenParenthesis_Valid() {
        _ = new Formula("(11.2e4*b44)");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestLastTokenVariable_Valid() {
        _ = new Formula("2e2*a11");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestLastTokenSpace_Valid() {
        _ = new Formula("a11+b2 ");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestLastTokenOpenParenthesis_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("2+a11("));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestLastTokenOperators_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("123+"));
    }
    
    // --- Tests for Parentheses/Operator Following Rule ---
    
    [TestMethod]
    public void FormulaConstructor_TestOpenParenthesisFollowedByNumberToken_Valid() {
        _ = new Formula("(11+a3)/b2");
        _ = new Formula("8.1e2*(11+a3)/b2");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOpenParenthesisFollowedByVariableToken_Valid() {
        _ = new Formula("(a3+11)/b2");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOpenParenthesisFollowedByOpenParenthesisToken_Valid() {
        _ = new Formula("((a3+11)/(42-c3))*((20))");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOpenParenthesisFollowedByOperatorToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(+21)"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOpenParenthesisFollowedByClosedParenthesisToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("()"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowedByNumberToken_Valid() {
        _ = new Formula("1+1");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowedByVariableToken_Valid() {
        _ = new Formula("1+a3");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowedByOpenParenthesisToken_Valid() {
        _ = new Formula("a2+(11*c3)-(b2/18)");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowedByClosingParenthesisToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(a11*)"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowedByOperatorToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("a1++23"));
    }

    // --- Tests for Extra Following Rule ---
    
    [TestMethod]
    public void FormulaConstructor_TestNumberTokenFollowedByOperator_Valid() {
        _ = new Formula("10+8*4-11/30");
    }

    [TestMethod]
    public void FormulaConstructor_TestNumberTokenFollowedByClosingParenthesis_Valid() {
        _ = new Formula("(10)*(8-14)");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestVariableTokenFollowedByOperator_Valid() {
        _ = new Formula("a10+b8*c4-d11/e30");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestVariableTokenFollowedByClosingParenthesis_Valid() {
        _ = new Formula("(a10)*(b8-c14)");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisFollowedByOperator_Valid() {
       _ = new Formula("(11*2)+(4)-(20)*(6)/(12)");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisFollowedByClosingParenthesis_Valid() {
        _ = new Formula("(((a11+b2))/((c3)))");
    }
    
    [TestMethod]
    public void FormulaConstructor_TestNumberTokenFollowedByNumberToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("11 12 + 13"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestNumberTokenFollowedByVariableToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("11 a2 + 13"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestVariableTokenFollowedByNumberToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("a2 12 + 13"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestVariableTokenFollowedByVariableToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("b11 a2 + 13"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisFollowedByNumberToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(a11)2 + 3"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisFollowedByVariableToken_Invalid() {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(a11)b11 + 3"));
    }
    
    [TestMethod]
    public void Formula_TestToStringAlreadyCanonized_Valid() {
        Formula f = new Formula("A1+11*B13/12");
        Assert.AreEqual("A1+11*B13/12", f.ToString());
    }
    
    [TestMethod]
    public void Formula_TestToStringHeavyWhitespace_Valid() {
        Formula f = new Formula("       A1       +   11 *          B13    /  12       ");
        Assert.AreEqual("A1+11*B13/12", f.ToString());
    }
    
    [TestMethod]
    public void Formula_TestToStringDifferentNumberFormats_Valid() {
        Formula f = new Formula("x1 + 1.2e3 * 11E5 - 12e0/2.000");
        Assert.AreEqual("X1+1200*1100000-12/2", f.ToString());
    }

    
    [TestMethod]
    public void Formula_TestToString_Valid() {
        Formula f = new Formula("11.2e3 + a11 * X2 - 1.10/12 + (E2)");
        Assert.AreEqual("11200+A11*X2-1.1/12+(E2)", f.ToString());
    }
   
        [TestMethod] public void Formula_TestGetVariables_Valid() {
            Formula test = new Formula("a1 - 11 * b2 + b33 / c100");
            ISet<string> testHash = test.GetVariables();
            
            ISet<string> expected = new HashSet<string>();
            expected.Add("A1");
            expected.Add("B2");
            expected.Add("B33");
            expected.Add("C100");
            
            Assert.AreEqual(expected.Count,  testHash.Count);
            Assert.AreEqual(true,  testHash.Contains("A1"));
            Assert.AreEqual(true,  testHash.Contains("B2"));
            Assert.AreEqual(true,  testHash.Contains("B33"));
            Assert.AreEqual(true,  testHash.Contains("C100"));
    
        }
    
    [TestMethod]
    public void Formula_TestGetVariablesDuplicates_Valid() {
        Formula test = new Formula("a1 + A1 * a1 - 11 / 2e6 + E2");
        ISet<string> testHash = test.GetVariables();
        
        ISet<string> expected = new HashSet<string>();
        expected.Add("A1");
        expected.Add("E2");
        
        Assert.AreEqual(expected.Count,  testHash.Count);
        Assert.AreEqual(true,  testHash.Contains("A1"));
        Assert.AreEqual(true,  testHash.Contains("E2"))       ;
    }
    
    // --- Tests for Equals method and equality operators

    [TestMethod]
    public void Formula_TestEqualsDifferentFormat_True()
    {
        Formula f = new Formula("    12.00 + A3/  5 * E2 - 12E6 ");
        Formula g = new Formula("12+A3/5*e2-12e6");
        
        Assert.IsTrue(f.Equals(g));
        Assert.IsTrue(f==g);
        Assert.IsFalse(f!=g);
    }
    
    [TestMethod]
    public void Formula_TestEqualsSameFormula_True()
    {
        Formula g = new Formula("12+A3/10*E7");
        
        Assert.IsTrue(g.Equals(g));
        Assert.IsTrue(g==g);
        Assert.IsFalse(g!=g);
    }
    
    [TestMethod]
    public void Formula_TestEqualsDiffFormula_False()
    {
        Formula f = new Formula("12+A3/10*E11");
        Formula g = new Formula("12+A3/10*E7");
        
        Assert.IsFalse(f.Equals(g));
        Assert.IsFalse(f==g);
        Assert.IsTrue(f!=g);
    }
    
    [TestMethod]
    public void Formula_TestEqualsFormulaToNull_False()
    {
        Formula f = new Formula("12+A3/10*E11");
        
        Assert.IsFalse(f.Equals(null));
        Assert.IsFalse(f==null);
        Assert.IsTrue(f!=null);
    }
    
    [TestMethod]
    public void Formula_TestEqualsFormulaToDifferentObject_False()
    {
        Formula f = new Formula("12+A3/10*E11");
        string str = "12+A3/10*E11";
        Assert.IsFalse(f.Equals(str));
    }
    
    [TestMethod]
    public void Formula_TestEqualsMethod_Symmetric()
    {
        Formula f = new Formula("A1+B1");
        Formula g = new Formula("a1+b1");
        
        Assert.IsTrue(f.Equals(g));
        Assert.IsTrue(f==g);
        Assert.IsFalse(f!=g);
        
        Assert.IsTrue(g.Equals(f));
        Assert.IsTrue(g==f);
        Assert.IsFalse(g!=f);
    }
    
    [TestMethod]
    public void EqualsMethod_Transitive()
    {
        Formula f = new Formula("   A1 + 1.0e0 ");
        Formula g = new Formula("A1+1");
        Formula h = new Formula("a1+1E0");
        
        Assert.IsTrue(f.Equals(g));
        Assert.IsTrue(f==g);
        Assert.IsFalse(f!=g);
        
        Assert.IsTrue(g.Equals(h));
        Assert.IsTrue(g==h);
        Assert.IsFalse(g!=h);
        
        Assert.IsTrue(f.Equals(h));
        Assert.IsTrue(f==h);
        Assert.IsFalse(f!=h);
    }
    
    
    [TestMethod]
    public void TestEqualsMethod_DifferentOrder_NotEqual()
    {
        Formula f = new Formula("A1+B1");
        Formula g = new Formula("B1+A1");
        Assert.IsFalse(f.Equals(g));
        Assert.IsFalse(f == g);
        Assert.IsTrue(f != g);
    }
    
    [TestMethod]
    public void EqualityOperator_NonNullEqualsNull_False()
    {
        Formula f = new Formula("1+1");
        Assert.IsFalse(f == null);
        Assert.IsTrue(f != null);
    }

    
    // --- Tests for Evaluate method
    
    //create a spreadsheet of a few cell values
    private static double Spreadsheet(string t) {
        switch (t)
        {
            case "A1":
            case "a1":
                return 2;
            case "B1":
            case "b1":
                return 3;
            case "C1":
            case "c1":
                return 4;
            case "D1":
            case "d1":
                return 8;
        }
        throw new Exception("variable DNE");
    }
    
    //create a second spreadsheet of a few cell values
    private static double Spreadsheet2(string t) =>
        t.ToUpper() switch
        {
            "A1" => 2,
            "B1" => 3,
            "C1" => 4,
            "D1" => 8,
            "A2" => 0,
            "B2" => 6,
            "C2" => 12,
            _    => throw new ArgumentException($"variable {t} DNE")
        };

    
    [TestMethod]
    public void EvaluateMethod_TestSimpleAddition_Valid()
    {
        Formula f = new Formula("2+2.0");
        Assert.AreEqual(4.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestSimpleSubtraction_Valid()
    {
        Formula f = new Formula("2e2 - 1e2");
        Assert.AreEqual(100.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestSimpleMultiplication_Valid()
    {
        Formula f = new Formula("  2*    5");
        Assert.AreEqual(10.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestSimpleDivision_Valid()
    {
        Formula f = new Formula("9/3");
        Assert.AreEqual(3.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestSimpleDivisionIrrational_Valid()
    {
        Formula f = new Formula("3/9");
        Assert.AreEqual(3/9.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestZeroDividedByNumber_Valid()
    {
        Formula f = new Formula("0/11");
        Assert.AreEqual(0.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestNumberDividedByZero_Invalid()
    {
        Formula f = new Formula("11/0");
        Assert.IsInstanceOfType(f.Evaluate(_ => 0), typeof(FormulaError));
    }
    
    [TestMethod]
    public void EvaluateMethod_DivisionByZeroNotShortCircuited_Invalid()
    {
        Formula f = new Formula("1/0+1/0");
        Assert.IsInstanceOfType(f.Evaluate(_ => 0), typeof(FormulaError));
    }

    
    [TestMethod]
    public void EvaluateMethod_TestSimpleParenthesesExpression_Valid()
    {
        Formula f = new Formula("( 10 - 12 + 4)  ");
        Assert.AreEqual(2.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestParenthesesExpressionPEMDAS_Valid()
    {
        Formula f = new Formula("( 10 / 5 - 12 + 2 - 4  * 2 / 4)  ");
        Assert.AreEqual(-10.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestMultipleParentheses_Valid()
    {
        Formula f = new Formula("(10+2)*4/(8-2)");
        Assert.AreEqual(8.0, f.Evaluate(_ => 0));
    }

    [TestMethod]
    public void EvaluateMethod_TestEvaluateSumOfProducts_Valid()
    {
        Formula f = new Formula("(10*2)-(10*2)+(10*2)");
        Assert.AreEqual(20.0, f.Evaluate(_ => 0));
    }

    [TestMethod]
    public void EvaluateMethod_TestEvaluateSumOfQuotients_Valid()
    {
        Formula f = new Formula("(10/2)-(10/2)+(10/2)");
        Assert.AreEqual(5.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestEvaluateProductOfSums_Valid()
    {
        Formula f = new Formula("(4+6)*(11-3)/(15+5)");
        Assert.AreEqual(4.0, f.Evaluate(_ => 0));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestEvaluateSimpleVariableStatement_Valid()
    {
        Formula f = new Formula("A1+11");
        Assert.AreEqual(15.0, f.Evaluate(_ => 4));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestEvaluateVariableStatement_Valid()
    {
        Formula f = new Formula("((A1+11)/3)*A1");
        Assert.AreEqual(20.0, f.Evaluate(_ => 4));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestEvaluateVariableDivideByZero_Invalid()
    {
        Formula f = new Formula("1/A3");
        Assert.IsInstanceOfType(f.Evaluate(_ => 0), typeof(FormulaError));
    }
    
    [TestMethod]
    public void EvaluateMethod_DivisionByVariable_Valid()
    {
        Formula f = new Formula("10/X1");
        Assert.AreEqual(5.0, f.Evaluate(_ => 2));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestEvaluateMultipleVariables_Valid()
    {
        Formula f = new Formula("(A1+b1)*2");
        Assert.AreEqual(10.0, f.Evaluate(Spreadsheet));
    }
    
    [TestMethod]
    public void EvaluateMethod_DivisionByZeroExpression_Invalid()
    {
        Formula f = new Formula("6/(B2-1)");
        Assert.IsInstanceOfType(f.Evaluate(_ => 1), typeof(FormulaError));
    }
    
    [TestMethod]
    public void EvaluateMethod_TestEvaluateUndefinedVariable_Invalid()
    {
        Formula f = new Formula("X1+5");
        Assert.IsInstanceOfType(f.Evaluate(_ => throw new ArgumentException("variableDNE")),
            typeof(FormulaError));
    }
    
        [TestMethod]
        public void EvaluateMethod_VariableNormalisedBeforeLookup()
        {
            Formula f = new Formula("a1+b1");
            Assert.AreEqual(5.0, f.Evaluate(Spreadsheet2));
        }
    
        [TestMethod]
        public void EvaluateMethod_SameVariableUsedTwice()
        {
            Formula f = new Formula("2*A1+A1");
            Assert.AreEqual(6.0, f.Evaluate(Spreadsheet2));
        }
    
        [TestMethod]
        public void EvaluateMethod_AllFourOperatorsWithVariables()
        {
            Formula f = new Formula("A1+B1*C1-D1/A1");
            Assert.AreEqual(10.0, f.Evaluate(Spreadsheet2));
        }
    
        [TestMethod]
        public void EvaluateMethod_ComplexExpressionWithVariables()
        {
            Formula f = new Formula("(A1+B1)*(C1-A1)");
            Assert.AreEqual(10.0, f.Evaluate(Spreadsheet2));
        }
    
        [TestMethod]
        public void EvaluateMethod_VariableInDenominator_Valid()
        {
            Formula f = new Formula("D1/C1");
            Assert.AreEqual(2.0, f.Evaluate(Spreadsheet2));
        }
    
    [TestMethod]
    public void EvaluateMethod_MultiplicationBeforeAddition_Valid()
    {
        Formula f = new Formula("2+3*4");
        Assert.AreEqual(14.0, f.Evaluate(_ => 0));
    }

    [TestMethod]
    public void EvaluateMethod_DivisionBeforeSubtraction_Valid()
    {
        Formula f = new Formula("20-8/4");
        Assert.AreEqual(18.0, f.Evaluate(_ => 0));
    }

    [TestMethod]
    public void EvaluateMethod_ParenthesesExemptPEMDAS_Valid()
    {
        Formula f = new Formula("1+2*3-4/2");
        Assert.AreEqual(5.0, f.Evaluate(_ => 0));
    }

    [TestMethod]
    public void EvaluateMethod_ParenthesesPrecedence_Valid()
    {
        Formula f = new Formula("(2+3)*4");
        Assert.AreEqual(20.0, f.Evaluate(_ => 0));
    }

    [TestMethod]
    public void EvaluateMethod_DeeplyNestedParentheses_Valid()
    {
        Formula f = new Formula("(((2+3))*((2*2)))");
        Assert.AreEqual(20.0, f.Evaluate(_ => 0));
    }

    [TestMethod]
    public void EvaluateMethod_ScientificNotation_Valid()
    {
        Formula f = new Formula("1e2+2e1");
        Assert.AreEqual(120.0, f.Evaluate(_ => 0));
    }

    // --- Tests for HashCode method

    [TestMethod]
    public void GetHashCodeMethod_TestHashCodeSimpleEquivalent_Valid()
    {
        Formula f = new Formula("(1+2+3)");
        Formula g = new Formula("(1+2+3)");
        Assert.IsTrue(f.GetHashCode() == g.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeMethod_TestHashCodeVariablesEquivalent_Valid()
    {
        Formula f = new Formula("A1");
        Formula g = new Formula("a1");
        Assert.IsTrue(f.GetHashCode() == g.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeMethod_TestHashCodeEquivalentDiffFormat_Valid()
    {
        Formula f = new Formula("( 1.0 + 2e0 + 3E0 ) ");
        Formula g = new Formula("(1+2+3)");
        Assert.IsTrue(f.GetHashCode() == g.GetHashCode());
    }


    [TestMethod]
    public void GetHashCodeMethod_TestHashCodeVariablesNotEquivalent_Valid()
    {
        Formula f = new Formula("A1");
        Formula g = new Formula("B1");
        Assert.IsFalse(f.GetHashCode() == g.GetHashCode());
    }

    [TestMethod]
    public void GetHashCodeMethod_TestHashCodeComplexEquivalent_Valid()
    {
        Formula f = new Formula("a1 * 11e2 / (b1+b2) - C13 * (2.11-B2) + c11");
        Formula g = new Formula("A1*1100/(B1+B2)-C13*(2.11-B2)+C11");
        Assert.IsTrue(f.GetHashCode() == g.GetHashCode());
    }
    
    [TestMethod]
    public void GetHashCode_StableAcrossMultipleCalls()
    {
        Formula f = new Formula("A1*B1+C1");
        int first  = f.GetHashCode();
        int second = f.GetHashCode();
        int third  = f.GetHashCode();
        Assert.AreEqual(first, second);
        Assert.AreEqual(second, third);
    }
    
    [TestMethod]
    public void GetHashCodeMethod_TestHashCodeComplexNotEquivalent_Valid()
    {
        //original formula f
        Formula f = new Formula("a1 * 11e2 / (b1+b2) - C13 * (2.11-B2) + c11");
        //equivalent form which will be changed for easier readability
        Formula fEquivalent = new Formula("A1*1100/(B1+B2)-C13*(2.11-B2)+C11");
        
        Assert.IsTrue(f.GetHashCode() == fEquivalent.GetHashCode());
        
        Formula g = new Formula("A1*1100/(B1+B2)-C13*(2.11-B2)-C11"); // change + to -
        Assert.IsFalse(f.GetHashCode() == g.GetHashCode());

        Formula h = new Formula("A1*1100/(B1+B2)-C13*(2.11-B2)+C12"); // change c11 to c12
        Assert.IsFalse(f.GetHashCode() == h.GetHashCode());

        Formula i = new Formula("A1*1101/(B1+B2)-C13*(2.11-B2)+C11"); // change 1100 to 1101
        Assert.IsFalse(f.GetHashCode() == i.GetHashCode(), "f " + f.GetHashCode() + " i " + i.GetHashCode());
    }

}