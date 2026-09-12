// <summary>
//   <para>
//     This code is provided to start your assignment.  It was written
//     by Profs Joe, Danny, Jim, and Travis.  You should keep this attribution
//     at the top of your code where you have your header comment, along
//     with any other required information.
//   </para>
//   <para>
//     You should remove/add/adjust comments in your file as appropriate
//     to represent your work and any changes you make.
//   </para>
// </summary>

using System.Reflection;

namespace Formula;

using System.Text.RegularExpressions;

/// <summary>
///   <para>
///     This class represents formulas written in standard infix notation using standard precedence
///     rules.  The allowed symbols are non-negative numbers written using double-precision
///     floating-point syntax; variables that consist of one or more letters followed by
///     one or more numbers; parentheses; and the four operator symbols +, -, *, and /.
///   </para>
///   <para>
///     Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
///     a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
///     and "x 23" consists of a variable "x" and a number "23".  Otherwise, spaces are to be removed.
///   </para>
///   <para>
///     For Assignment Two, you are to implement the following functionality:
///   </para>
///   <list type="bullet">
///     <item>
///        Formula Constructor which checks the syntax of a formula.
///     </item>
///     <item>
///        Get Variables
///     </item>
///     <item>
///        ToString
///     </item>
///   </list>
/// </summary>
public class Formula
{
    /// <summary>
    ///   All variables are letters followed by numbers.  This pattern
    ///   represents valid variable name strings.
    /// </summary>
    private const string VariableRegExPattern = @"[a-zA-Z]+\d+";
    private string StandardizedForm;
    /// <summary>
    ///   Initializes a new instance of the <see cref="Formula"/> class.
    ///   <para>
    ///     Creates a Formula from a string that consists of an infix expression written as
    ///     described in the class comment.  If the expression is syntactically incorrect,
    ///     throws a FormulaFormatException with an explanatory Message.  See the assignment
    ///     specifications for the syntax rules you are to implement.
    ///   </para>
    ///   <para>
    ///     Non-Exhaustive Example Errors:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///        Invalid variable name, e.g., x, x1x  (Note: x1 is valid, but would be normalized to X1)
    ///     </item>
    ///     <item>
    ///        Empty formula, e.g., string.Empty
    ///     </item>
    ///     <item>
    ///        Mismatched Parentheses, e.g., "(("
    ///     </item>
    ///     <item>
    ///        Invalid Following Rule, e.g., "2x+5"
    ///     </item>
    ///   </list>
    /// </summary>
    /// <param name="formula"> The string representation of the formula to be created.</param>
    public Formula(string formula)
    {
        List<string> tokens = GetTokens(formula);
        
        //RULE 1 - there must be at least one token
        if (tokens.Count == 0)
        {
            throw new FormulaFormatException("formula must have at least one token");
        }
        //RULE 5 - first token must be number, variable, or opening parenthesis
        if (!validFollowingToken(tokens[0], "("))
            throw new FormulaFormatException("first token must be number, variable, or opening parenthesis");
        
        //RULE 6 - last token must be number, variable, or closing parenthesis
        if (!validFollowingToken(tokens[tokens.Count - 1], ")"))
            throw new FormulaFormatException("last token must be number, variable, or closing parenthesis");

        int openParenthesisCount = 0;
        int closingParenthesisCount = 0;
        
        for (int i = 0; i < tokens.Count; i++)
        {
            if (double.TryParse(tokens[i], out double x) == true || IsVar(tokens[i])) //if token is a number or variable
            {
                if (i + 1 < tokens.Count)
                {
                    if (validFollowingToken2(tokens[i + 1]) == false)
                    {
                        throw new FormulaFormatException("following an operator must be a number, variable, or open parenthesis");
                    }
                }  
                continue;
            }

            switch (tokens[i])
            {
                case "(":
                    openParenthesisCount++;
                    // RULE 7a - parenthesis following rule  
                    if (i + 1 >= tokens.Count || validFollowingToken(tokens[i + 1], "(") == false) // check that token after ( is out of range or is invalid
                {
                    throw new FormulaFormatException(
                        "following an open parenthesis must be a number, variable, or open parenthesis");
                }
                    break;
                case ")":
                    closingParenthesisCount++;
                    if (i + 1 < tokens.Count) // check that a token after )
                    {
                        if (validFollowingToken2(tokens[i + 1]) == false) // check that this token is invalid
                        throw new FormulaFormatException(
                            "following a closed parenthesis must be an operator or closing parenthesis");
                    }
                    break;
                case "+": case "-": case "/": case "*":
                    // RULE 7b - operator following rule
                    if (i + 1 >= tokens.Count || validFollowingToken(tokens[i + 1], "(") == false) // check that token after operator is out of range or is invalid
                    {
                        throw new FormulaFormatException(
                            "following an operator must be a number, variable, or open parenthesis");
                    }
                    break;
            }
            
            //RULE 3 - # of closing parentheses cannot be greater than the # of opening parentheses seen so far
            if (closingParenthesisCount > openParenthesisCount)
            {
                throw new FormulaFormatException("closing parenthesis found before opening parenthesis");
            }

        }
        
        //RULE 4 - balanced parentheses
        if (openParenthesisCount != closingParenthesisCount)
        {
            throw new FormulaFormatException("parentheses count mismatch");
        }

        StandardizedForm = canonize(formula);

    }

    /// <summary>
    ///   <para>
    ///     Returns a set of all the variables in the formula.
    ///   </para>
    ///   <remarks>
    ///     Important: no variable may appear more than once in the returned set, even
    ///     if it is used more than once in the Formula.
	///     Variables should be returned in canonical form, having all letters converted
	///     to uppercase.
    ///   </remarks>
    ///   <list type="bullet">
    ///     <item>new("x1+y1*z1").GetVariables() should return a set containing "X1", "Y1", and "Z1".</item>
    ///     <item>new("x1+X1"   ).GetVariables() should return a set containing "X1".</item>
    ///   </list>
    /// </summary>
    /// <returns> the set of variables (string names) representing the variables referenced by the formula. </returns>
    public ISet<string> GetVariables()
    {
        HashSet<string> variables = new HashSet<string>();
        List<string> tokens = GetTokens(StandardizedForm);
        
        foreach (string token in tokens)
        {
            if (IsVar(token))
            {
                variables.Add(token);
            }

        }
        
        return variables;
    }

    /// <summary>
    ///   <para>
    ///     Returns a string representation of a canonical form of the formula.
    ///   </para>
    ///   <para>
    ///     The string will contain no spaces.
    ///   </para>
    ///   <para>
    ///     If the string is passed to the Formula constructor, the new Formula f
    ///     will be such that this.ToString() == f.ToString().
    ///   </para>
    ///   <para>
    ///     All the variable and number tokens in the string will be normalized.
    ///     For numbers, this means that the original string token is converted to
    ///     a number using double.Parse or double.TryParse, then converted back to a
    ///     string using double.ToString.
    ///     For variables, this means all letters are uppercased.
    ///   </para>
    ///   <para>
    ///       For example:
    ///   </para>
    ///   <code>
    ///       new("x1 + Y1").ToString() should return "X1+Y1"
    ///       new("x1 + 5.0000").ToString() should return "X1+5".
    ///   </code>
    ///   <para>
    ///     This method should execute in O(1) time.
    ///   </para>
    /// </summary>
    /// <returns>
    ///   A canonical version (string) of the formula. All "equal" formulas
    ///   should have the same value here.
    /// </returns>
    public override string ToString()
    {
        return string.Concat(GetTokens(StandardizedForm));
    }

    /// <summary>
    ///   Reports whether "token" is a variable.  It must be one or more letters
    ///   followed by one or more numbers.
    /// </summary>
    /// <param name="token"> A token that may be a variable. </param>
    /// <returns> true if the string matches the requirements, e.g., A1 or a1. </returns>
    private static bool IsVar( string token )
    {
        // notice the use of ^ and $ to denote that the entire string being matched is just the variable
        string standaloneVarPattern = $"^{VariableRegExPattern}$";
        return Regex.IsMatch( token, standaloneVarPattern );
    }

    /// <summary>
    ///   <para>
    ///     Given an expression, enumerates the tokens that compose it.
    ///   </para>
    ///   <para>
    ///     Tokens returned are:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>left paren</item>
    ///     <item>right paren</item>
    ///     <item>one of the four operator symbols</item>
    ///     <item>a string consisting of one or more letters followed by one or more numbers</item>
    ///     <item>a double literal</item>
    ///     <item>and anything that doesn't match one of the above patterns</item>
    ///   </list>
    ///   <para>
    ///     There are no empty tokens; white space is ignored (except to separate other tokens).
    ///   </para>
    /// </summary>
    /// <param name="formula"> A string representing an infix formula such as 1*B1/3.0. </param>
    /// <returns> The ordered list of tokens in the formula. </returns>
    private static List<string> GetTokens( string formula )
    {
        List<string> results = [];

        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format(
                                        "({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern,
                                        rpPattern,
                                        opPattern,
                                        VariableRegExPattern,
                                        doublePattern,
                                        spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach ( string s in Regex.Split( formula, pattern, RegexOptions.IgnorePatternWhitespace ) )
        {
            if ( !Regex.IsMatch( s, @"^\s*$", RegexOptions.Singleline ) )
            {
                results.Add(s);
            }
        }

        return results;
    }

    /// <summary>
    /// Checks if the token is valid for Rules 5, 6, and 7 (i.e. a number, variable, or open parenthesis/closing parenthesis depending on arguments)
    /// </summary>
    /// <param name="token">token to be validated</param>
    /// <param name="paren">opening or closing parenthesis to be expected from the token</param>
    /// <returns>true if token is valid, false if token is invalid (i.e. token cannot follow an open parenthesis)</returns>
    private static bool validFollowingToken(string token, string paren)
    {
        // if token is not a variable or a number or parenthesis, return false
        if (!IsVar(token) && double.TryParse(token, out double x) == false && token != paren)
            return false;
        return true;
    }

    private static bool validFollowingToken2(string token)
    {
        //Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis.
        string[] validTokens = { ")", "+", "-", "*", "/" };
        foreach (string validToken in validTokens)
        {
            if (token == validToken)
                return true;
        }
        return false;
    }
    
    //returns a string of the canonized form of the formula/input
    private String canonize(string input)
    { 
        List<string> tokens = GetTokens(input);
        string ret = string.Empty;
        
        foreach (string token in tokens)
        {
            if (double.TryParse(token, out double x) == true)
            {
                ret += double.Parse(token).ToString();
                continue;
            }

            if (IsVar(token))
            {
                ret += token.ToUpper();
                continue;
            }

            ret += token;

        }
        return ret;
    }

    
}


    /// <summary>
///   Used to report syntax errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaFormatException"/> class.
    ///   <para>
    ///      Constructs a FormulaFormatException containing the explanatory message.
    ///   </para>
    /// </summary>
    /// <param name="message"> A developer defined message describing why the exception occured.</param>
    public FormulaFormatException( string message )
        : base( message )
    {
        // All this does is call the base constructor. No extra code needed.
    }
}
