using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GUI.Components.Pages;

/// <summary>
/// The Food class represents a food item in the Snake game. It holds data about the food's
/// current state, including its unique identifier, position, and died status.
/// </summary>
public class Food
{
    /// <summary>
    /// The unique identifier for the food. This ID is assigned by the
    /// server when the client connects and is used to identify a food in game.
    /// <value>A unique integer representing the food's ID.</value>
    [JsonInclude]
    [JsonPropertyName("power")]
    public int ID { get; set; }


    /// <summary>
    /// The position of the food on the game board. This is represented as a Vector2D object,
    /// where the X and Y properties indicate the horizontal and vertical coordinates of the food,
    /// respectively. The position is updated by the server as the game progresses.
    /// </summary>
    /// <returns></returns>
    [JsonInclude]
    [JsonPropertyName("loc")]
    public Vector2D Location { get; set; } = new Vector2D();

    /// <summary>
    /// A boolean value indicating whether the food has just died. This is set only once
    /// by the server when the food dies, repersents a event when the food has died.
    /// </summary>
    /// <value>A boolean value indicating whether the food has just died.</value>
    [JsonInclude]
    [JsonPropertyName("died")]
    public bool Died { get; set; } = false;

}
