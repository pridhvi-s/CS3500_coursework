// <copyright file="SpreadsheetPage.razor.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text;
using Formula;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Spreadsheets;

namespace GUI.Components.Pages;

/// <summary>
/// Main view pattern for the spreadsheet.  Displays the spreadsheet grid and its cells for the user to interact with. 
/// </summary>
public partial class SpreadsheetPage
{

    /// <summary>
    /// Based on your computer, you could shrink/grow this value based on performance.
    /// </summary>
    private const int Rows = 50;

    /// <summary>
    /// Number of columns, which will be labeled A-Z.
    /// </summary>
    private const int Cols = 26;

    /// <summary>
    /// Provides an easy way to convert from an index to a letter (0 -> A)
    /// </summary>
    private char[] Alphabet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    /// <summary>
    ///   Gets or sets the name of the file to be saved
    /// </summary>
    private string FileSaveName { get; set; } = "Spreadsheet.sprd";

    /// <summary>
    ///   The spreadsheet being displayed and edited.
    /// </summary>
    private Spreadsheet spreadsheet = new Spreadsheet();

    /// <summary>
    ///   <para> Gets or sets the data for all the cells in the spreadsheet GUI. </para>
    ///   <remarks>Backing Store for HTML</remarks>
    /// </summary>
    private string[,] CellsDisplayStore { get; set; } = new string[Rows, Cols];

    /// <summary>
    /// Contains the raw contents of the invalid cells.
    /// This is used so that during the editing of an invalid formula,
    /// the user can see what they typed, rather than the error message.
    /// Note: THIS IS NOT SAVED TO THE SPREADSHEET, it is only used for display purposes.
    /// </summary>
    private string?[,] ExceptionContentsStore = new string[Rows, Cols];

    /// <summary>
    ///     The backing field for the currently selected row
    /// </summary>
    private int selectedRow = 0;

    /// <summary>
    /// The row of the currently selected cell.
    /// </summary>
    /// <value>The Cell row</value>
    public int SelectedRow
    {
        get
        {
            return selectedRow;
        }
        set
        {
            if (value < 0 || value >= Rows)
            {
                return;
            }

            if (selectedRow != value)
            {
                // Update the focused text input when the selected row changes
                selectedRow = value;
                FocusedTextInput = GetCellContents(SelectedRow, SelectedCol);
                StateHasChanged();
            }
        }
    }

    /// <summary>
    ///     The backing field for the currently selected column
    /// </summary>
    private int selectedCol = 0;

    /// <summary>
    /// The column of the currently selected cell.
    /// </summary>
    /// <value> The Cell column </value>
    public int SelectedCol {
        get
        {
            return selectedCol;
        }
        set
        {
            if (value < 0 || value >= Cols)
            {
                return;
            }

            if (selectedCol != value)
            {
                // Update the focused text input when the selected column changes
                selectedCol = value;
                FocusedTextInput = GetCellContents(SelectedRow, SelectedCol);
                StateHasChanged();
            }
        }
    }

    private ElementReference textInputRef;

    /// <summary>
    /// 
    /// </summary>
    private string focusedTextInput;

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public string FocusedTextInput
    {
        get
        {
            if (SelectedRow < 0 || SelectedCol < 0)
            {
                return string.Empty;
            }

            var currentCellContents = GetCellContents(SelectedRow, SelectedCol);
            if (currentCellContents == focusedTextInput)
            {
                return currentCellContents.ToString();
            }

            // If the current cell contents have changed, but are
            // different from the focused text input, we want to return the focused text input
            // as the focused text input may be invalid.
            return focusedTextInput;
        }
        set
        {
            if (SelectedRow < 0 || SelectedCol < 0)
            {
                return;
            }

            try
            {
                var updatedCells = spreadsheet.SetContentsOfCell(SelectedCellName, value);
                foreach (string cell in updatedCells)
                {
                    var (row, col) = ParseCellName(cell);
                    var cellValue = spreadsheet.GetCellValue(cell);
                    ExceptionContentsStore[row, col] = null; // Clear the invalid contents store for this cell
                    
                    if (cellValue is FormulaError)
                    {
                        CellsDisplayStore[row, col] = "Error!";
                    }
                    else
                    {
                        CellsDisplayStore[row, col] = cellValue.ToString();
                    }
                }
            }
            catch (FormulaFormatException e)
            {
                // Update the backing store with the new value, even if it is invalid,
                // so that the user can see what they typed.
                ExceptionContentsStore[SelectedRow, SelectedCol] = value;
                CellsDisplayStore[SelectedRow, SelectedCol] = "Formula Err!";
            }
            catch (CircularException e)
            {
                // Update the backing store with the new value, even if it is invalid,
                // so that the user can see what they typed.
                ExceptionContentsStore[SelectedRow, SelectedCol] = value;
                CellsDisplayStore[SelectedRow, SelectedCol] = "Circular!";
            }
            finally
            {
                focusedTextInput = value;
            }

        }
    }


    /// <summary>
    /// Gets the name of the currently selected cell, in the format "A1", "B2", etc.
    /// </summary>
    /// <value>The name of the selected cell</value>
    public string SelectedCellName
    {
        get
        {
            if (SelectedRow < 0 || SelectedCol < 0)
            {
                return string.Empty;
            }

            return GetCellName(SelectedRow, SelectedCol);
        }
    }

    /// <summary>
    /// Handler for when a cell is clicked
    /// </summary>
    /// <param name="row">The row component of the cell's coordinates</param>
    /// <param name="col">The column component of the cell's coordinates</param>
    private void CellClicked( int row, int col )
    {
        SelectedRow = row;
        SelectedCol = col;
        textInputRef.FocusAsync();
    }


    /// <summary>
    /// Gets the string contents of a cell if its a formula it starts
    /// with an equals sign.
    /// </summary>
    /// <param name="row">The row component of the cell's coordinates</param>
    /// <param name="col">The column component of the cell's coordinates</param>
    private string GetCellContents( int row, int col )
    {
        if (ExceptionContentsStore[row, col] != null)
        {
            return ExceptionContentsStore[row, col];
        }

        var cellName = GetCellName(row, col);
        var contents = spreadsheet.GetCellContents(cellName);
        if (contents is Formula.Formula)
            contents = "=" + contents.ToString();
        return contents.ToString();
    }

    /// <summary>
    /// Saves the current spreadsheet, by providing a download of a file
    /// containing the json representation of the spreadsheet.
    /// </summary>
    private async void SaveFile()
    {
        await JsRuntime.InvokeVoidAsync( "downloadFile", FileSaveName, 
            spreadsheet.ToJson());
    }

    /// <summary>
    /// This method will run when the file chooser is used, for loading a file.
    /// Uploads a file containing a json representation of a spreadsheet, and 
    /// replaces the current sheet with the loaded one.
    /// </summary>
    /// <param name="args">The event arguments, which contains the selected file name</param>
    private async void HandleFileChooser( EventArgs args )
    {
        try
        {
            string fileContent = string.Empty;

            InputFileChangeEventArgs eventArgs = args as InputFileChangeEventArgs ?? throw new Exception("unable to get file name");
            if ( eventArgs.FileCount == 1 )
            {
                var file = eventArgs.File;
                if ( file is null )
                {
                    return;
                }

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                // fileContent will contain the contents of the loaded file
                fileContent = await reader.ReadToEndAsync();
                focusedTextInput = string.Empty;
                spreadsheet = new Spreadsheet();
                spreadsheet.LoadFromJson(fileContent);

                // Clear the backing store before loading new values
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        CellsDisplayStore[i, j] = string.Empty;
                        ExceptionContentsStore[i, j] = null;
                    }
                }

                // Load all non empty cell contents from the spreadsheet into the backing store
                foreach (var cells in spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    var (row, col) = ParseCellName(cells);
                    var cellValue = spreadsheet.GetCellValue(cells);

                    if (cellValue is FormulaError)
                    {
                        CellsDisplayStore[row, col] = "Error!";
                    }
                    else
                    {
                        CellsDisplayStore[row, col] = cellValue.ToString();
                    }
                }

                // Select 0, 0 (A1) after loading a new file
                SelectedRow = 0;
                SelectedCol = 0;

                StateHasChanged();
            }
        }
        catch ( Exception e )
        {
            Debug.WriteLine( "an error occurred while loading the file..." + e );
        }
    }

    /// <summary>
    ///   Whether the page is currently showing the dark theme. The theme itself lives in the
    ///   browser (a data-theme attribute on the html element, remembered in localStorage),
    ///   so this only drives the toggle button's label.
    /// </summary>
    private bool IsDarkMode { get; set; } = false;

    /// <summary>
    ///   Once the page is interactive, ask the browser which theme was applied at startup
    ///   (saved choice, or the operating system's preference) so the button label is correct.
    /// </summary>
    /// <param name="firstRender">True only for the first render of this component.</param>
    /// <returns>A task that completes when the theme has been read.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                IsDarkMode = await JsRuntime.InvokeAsync<string>("getTheme") == "dark";
                StateHasChanged();
            }
            catch (Exception e) when (e is JSException || e is InvalidOperationException)
            {
                Debug.WriteLine("unable to read the current theme..." + e);
            }
        }
    }

    /// <summary>
    ///   Switches between the light and dark themes and remembers the choice in the browser.
    /// </summary>
    /// <returns>A task that completes when the browser has applied the new theme.</returns>
    private async Task ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        await JsRuntime.InvokeVoidAsync("setTheme", IsDarkMode ? "dark" : "light");
    }

    private static string GetCellName(int row, int col)
    {
        return ToColumnName(col + 1) + (row + 1).ToString();
    }

    private static (int row, int col) ParseCellName(string name)
    {
        // Extract the column letters and row numbers from the cell name
        int i = 0;
        while (i < name.Length && char.IsLetter(name[i]))
        {
            i++;
        }
        var colName = name.Substring(0, i);
        var rowName = name.Substring(i);
        return (int.Parse(rowName) - 1, FromColumnName(colName.ToUpper()) - 1);
    }

    private static int FromColumnName(string s)
    {
        int col = 0;
        for (int i = 0; i < s.Length; i++)
        {
            col *= 26;
            col += (s[i] - 'A' + 1);
        }
        return col; // row is not determined here
    }

    private static string ToColumnName(int n)   // 1-based: 1=A, 26=Z, 27=AA
    {
        var sb = new StringBuilder();
        while (n > 0)
        {
            n--;                                  // shift to 0-based each digit
            sb.Insert(0, (char)('A' + n % 26));
            n /= 26;
        }
        
        return sb.ToString();
    }
}
