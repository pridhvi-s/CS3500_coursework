using System;
using System.Text.Json.Serialization;

namespace GUI.Components.Pages;

/// <summary>
/// The Vector2D struct represents a two-dimensional vector with integer coordinates.
/// It is used to represent positions and movements in the Snake game.
/// The X and Y properties represent the horizontal
/// and vertical coordinates of the vector, respectively.
/// </summary>
public struct Vector2D
{
    /// <summary>
    /// The x-coordinate of the vector.
    /// </summary>
    /// <value>An integer representing the x-coordinate.</value>
    [JsonInclude]
    public int X { get; set; }
    
    /// <summary>
    /// The y-coordinate of the vector.
    /// </summary>
    /// <value>An integer representing the y-coordinate.</value>
    [JsonInclude]
    public int Y { get; set; }
}
