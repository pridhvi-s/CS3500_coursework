using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GUI.Components.Pages;

/// <summary>
/// The Snake class represents a snake in the Snake game. It holds data about the snake's
/// current state, including its unique identifier, name, body segments, direction of movement,
/// score, and status (alive, dead, or disconnected). This class is used to manage
/// the state of each snake in the game and is updated by the server as the game progresses.
/// </summary>
public class Snake
{
    /// <summary>
    /// The unique identifier for the snake. This ID is assigned by the
    /// server when the client connects and is used to identify a snake in game.
    /// <value>A unique integer representing the snake's ID.</value>
    [JsonInclude]
    [JsonPropertyName("snake")]
    public int ID { get; set; }

    /// <summary>
    /// The name of the snake. This is the name that the user has chosen for their snake.
    /// This name is sent to the server when the user connects, and is used to identify
    /// the snake in game. The name is also displayed to other players in the game.
    /// </summary>
    /// <value>A string representing the snake's name.</value>
    [JsonInclude]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The body of the snake. This is a list of Vector2D objects that represent
    /// the positions of each segment of the snake's body on the game board. The first
    /// element in the list is the tail of the snake, and the last element is the head.
    /// The body is updated as the snake moves and grows in the game.
    /// </summary>
    /// <value>A list of Vector2D objects representing the positions of the snake's body segments.</value>
    [JsonInclude]
    [JsonPropertyName("body")]
    public List<Vector2D> Body { get; set; } = new List<Vector2D>();

    /// <summary>
    /// The direction in which the snake is currently moving. This is represented as a Vector2D object,
    /// where the X and Y properties indicate the horizontal and vertical components of the direction,
    /// respectively. 
    /// For example, a direction of (1, 0) means the snake is moving to the left,
    /// while a direction of (0, 1) means the snake is moving downwards.
    /// The direction is updated by the server.
    /// </summary>
    /// <value>A Vector2D object representing the direction of the snake's movement.</value>
    [JsonInclude]
    [JsonPropertyName("dir")]
    public Vector2D Direction { get; set; } = new Vector2D();

    /// <summary>
    /// The current score of the snake. This is an integer value that represents
    /// the number of points the snake has earned in the game. The score is updated
    /// by the server as the snake eats food and grows in length. The score is displayed
    /// to the user in the game UI.
    /// </summary>
    /// <value>An integer representing the snake's current score.</value>
    [JsonInclude]
    [JsonPropertyName("score")]
    public int Score { get; set; } = 0;

    /// <summary>
    /// A boolean value indicating whether the snake has just died. This is set only once
    /// by the server when the snake dies, repersents a event when the snake has died.
    /// </summary>
    /// <value>A boolean value indicating whether the snake has just died.</value>
    [JsonInclude]
    [JsonPropertyName("died")]
    public bool Died { get; set; } = false;

    /// <summary>
    /// A boolean value indicating whether the snake is currently alive. This is set by the server
    /// and is used to determine whether the snake should is not alive. If
    /// the snake is dead, we will not render the snake on the game board.
    /// </summary>
    /// <value>A boolean value indicating whether the snake is currently alive.</value>
    [JsonInclude]
    [JsonPropertyName("alive")]
    public bool Alive { get; set; } = true;
    
    /// <summary>
    /// A boolean value indicating whether the snake has disconnected from the server. This is set by the server
    /// and is used to determine whether the snake should be removed from the game board. If
    /// the snake has disconnected, we will remove the snake from the game board.
    /// </summary>
    /// <value> A boolean value indicating whether the snake has disconnected from the server.</value>
    [JsonInclude]
    [JsonPropertyName("dc")]
    public bool Disconnected { get; set; } = false;
}
