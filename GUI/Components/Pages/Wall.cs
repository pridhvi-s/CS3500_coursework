using System;
using System.Text.Json.Serialization;

namespace GUI.Components.Pages;

public class Wall
{
    /// <summary>
    /// The unique identifier for the wall. This ID is assigned by the
    /// server when the client connects and is used to identify a wall in game.
    /// </summary>
    /// <value> A unique integer representing the wall's ID.</value>
    [JsonInclude]
    [JsonPropertyName("wall")]
    public int ID { get; set; }

    /// <summary>
    /// The start point of the wall on the game board. This is represented as a Vector2D object,
    /// where the X and Y properties indicate the horizontal and vertical coordinates of the wall,
    /// respectively.
    /// </summary>
    /// <returns>The start point of the wall.</returns>
    [JsonInclude]
    [JsonPropertyName("p1")]
    public Vector2D Point1 { get; set; } = new Vector2D();

    /// <summary>
    /// The end point of the wall on the game board. This is represented as a VectorD object,
    /// where the X and Y properties indicate the horizontal and vertical coordinates of the wall,
    /// respectively.
    /// </summary>
    /// <returns>The end point of the wall.</returns>
    [JsonInclude]
    [JsonPropertyName("p2")]
    public Vector2D Point2 { get; set; } = new Vector2D();
}
