using GUI.Components.Pages;

namespace GUI.Tests;

public class GameStateTests
{
    [Test]
    public void ZoomIn_Multiplies()
    {
        var s = new GameState();
        s.ZoomIn();
        Assert.That(s.Zoom, Is.EqualTo(1.25).Within(1e-9));
    }

    [Test]
    public void Zoom_ClampsBothEnds()
    {
        var s = new GameState();
        for (int i = 0; i < 20; i++) s.ZoomIn();
        Assert.That(s.Zoom, Is.EqualTo(4.0));
        for (int i = 0; i < 40; i++) s.ZoomOut();
        Assert.That(s.Zoom, Is.EqualTo(0.25));
    }

    [Test]
    public void CopyTo_CopiesSnakeIdBoardSizeZoomMenu()
    {
        var a = new GameState { SnakeID = 3, BoardSize = 2000 };
        a.ZoomIn();
        a.Menu.Error = "boom";
        a.Menu.Ip = "1.2.3.4";
        var b = new GameState();
        a.CopyTo(ref b);
        Assert.That(b.SnakeID, Is.EqualTo(3));
        Assert.That(b.BoardSize, Is.EqualTo(2000));
        Assert.That(b.Zoom, Is.EqualTo(1.25).Within(1e-9));
        Assert.That(b.Menu.Error, Is.EqualTo("boom"));
        Assert.That(b.Menu.Ip, Is.EqualTo("1.2.3.4"));
    }

    [Test]
    public void CopyTo_IsSnapshot()
    {
        var a = new GameState();
        a.Snakes[1] = new Snake { ID = 1 };
        var b = new GameState();
        a.CopyTo(ref b);
        a.Snakes.Clear();
        Assert.That(b.Snakes, Has.Count.EqualTo(1));
    }
}
