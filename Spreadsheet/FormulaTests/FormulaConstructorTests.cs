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
        Assert.AreEqual(true,  testHash.Contains("E2"));
    }
    
}
