using System;

namespace GUI.Components.Pages;

/// <summary>
/// This class represents the current state of the Snake game.
/// It is used to determine which parts of the game should be displayed to the user.
/// 
/// This class holds data about the current state of the game, such as the all snakes,
/// food state, and the current state of the game (ConnectMenu or Game).
/// </summary>
public class GameState
{
    /// <summary>
    /// New empty instance of the GameState class. This constructor initializes the game state to the ConnectMenu state.
    /// </summary>
    public GameState()
    {
        CurrentState = State.ConnectMenu;
    }

    /// <summary>
    /// Copy the current state of the game to another instance of the GameState class.
    /// This method creates a deep copy of the current game state, including all snakes, food, and walls.
    /// </summary>
    /// <param name="other"> The GameState instance to copy the current state to. </param>
    public void CopyTo(ref GameState other)
    {
        other.CurrentState = this.CurrentState;
        other.SnakeID = this.SnakeID;
        other.BoardSize = this.BoardSize;
        other.Zoom = this.Zoom;
        this.Menu.CopyTo(other.Menu);
        CopyWorldTo(other);
    }

    /// <summary>
    /// Replaces the snakes, food and walls of <paramref name="other"/> with this state's.
    /// The target's dictionaries are reused, so a per-frame copy does not allocate.
    /// </summary>
    public void CopyWorldTo(GameState other)
    {
        Refill(other.Snakes, Snakes);
        Refill(other.Foods, Foods);
        Refill(other.Walls, Walls);
    }

    private static void Refill<T>(Dictionary<int, T> target, Dictionary<int, T> source)
    {
        target.Clear();
        foreach (var pair in source)
        {
            target[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// The unique identifier for the snake controlled by this client.
    /// Null when the client is not connected to the server,
    /// or when the server has not yet assigned an ID to this client.
    /// </summary>
    public int? SnakeID { get; set; } = null;

    /// <summary>
    /// The size of the game board. This value is set by the server when the client connects,
    /// and is used to determine the boundaries of the game board. The board size is represented
    /// as an integer value, where the game board is a square of size BoardSize x BoardSize.
    /// </summary>
    /// <value></value>
    public int BoardSize { get; set; } = 0;

    /// <summary>
    /// The list of all snakes currently in the game.
    /// </summary>
    /// <typeparam name="Snake"> The type of the snake objects in the list.</typeparam>
    /// <returns> A list of Snake objects representing the current snakes in the game. </returns>
    public Dictionary<int, Snake> Snakes { get; private set; } = new Dictionary<int, Snake>();

    /// <summary>
    /// The list of all food items currently in the game.
    /// </summary>
    /// <typeparam name="Food"> The type of the food objects in the list.</typeparam>
    /// <returns> A list of Food objects representing the current food items in the game. </returns>
    public Dictionary<int, Food> Foods { get; private set; } = new Dictionary<int, Food>();

    /// <summary>
    /// The list of all walls currently in the game.
    /// </summary>
    /// <typeparam name="Wall"> The type of the wall objects in the list.</typeparam>
    /// <returns> A list of Wall objects representing the current walls in the game. </returns>
    public Dictionary<int, Wall> Walls { get; private set; } = new Dictionary<int, Wall>();

    /// <summary>
    /// The current state of the game, which can be either ConnectMenu or Game.
    /// This is a client only state, and is used to determine which parts of the game should be displayed to the user.
    /// </summary>
    public State CurrentState { get; set; }

    /// <summary>The connect menu fields (client only).</summary>
    public ConnectMenu Menu { get; } = new ConnectMenu();

    /// <summary>World-to-pixel scale factor, changed with N and M. Client only.</summary>
    public double Zoom { get; private set; } = 1.0;

    private const double ZoomStep = 1.25, MinZoom = 0.25, MaxZoom = 4.0;

    public void ZoomIn() => Zoom = Math.Min(MaxZoom, Zoom * ZoomStep);

    public void ZoomOut() => Zoom = Math.Max(MinZoom, Zoom / ZoomStep);

    public enum State
    {
        ConnectMenu,
        Game,
    }
}
