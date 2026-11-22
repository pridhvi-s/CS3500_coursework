using System.Net;
using System.Net.Sockets;
using GUI.Components.Pages;

namespace GUI.Tests;

/// <summary>Minimal stand-in for the snake server: handshake plus a few JSON lines.</summary>
internal sealed class FakeServer : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly Task<TcpClient> _accept;
    private StreamReader? _in;
    private StreamWriter? _out;

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;
    public bool ClientSeen => _accept.IsCompletedSuccessfully;

    public FakeServer()
    {
        _listener.Start();
        _accept = _listener.AcceptTcpClientAsync();
    }

    /// <summary>Waits for the client, reads its name, sends the handshake and the given lines.</summary>
    public async Task HandshakeAsync(int id, int size, params string[] lines)
    {
        var c = await _accept.WaitAsync(TimeSpan.FromSeconds(3));
        _in = new StreamReader(c.GetStream());
        _out = new StreamWriter(c.GetStream()) { AutoFlush = true };
        await _in.ReadLineAsync();
        await _out.WriteLineAsync(id.ToString());
        await _out.WriteLineAsync(size.ToString());
        foreach (var l in lines) await _out.WriteLineAsync(l);
    }

    public Task SendAsync(string line) => _out!.WriteLineAsync(line);

    public async Task<string?> ReadCommandAsync() =>
        await _in!.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(3));

    public void Dispose()
    {
        _out?.Dispose();
        _in?.Dispose();
        _listener.Stop();
    }
}

public class SnakeControllerTests
{
    private const string Wall = "{\"wall\":1,\"p1\":{\"X\":-100,\"Y\":0},\"p2\":{\"X\":100,\"Y\":0}}";
    private const string Food = "{\"power\":5,\"loc\":{\"X\":10,\"Y\":10},\"died\":false}";

    private static string SnakeJson(int id, bool dc = false) =>
        $"{{\"snake\":{id},\"name\":\"s{id}\",\"body\":[{{\"X\":0,\"Y\":0}},{{\"X\":0,\"Y\":100}}],\"dir\":{{\"X\":0,\"Y\":1}},\"score\":0,\"died\":false,\"alive\":true,\"dc\":{dc.ToString().ToLower()}}}";

    private static GameState Snap(SnakeController c)
    {
        var s = new GameState();
        c.GameStateForRenderView(ref s);
        return s;
    }

    private static void WaitFor(Func<bool> cond)
    {
        var end = DateTime.UtcNow.AddSeconds(3);
        while (!cond())
        {
            if (DateTime.UtcNow > end) Assert.Fail("Timed out waiting for condition");
            Thread.Sleep(10);
        }
    }

    private static void Type(SnakeController c, string s)
    {
        foreach (char ch in s) c.HandleKey(ch.ToString());
    }

    /// <summary>Sets the menu fields through key presses, then presses Enter.</summary>
    private static void ConnectVia(SnakeController c, string ip, string port)
    {
        while (Snap(c).Menu.Focused != MenuField.Ip) c.HandleKey("Tab");
        for (int i = 0; i < 40; i++) c.HandleKey("Backspace");
        Type(c, ip);
        c.HandleKey("Tab");
        for (int i = 0; i < 40; i++) c.HandleKey("Backspace");
        Type(c, port);
        c.HandleKey("Enter");
    }

    private static async Task<(SnakeController c, FakeServer s)> ConnectedAsync(params string[] extra)
    {
        var s = new FakeServer();
        var c = new SnakeController();
        ConnectVia(c, "127.0.0.1", s.Port.ToString());
        await s.HandshakeAsync(7, 2000, extra.Length > 0 ? extra : new[] { Wall, SnakeJson(7), Food });
        WaitFor(() => Snap(c).CurrentState == GameState.State.Game && Snap(c).Foods.Count == 1);
        return (c, s);
    }

    [Test]
    public async Task Enter_Connects_And_HandshakeFillsState()
    {
        var (c, s) = await ConnectedAsync();
        using var _ = s;
        var snap = Snap(c);
        Assert.That(snap.SnakeID, Is.EqualTo(7));
        Assert.That(snap.BoardSize, Is.EqualTo(2000));
        Assert.That(snap.Walls, Has.Count.EqualTo(1));
        Assert.That(snap.Snakes, Has.Count.EqualTo(1));
        Assert.That(snap.Foods, Has.Count.EqualTo(1));
        c.Disconnect();
    }

    [Test]
    public async Task WASD_And_Arrows_SendMoveJson()
    {
        var (c, s) = await ConnectedAsync();
        using var _ = s;
        foreach (var (key, dir) in new[] { ("W", "up"), ("ArrowLeft", "left"), ("s", "down"), ("d", "right") })
        {
            c.HandleKey(key);
            Assert.That(await s.ReadCommandAsync(), Is.EqualTo($"{{\"moving\":\"{dir}\"}}"));
        }
        c.Disconnect();
    }

    [Test]
    public async Task NandM_Zoom_InGameOnly()
    {
        var c = new SnakeController();
        c.HandleKey("n");
        Assert.That(Snap(c).Zoom, Is.EqualTo(1.0));

        var (c2, s) = await ConnectedAsync();
        using var _ = s;
        c2.HandleKey("N");
        Assert.That(Snap(c2).Zoom, Is.EqualTo(1.25).Within(1e-9));
        c2.HandleKey("m");
        Assert.That(Snap(c2).Zoom, Is.EqualTo(1.0).Within(1e-9));
        c2.Disconnect();
    }

    [Test]
    public void Typing_WInMenu_EditsFieldOnly()
    {
        var c = new SnakeController();
        c.HandleKey("w");
        Assert.That(Snap(c).Menu.Ip, Is.EqualTo("localhostw"));
        Assert.That(Snap(c).CurrentState, Is.EqualTo(GameState.State.ConnectMenu));
    }

    [TestCase("")]
    [TestCase("abc")]
    [TestCase("0")]
    [TestCase("70000")]
    public void BadPort_ShowsError_NoConnect(string port)
    {
        using var s = new FakeServer();
        var c = new SnakeController();
        ConnectVia(c, "127.0.0.1", port);
        var snap = Snap(c);
        Assert.That(snap.CurrentState, Is.EqualTo(GameState.State.ConnectMenu));
        Assert.That(snap.Menu.Error, Is.Not.Null);
        Thread.Sleep(100);
        Assert.That(s.ClientSeen, Is.False);
    }

    [Test]
    public async Task ServerClose_ReturnsToMenuWithError()
    {
        var (c, s) = await ConnectedAsync();
        s.Dispose();
        WaitFor(() => Snap(c).CurrentState == GameState.State.ConnectMenu);
        Assert.That(Snap(c).Menu.Error, Is.Not.Empty);
    }

    [Test]
    public void Refused_ReturnsToMenuWithError()
    {
        int port;
        using (var l = new TcpListener(IPAddress.Loopback, 0))
        {
            l.Start();
            port = ((IPEndPoint)l.LocalEndpoint).Port;
        }
        var c = new SnakeController();
        ConnectVia(c, "127.0.0.1", port.ToString());
        WaitFor(() => Snap(c).Menu.Error != null);
        Assert.That(Snap(c).CurrentState, Is.EqualTo(GameState.State.ConnectMenu));
    }

    [Test]
    public async Task DisconnectedSnake_IsRemoved()
    {
        var (c, s) = await ConnectedAsync();
        using var _ = s;
        await s.SendAsync(SnakeJson(8));
        WaitFor(() => Snap(c).Snakes.ContainsKey(8));
        await s.SendAsync(SnakeJson(8, dc: true));
        WaitFor(() => !Snap(c).Snakes.ContainsKey(8));
        c.Disconnect();
    }

    [Test]
    public async Task Reconnect_ClearsOldStateAndWorksAgain()
    {
        var (c, s) = await ConnectedAsync();
        s.Dispose();
        WaitFor(() => Snap(c).CurrentState == GameState.State.ConnectMenu);

        using var s2 = new FakeServer();
        ConnectVia(c, "127.0.0.1", s2.Port.ToString());
        await s2.HandshakeAsync(9, 1000, SnakeJson(9));
        WaitFor(() => Snap(c).CurrentState == GameState.State.Game && Snap(c).Snakes.ContainsKey(9));
        var snap = Snap(c);
        Assert.That(snap.Walls, Is.Empty);
        Assert.That(snap.Snakes.ContainsKey(7), Is.False);
        c.Disconnect();
    }

    [Test]
    public void SendMove_InMenu_DoesNothing()
    {
        var c = new SnakeController();
        Assert.DoesNotThrow(() => c.SendMove(SnakeController.Direction.Up));
    }

    [Test]
    public async Task MalformedAndUnknownMessages_AreIgnored()
    {
        var (c, s) = await ConnectedAsync();
        using var _ = s;
        await s.SendAsync("not json");
        await s.SendAsync("{\"mystery\":1}");
        await s.SendAsync(SnakeJson(3));
        WaitFor(() => Snap(c).Snakes.ContainsKey(3));
        Assert.That(Snap(c).CurrentState, Is.EqualTo(GameState.State.Game));
    }

    private static async Task<bool> SignalledAsync(SnakeController c, int ms)
    {
        using var cts = new CancellationTokenSource(ms);
        try
        {
            await c.WaitForFrameAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    [Test]
    public async Task NewServerFrame_SignalsOncePerFrame()
    {
        var (c, s) = await ConnectedAsync();
        using var _ = s;
        while (await SignalledAsync(c, 100)) { }

        await s.SendAsync(SnakeJson(1));
        Assert.That(await SignalledAsync(c, 3000), Is.True, "snake after food starts a frame");
        await s.SendAsync(SnakeJson(2));
        await s.SendAsync(Food);
        Assert.That(await SignalledAsync(c, 300), Is.False, "snake after snake is the same frame");

        await s.SendAsync(SnakeJson(1));
        Assert.That(await SignalledAsync(c, 3000), Is.True, "a repeated snake id or snake after food starts the next frame");
    }

    [Test]
    public void Refused_ErrorTextIsNotAnAggregate()
    {
        int port;
        using (var l = new TcpListener(IPAddress.Loopback, 0))
        {
            l.Start();
            port = ((IPEndPoint)l.LocalEndpoint).Port;
        }
        var c = new SnakeController();
        ConnectVia(c, "127.0.0.1", port.ToString());
        WaitFor(() => Snap(c).Menu.Error != null);
        Assert.That(Snap(c).Menu.Error, Does.Not.Contain("One or more errors"));
    }

    [Test]
    public async Task Drawing_ShowsLastCompleteFrame_NotHalfReceivedOne()
    {
        var (c, s) = await ConnectedAsync(); // live: snake 7 and a food
        using var _ = s;
        await s.SendAsync(SnakeJson(1));     // first message of the next frame
        WaitFor(() => Snap(c).Snakes.ContainsKey(1));

        var drawn = new GameState();
        c.GameStateForDrawing(ref drawn);
        Assert.That(drawn.Snakes.ContainsKey(7), Is.True);
        Assert.That(drawn.Snakes.ContainsKey(1), Is.False, "snake 1 belongs to the frame still arriving");
        Assert.That(drawn.Foods, Has.Count.EqualTo(1));
        c.Disconnect();
    }

    [Test]
    public void Disconnect_DuringHandshake_ReturnsToMenuWithError()
    {
        using var s = new FakeServer();      // accepts but never answers
        var c = new SnakeController();
        ConnectVia(c, "127.0.0.1", s.Port.ToString());
        WaitFor(() => s.ClientSeen);
        c.Disconnect();
        WaitFor(() => Snap(c).Menu.Error != null);
        Assert.That(Snap(c).CurrentState, Is.EqualTo(GameState.State.ConnectMenu));
    }

    [Test]
    public void Connect_AfterDisconnect_StartsNothing()
    {
        using var s = new FakeServer();
        var c = new SnakeController();
        c.Disconnect();
        Assert.Throws<InvalidOperationException>(() => c.Connect("127.0.0.1", s.Port, "x"));
        Thread.Sleep(100);
        Assert.That(s.ClientSeen, Is.False);
    }
}
