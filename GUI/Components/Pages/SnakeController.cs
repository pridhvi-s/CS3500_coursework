using System;
using System.Diagnostics;
using System.Text.Json;
using Networking;

namespace GUI.Components.Pages;

/// <summary>
/// Controller class for the Snake game. This class is responsible for
/// managing the state of the game, handling user input,
/// and communicating with the server. We take in an instance of NetworkConnection
/// to send and receive messages from the server.
/// </summary>
public class SnakeController
{
    /// <summary>
    /// The current network state of the client. This value is used to determine
    /// whether the client is connected to the server, in the process of handshaking,
    /// or disconnected. The network state is updated based on messages received from the server.
    /// </summary>
    private volatile NetworkState networkState;

    /// <summary>
    /// The thread used to handle network communication with the server. This thread is responsible for
    /// sending and receiving messages from the server, and updating the game state accordingly.
    /// </summary>
    private Thread? networkThread;

    /// <summary>
    /// The NetworkConnection instance used to communicate with the server.
    /// This value is always non-null, and may or may not be connected to the server at any given time.
    /// </summary>
    private NetworkConnection connection;

    /// <summary>
    /// Hold the current state of the game, including all snakes, food, and walls.
    /// This state is updated by the server and is used to render the game view.
    /// </summary>
    private GameState gameState;

    /// <summary>
    /// Released when there is something new to draw: a new server frame, or any menu/connection change.
    /// Holds at most one pending signal, so a slow renderer skips frames instead of falling behind.
    /// </summary>
    private readonly SemaphoreSlim frameSignal = new SemaphoreSlim(1, 1);

    /// <summary>Snake ids already seen in the server frame being received (network thread only).</summary>
    private readonly HashSet<int> frameSnakes = new HashSet<int>();

    /// <summary>True when the previous server message was a snake (network thread only).</summary>
    private bool lastWasSnake;

    /// <summary>The world as it was when the last full server frame finished (guarded by gameState lock).</summary>
    private readonly GameState completedFrame = new GameState();

    private bool hasCompletedFrame;

    /// <summary>Set by <see cref="Disconnect"/> when the page is closed; no new connection may start after.</summary>
    private volatile bool disposed;

    public SnakeController()
    {
        networkState = NetworkState.Disconnected;
        connection = new NetworkConnection();
        gameState = new GameState();
    }

    /// <summary>
    /// Connects to the server at the specified IP address and port, and sends the specified name to the server.
    /// This method initiates the connection process and begins the handshake with the server.
    /// </summary>
    /// <param name="ip"> The IP address of the server to connect to. </param>
    /// <param name="port"> The port number of the server to connect to. </param>
    /// <param name="name"> The name of the snake controlled by this client. </param>
    /// <exception cref="InvalidOperationException">Thrown if the client is already connected or in the process of handshaking with the server.</exception>
    public void Connect(string ip, int port, string name)
    {
        lock (this)
        {
            if (networkState != NetworkState.Disconnected)
            {
                throw new InvalidOperationException("Cannot connect while already connected or handshaking.");
            }

            if (disposed)
            {
                throw new InvalidOperationException("The controller was disconnected for good.");
            }

            // Claim the connection slot now so a second Enter cannot start a second connection.
            networkState = NetworkState.Handshaking;
        }

        // Drop everything left over from a previous session.
        lock (gameState)
        {
            gameState.Snakes.Clear();
            gameState.Walls.Clear();
            gameState.Foods.Clear();
            gameState.SnakeID = null;
            gameState.Menu.Error = null;
            completedFrame.Snakes.Clear();
            completedFrame.Foods.Clear();
            completedFrame.Walls.Clear();
            hasCompletedFrame = false;
        }

        if(networkThread != null && networkThread.IsAlive)
        {
            networkThread.Join();
        }

        // The old network thread is gone, so these are safe to reset.
        frameSnakes.Clear();
        lastWasSnake = false;

        // Spawn a new thread to handle the connection and handshake process with the server.
        // This allows the main thread to continue running and not block the UI.
        networkThread = new Thread(() => HandleConnection(ip, port, name)) { IsBackground = true };
        networkThread.Start();
    }

    /// <summary>
    /// Completes when there is a new frame to draw. A server frame starts with its snake messages and
    /// then carries the food, so the first snake after anything else (or a repeated snake id) starts a new frame.
    /// There is no timer: the draw rate follows the server, whatever it is.
    /// </summary>
    public Task WaitForFrameAsync(CancellationToken token) => frameSignal.WaitAsync(token);

    private void SignalFrame()
    {
        try
        {
            if (frameSignal.CurrentCount == 0)
            {
                frameSignal.Release();
            }
        }
        catch (SemaphoreFullException)
        {
            // Already signalled.
        }
    }

    /// <summary>
    /// Provides a copy of the current game state for rendering purposes. This method is thread-safe and
    /// ensures that the game state is not modified while being rendered.
    /// </summary>
    /// <returns>A copy of the current game state.</returns>
    public void GameStateForRenderView(ref GameState gameState)
    {
        lock (this.gameState)
        {
            this.gameState.CopyTo(ref gameState);
        }
    }

    /// <summary>
    /// Like <see cref="GameStateForRenderView"/>, but in the game the snakes, food and walls are those of the
    /// last <i>complete</i> server frame, so a frame still being received is never drawn half-updated.
    /// Menu, zoom and connection state are always current.
    /// </summary>
    public void GameStateForDrawing(ref GameState gameState)
    {
        lock (this.gameState)
        {
            this.gameState.CopyTo(ref gameState);
            if (hasCompletedFrame && gameState.CurrentState == GameState.State.Game)
            {
                completedFrame.CopyWorldTo(gameState);
            }
        }
    }

    /// <summary>
    /// Sends a move command to the server. This method is called when the user
    /// presses an arrow key to move the snake. The direction parameter indicates
    /// the direction in which the snake should move.
    /// 
    /// If this method is called during the connection menu state, nothing will happen.
    /// </summary>
    /// <param name="direction"> The direction in which the snake should move. </param>
    public void SendMove(Direction direction)
    {
        lock (gameState)
        {
            if (gameState.CurrentState != GameState.State.Game)
            {
                return;
            }
        }

        string moving = direction switch
        {
            Direction.Up => "up",
            Direction.Down => "down",
            Direction.Left => "left",
            _ => "right",
        };

        NetworkConnection current;
        lock (this)
        {
            current = connection;
        }

        try
        {
            current.Send($"{{\"moving\":\"{moving}\"}}");
        }
        catch (InvalidOperationException)
        {
            // The connection already closed; the network thread will return us to the menu.
        }
    }

    /// <summary>
    /// Handles a key press from the page. In the menu the key edits the connect form (Enter connects);
    /// in the game WASD/arrows steer and N/M zoom. Anything else is ignored.
    /// </summary>
    /// <param name="key"> The KeyboardEvent.key string. </param>
    public void HandleKey(string key)
    {
        try
        {
            HandleKeyCore(key);
        }
        finally
        {
            // Typing, zooming or connecting changes what the menu/world looks like.
            SignalFrame();
        }
    }

    private void HandleKeyCore(string key)
    {
        string ip;
        int port;
        string name;
        Direction? move = null;

        lock (gameState)
        {
            if (gameState.CurrentState == GameState.State.ConnectMenu)
            {
                if (!gameState.Menu.HandleKey(key) || networkState != NetworkState.Disconnected)
                {
                    return;
                }

                if (!int.TryParse(gameState.Menu.Port, out port) || port < 1 || port > 65535)
                {
                    gameState.Menu.Error = "Port must be a number from 1 to 65535";
                    return;
                }

                ip = gameState.Menu.Ip;
                name = gameState.Menu.Name;
            }
            else
            {
                switch (key.ToLowerInvariant())
                {
                    case "w": case "arrowup": move = Direction.Up; break;
                    case "a": case "arrowleft": move = Direction.Left; break;
                    case "s": case "arrowdown": move = Direction.Down; break;
                    case "d": case "arrowright": move = Direction.Right; break;
                    case "n": gameState.ZoomIn(); break;
                    case "m": gameState.ZoomOut(); break;
                }

                if (move == null)
                {
                    return;
                }

                ip = string.Empty;
                port = 0;
                name = string.Empty;
            }
        }

        // Network I/O happens outside the state lock so it cannot stall rendering.
        if (move is Direction d)
        {
            SendMove(d);
            return;
        }

        try
        {
            Connect(ip, port, name);
        }
        catch (InvalidOperationException)
        {
            // Already connecting; ignore the extra Enter.
        }
    }

    /// <summary>
    /// Closes the current connection (if any) so the network thread ends, and refuses any later
    /// <see cref="Connect"/>. Meant for when the page closes. Safe to call repeatedly.
    /// </summary>
    public void Disconnect()
    {
        NetworkConnection current;
        lock (this)
        {
            disposed = true;
            current = connection;
        }

        current.Disconnect();
    }

    private void HandshakeConnection(string name)
    {
        // Update the network state to indicate that we are in the process of handshaking with the server.
        lock (this)
        {
            networkState = NetworkState.Handshaking;
        }

        // A server that accepts but never answers must not hang the menu forever.
        connection.ReceiveTimeout = HandshakeTimeoutMs;

        // Write our user name to the server as part of the handshake process.
        connection.Send(name);

        // We are waiting for first our snake ID from the server
        string? snakeIdMessage = connection.ReadLine();
        if (snakeIdMessage == null || !int.TryParse(snakeIdMessage, out int snakeId))
        {
            throw new InvalidOperationException("Failed to receive valid snake ID from server.");
        }

        // We now receive the size of the game board from the server
        string? boardSizeMessage = connection.ReadLine();
        if (boardSizeMessage == null || !int.TryParse(boardSizeMessage, out int boardSize))
        {
            throw new InvalidOperationException("Failed to receive valid board size from server.");
        }

        // Update the game state with the received snake ID and board size.
        lock (this.gameState)
        {
            gameState.SnakeID = snakeId;
            gameState.BoardSize = boardSize;
            
            // We are connected and ready to render the game.
            gameState.CurrentState = GameState.State.Game;
        }

        // Game updates can be quiet for a while, so wait indefinitely from here on.
        connection.ReceiveTimeout = 0;
        networkState = NetworkState.Connected;
    }

    private const int HandshakeTimeoutMs = 5000;

    private void ProcessServerMessage(string message)
    {
        // Parse the message from the server and update the game state accordingly.
        try
        {
            using var json = JsonDocument.Parse(message);
            bool wasSnake = lastWasSnake;
            lastWasSnake = json.RootElement.TryGetProperty("snake", out _);

            // Parse as either Snake, Wall, or Food, and update the game state accordingly.
            if (json.RootElement.TryGetProperty("snake", out var snakeId)) {
                Snake? snake = json.RootElement.Deserialize<Snake>();
                if (snake == null)
                {
                    Debug.WriteLine($"Failed to deserialize snake from message: {message}");
                    return;
                }

                bool newFrame = !wasSnake || !frameSnakes.Add(snake.ID);
                if (newFrame)
                {
                    frameSnakes.Clear();
                    frameSnakes.Add(snake.ID);
                }

                lock (this.gameState)
                {
                    if (newFrame)
                    {
                        // Everything so far belongs to the frame that just ended: keep it for drawing.
                        gameState.CopyWorldTo(completedFrame);
                        hasCompletedFrame = true;
                    }

                    if (snake.Disconnected)
                    {
                        gameState.Snakes.Remove(snake.ID);
                    }
                    else
                    {
                        gameState.Snakes[snake.ID] = snake;
                    }
                }

                if (newFrame)
                {
                    SignalFrame();
                }
            } else if (json.RootElement.TryGetProperty("wall", out var wallId)) {
                Wall? wall = json.RootElement.Deserialize<Wall>();
                if (wall == null)
                {
                    Debug.WriteLine($"Failed to deserialize wall from message: {message}");
                    return;
                }

                lock (this.gameState)
                {
                    this.gameState.Walls[wall.ID] = wall;
                }
            } else if (json.RootElement.TryGetProperty("power", out var foodId)) {
                Food? food = json.RootElement.Deserialize<Food>();
                if (food == null)
                {
                    Debug.WriteLine($"Failed to deserialize food from message: {message}");
                    return;
                }

                lock (this.gameState)
                {
                    if (food.Died)
                    {
                        this.gameState.Foods.Remove(food.ID);
                    }
                    else
                    {
                        this.gameState.Foods[food.ID] = food;
                    }
                }
            } else {
                Debug.WriteLine($"Unknown message type received from server: {message}");
            }
        } catch (Exception ex)
        {
            Debug.WriteLine($"Failed to process server message: {ex.Message}");
        }
    }

    private void HandleConnection(string ip, int port, string name)
    {
        // Implementation for handling the connection and handshake with the server.
        // This method will manage the network state and update the game state based on messages from the server.
        try
        {
            // Attempt to connect to the server using the provided IP address and port.
            connection.Connect(ip, port);

            // After successfully connecting, initiate the handshake process with the server.
            HandshakeConnection(name);
            while (connection.IsConnected)
            {
                // Read messages from the server and update the game state accordingly.
                string? message = connection.ReadLine();
                if (message == null)
                {
                    break;
                }

                // Process the received message and update the game state.
                ProcessServerMessage(message);
            }
        } catch(Exception ex)
        {
            Debug.WriteLine($"Connection ended: {ex.Message}");
            lock (this.gameState)
            {
                this.gameState.Menu.Error = ex.Message;
            }
        }
        finally
        {
            // Always return to the menu. We keep a non-disposed NetworkConnection
            // available, so dispose the old one and create a fresh one.
            // Free the connection slot first: anyone who sees the menu may reconnect immediately.
            lock (this)
            {
                this.connection.Dispose();
                this.connection = new NetworkConnection();
                networkState = NetworkState.Disconnected;
            }

            lock (this.gameState)
            {
                gameState.CurrentState = GameState.State.ConnectMenu;
            }

            SignalFrame();
        }
    }

    private enum NetworkState
    {
        Disconnected,
        Handshaking,
        Connected
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}