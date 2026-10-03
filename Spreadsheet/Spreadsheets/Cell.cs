namespace Spreadsheets;

using Formula;
using DependencyGraph;


/// <summary>
///     <para>
///         Represents the cell
///     </para>
/// </summary>
public class Cell<T>
{
    private string location; //location
    private T content;
    private T value;

    /// <summary>
    /// Constructor for a cell which contains a formula
    /// </summary>
    /// <param name="content"> content in the cell - i.e. string, double, formula</param>
    public Cell(string location, T content)
    {
        this.location =  location.ToUpper();
        this.content = content;

        if (content.GetType() == typeof(double) || content.GetType() == typeof(string))
            this.value = content;

    }
    
    // allows access of location through "name"
    public string name
    {
        get
        {
            return location;
        }
    }


    // do i need a constructor for an empty cell
}