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
// <authors> Pridhvi Subgani </authors>
// <date> September 10, 2026 </date>
// </summary>

using System.Collections;
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

    //holds the standardized form of the infix expression
    private string StandardizedForm;

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
        if (!validFollowingTokenRules567(tokens[0], "("))
            throw new FormulaFormatException("first token must be number, variable, or opening parenthesis");

        //RULE 6 - last token must be number, variable, or closing parenthesis
        if (!validFollowingTokenRules567(tokens[tokens.Count - 1], ")"))
            throw new FormulaFormatException("last token must be number, variable, or closing parenthesis");

        int openParenthesisCount = 0;
        int closingParenthesisCount = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            if (double.TryParse(tokens[i], out double x) == true ||
                IsVar(tokens[i])) // check if token is a number or variable
            {
                if (i + 1 < tokens.Count)
                {
                    if (validFollowingTokenRule8(tokens[i + 1]) == false)
                    {
                        throw new FormulaFormatException(
                            "following an operator must be a number, variable, or open parenthesis");
                    }
                }

                continue;
            }

            switch (tokens[i])
            {
                case "(":
                    openParenthesisCount++;
                    // RULE 7a - parenthesis following rule  
                    if (i + 1 >= tokens.Count ||
                        validFollowingTokenRules567(tokens[i + 1], "(") ==
                        false) // check that the token after "(" is out of range or is invalid
                    {
                        throw new FormulaFormatException(
                            "following an open parenthesis must be a number, variable, or open parenthesis");
                    }

                    break;
                case ")":
                    closingParenthesisCount++;
                    if (i + 1 < tokens.Count) // check if a token after ")" exists
                    {
                        if (validFollowingTokenRule8(tokens[i + 1]) == false) // check that this token is invalid
                            throw new FormulaFormatException(
                                "following a closed parenthesis must be an operator or closing parenthesis");
                    }

                    break;
                case "+":
                case "-":
                case "/":
                case "*":
                    // RULE 7b - operator following rule
                    if (i + 1 >= tokens.Count ||
                        validFollowingTokenRules567(tokens[i + 1], "(") ==
                        false) // check that token after operator is out of range or is invalid
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

        //create the expected canonical form of the formula parameter
        StandardizedForm = canonize(formula);
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
    private static bool IsVar(string token)
    {
        // notice the use of ^ and $ to denote that the entire string being matched is just the variable
        string standaloneVarPattern = $"^{VariableRegExPattern}$";
        return Regex.IsMatch(token, standaloneVarPattern);
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
    private static List<string> GetTokens(string formula)
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
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                results.Add(s);
            }
        }

        return results;
    }

    /// <summary>
    /// Checks if the following token is valid for Rules 5, 6, and 7:
    /// The first token of an expression must be a number, a variable, or an opening parenthesis.
    /// The last token of an expression must be a number, a variable, or a closing parenthesis.
    /// Any token that immediately follows an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis.
    /// </summary>
    /// <param name="token">token to be validated</param>
    /// <param name="paren">opening or closing parenthesis to be expected</param>
    /// <returns>true if following token is valid, false if following token is invalid (i.e. certain token cannot follow an open parenthesis)</returns>
    private static bool validFollowingTokenRules567(string token, string paren)
    {
        // if token is not a variable or a number or parenthesis, return false
        if (!IsVar(token) && double.TryParse(token, out double x) == false && token != paren)
            return false;
        return true;
    }

    /// <summary>
    /// Checks if the following token is valid for Rule 8:
    /// Any token that immediately follows a number, a variable,
    /// or a closing parenthesis must be either an operator or a closing parenthesis.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static bool validFollowingTokenRule8(string token)
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

    /// <summary>
    /// Helper method to canonize the given infix expression
    /// </summary>
    /// <param name="input">string input to be canonizedd</param>
    /// <returns>returns the canonized form of the input string</returns>
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

    /// <summary>
    ///   <para>
    ///     Reports whether f1 == f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are the same.</returns>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);
    }

    /// <summary>
    ///   <para>
    ///     Reports whether f1 != f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are not equal to each other.</returns>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !(f1.Equals(f2));
    }

    /// <summary>
    ///   <para>
    ///     Determines if two formula objects represent the same formula.
    ///   </para>
    ///   <para>
    ///     By definition, if the parameter is null or does not reference
    ///     a Formula Object then return false.
    ///   </para>
    ///   <para>
    ///     Two Formulas are considered equal if their canonical string representations
    ///     (as defined by ToString) are equal.
    ///   </para>
    /// </summary>
    /// <param name="obj"> The other object.</param>
    /// <returns>
    ///   True if the two objects represent the same formula.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != typeof(Formula))
            return false;
        return this.ToString().Equals(obj.ToString());
    }

    /// <summary>
    ///   <para>
    ///     Evaluates this Formula, using the lookup delegate to determine the values of
    ///     variables.
    ///   </para>
    ///   <remarks>
    ///     When the lookup method is called, it will always be passed a normalized (capitalized)
    ///     variable name.  The lookup method will throw an ArgumentException if there is
    ///     not a definition for that variable token.
    ///   </remarks>
    ///   <para>
    ///     If no undefined variables or divisions by zero are encountered when evaluating
    ///     this Formula, the numeric value of the formula is returned.  Otherwise, a
    ///     FormulaError is returned (with a meaningful explanation as the Reason property).
    ///   </para>
    ///   <para>
    ///     This method should never throw an exception.
    ///   </para>
    /// </summary>
    /// <param name="lookup">
    ///   <para>
    ///     Given a variable symbol as its parameter, lookup returns the variable's value
    ///     (if it has one) or throws an ArgumentException (otherwise).  This method will expect
    ///     variable names to be normalized.
    ///   </para>
    /// </param>
    /// <returns> Either a double or a FormulaError, based on evaluating the formula.</returns>
    public object Evaluate(Lookup lookup)
    {
        Stack<double> values = new Stack<double>();
        Stack<string> operators = new Stack<string>();

        List<string> tokens = GetTokens(StandardizedForm);

        for (int i = 0; i < tokens.Count; i++)
        {
            string token = tokens[i];
            if (double.TryParse(token, out double x))
            {
                if (operators.Count != 0 && operators.Peek() == "*")
                {
                    double num = values.Pop();
                    operators.Pop();
                    values.Push(x * num);
                }

                else if (operators.Count != 0 && operators.Peek() == "/")
                {
                    double num = values.Pop();
                    if (x == 0)
                        return new FormulaError("divide by 0 error");
                    operators.Pop();
                    values.Push(num / x);
                }
                else
                {
                    values.Push(double.Parse(token));
                }
            }

            if (IsVar(token))
            {
                double val;
                try
                {
                    val = lookup(token);
                }
                catch (ArgumentException)
                {
                    return new FormulaError("variable " + token + " is undefined");
                }

                if (operators.Count != 0 && operators.Peek() == "*")
                {
                    double num = values.Pop();
                    operators.Pop();
                    values.Push(val*num);
                }

                else if (operators.Count != 0 && operators.Peek() == "/")
                {
                    double num = values.Pop();
                    if (val == 0)
                        return new FormulaError("divide by 0 error");
                    operators.Pop();
                    values.Push(num/val);
                }
                else
                {
                    values.Push(val);
                }
            }

            if (token == "+" || token == "-")
            {
                if (operators.Count != 0 && (operators.Peek() == "+" || operators.Peek() == "-"))
                {
                    double a = values.Pop();
                    double b = values.Pop();

                    string operation = operators.Pop();

                    switch (operation)
                    {
                        case "+":
                            values.Push(a + b);
                            break;
                        case "-":
                            values.Push(b - a);
                            break;
                    }
                }
                operators.Push(token);
            }

            if (token == "*" || token == "/" || token == "(")
            {
                operators.Push(token);
            }

            if (token == ")")
            {
                if (operators.Count != 0 && (operators.Peek() == "+" || operators.Peek() == "-"))
                {
                    double a = values.Pop();
                    double b = values.Pop();

                    string operation = operators.Pop();

                    switch (operation)
                    {
                        case "+":
                            values.Push(a + b);
                            break;
                        case "-":
                            values.Push(b - a);
                            break;
                    }
                }

                operators.Pop();

                if (operators.Count != 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                {
                    double a = values.Pop();
                    double b = values.Pop();

                    string operation = operators.Pop();

                    switch (operation)
                    {

                        case "*":
                            values.Push(a * b);
                            break;
                        case "/":
                            if (a == 0)
                                return new FormulaError("divide by 0 error");
                            values.Push(b / a);
                            break;
                    }
                }
            }

            if (tokens.Count - i == 1)
            {
                if (operators.Count == 0)
                    return values.Pop();
                
                double a = values.Pop();
                    double b = values.Pop();

                    string operation = operators.Pop();

                    switch (operation)
                    {
                        case "+":
                            values.Push(a + b);
                            return values.Pop();
                        case "-":
                            values.Push(b - a);
                            return values.Pop();
                }
            }
        }
        // unreachable, but method expects some return value
        return new FormulaError("Evaluate method has broken");
    }

    /// <summary>
    ///   <para>
    ///     Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    ///     case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    ///     randomly-generated unequal Formulas have the same hash code should be miniscule.
    ///   </para>
    /// </summary>
    /// <returns> The hashcode for the object. </returns>
    public override int GetHashCode()
    {
        List<string> tokens = GetTokens(StandardizedForm);
        
        int sum = 0;
        int variables = 0;
        int characters = 0;
        int numbers = 0;
        
        for (int i = 0; i < tokens.Count; i++)
        {

            // if the token is a number, add it to the sum
            // add to the number counter
            if (double.TryParse(tokens[i], out double x))
            {
                sum += (int)double.Parse(tokens[i]);
                numbers++;
            }
            
            // if the token is not a number or variable, it is some ASCII character. add this character's value to the sum.
            // add to character counter
            else if (!IsVar(tokens[i]))
            {
                sum += tokens[i][0];
                characters++;
            }
            // otherwise the token is a variable, collect each individual character's ASCII code and add it to the sum
            // add to variable counter
            else
            {
                sum += VariableToNumber(tokens[i]);
                variables++;
            }
        }
        // the additional variable and character counters can be used to help avoid collisions (i.e. if sums happen to be equal)
        return HashCode.Combine(sum, variables, characters, numbers);
    }
    
    /// <summary>
    /// Converts a variable (consisting of x characters + y digits) into a number by summing the ASCII codes of
    /// each character and digit. This is for the purpose of creating a HashCode.
    /// </summary>
    /// <param name="variable">Variable to be converted</param>
    /// <returns></returns>
    private int VariableToNumber(string variable)
    {
        int sum = 0;
        foreach (char c in variable)
        {
            sum += c;
        }

        return sum;
    }

}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public class FormulaError
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaError"/> class.
    ///   <para>
    ///     Constructs a FormulaError containing the explanatory reason.
    ///   </para>
    /// </summary>
    /// <param name="message"> Contains a message for why the error occurred.</param>
    public FormulaError( string message )
    {
        Reason = message;
    }

    /// <summary>
    ///  Gets the reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

/// <summary>
///   Any method meeting this type signature can be used for
///   looking up the value of a variable.
/// </summary>
/// <exception cref="ArgumentException">
///   If a variable name is provided that is not recognized by the implementing method,
///   then the method should throw an ArgumentException.
/// </exception>
/// <param name="variableName">
///   The name of the variable (e.g., "A1") to lookup.
/// </param>
/// <returns> The value of the given variable (if one exists). </returns>
public delegate double Lookup( string variableName );


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
    public FormulaFormatException(string message)
        : base(message)
    {
        // All this does is call the base constructor. No extra code needed.
    }
}