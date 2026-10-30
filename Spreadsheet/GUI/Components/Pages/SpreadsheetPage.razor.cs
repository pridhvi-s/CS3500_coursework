// <copyright file="SpreadsheetPage.razor.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text;
using Formula;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Spreadsheets;

namespace GUI.Components.Pages;

/// <summary>
/// TODO: Fill in
/// </summary>
public partial class SpreadsheetPage
{
    /// <summary>
    /// 
    /// </summary>
    private string backingfocusedtextinput;
    
    /// <summary>
    /// Holds the user input (valid or invalid)
    /// </summary>
    private string FocusedTextInput {
        get
        {
            return backingfocusedtextinput;
        }
        set 
        {
            try
            {
                var updatedCells = spreadsheet.SetContentsOfCell(SelectedCellName, value);
                foreach (string cell in updatedCells)
                {
                    var (row, col) = ParseCellName(cell);
                    CellsBackingStore[row, col] = spreadsheet.GetCellValue(cell).ToString();
                }
            }
            catch (FormulaFormatException e)
            {
                Console.WriteLine("expected2");
            }
            finally
            {
                backingfocusedtextinput = value;
            }

        }
    }

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
    private string[,] CellsBackingStore { get; set; } = new string[Rows, Cols];

    /// <summary>
    /// The row of the currently selected cell.
    /// </summary>
    /// <value>The Cell row</value>
    public int SelectedRow { get; set; } = 1;

    /// <summary>
    /// The column of the currently selected cell.
    /// </summary>
    /// <value> The Cell column </value>
    public int SelectedCol { get; set; } = 1;

    public string SelectedCellContents
    {
        get
        {
            if (SelectedRow < 0 || SelectedCol < 0)
            {
                return string.Empty;
            }

            return spreadsheet.GetCellContents(SelectedCellName).ToString();
        }
        set
        {
            if (SelectedRow < 0 || SelectedCol < 0)
            {
                return;
            }
                var updatedCells = spreadsheet.SetContentsOfCell(SelectedCellName, value);
                foreach (string cell in updatedCells)
                {
                    var (row, col) = ParseCellName(cell);
                    CellsBackingStore[row, col] = spreadsheet.GetCellValue(cell).ToString();
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
    }


    /// <summary>
    /// Saves the current spreadsheet, by providing a download of a file
    /// containing the json representation of the spreadsheet.
    /// </summary>
    private async void SaveFile()
    {
        await JsRuntime.InvokeVoidAsync( "downloadFile", FileSaveName, 
            "replace this with the json representation of the current spreadsheet" );
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

                // TODO: Use the loaded fileContent to replace the current spreadsheet

                StateHasChanged();
            }
        }
        catch ( Exception e )
        {
            Debug.WriteLine( "an error occurred while loading the file..." + e );
        }
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
