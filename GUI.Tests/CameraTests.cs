using GUI.Components.Pages;

namespace GUI.Tests;

public class CameraTests
{
    private static Vector2D V(int x, int y) => new() { X = x, Y = y };

    [Test]
    public void Head_MapsToCanvasCenter()
    {
        var (x, y) = GameRenderView.WorldToScreen(V(123, -456), V(123, -456), 2.5, 1000);
        Assert.That((x, y), Is.EqualTo((500.0, 500.0)));
    }

    [Test]
    public void Zoom_ScalesOffsets()
    {
        var (x, y) = GameRenderView.WorldToScreen(V(110, 80), V(100, 100), 2, 1000);
        Assert.That((x, y), Is.EqualTo((520.0, 460.0)));
    }

    [Test]
    public void FindHead_ReturnsLastVertex()
    {
        var s = new GameState { SnakeID = 1 };
        s.Snakes[1] = new Snake { ID = 1, Body = { V(0, 0), V(5, 9) } };
        Assert.That(GameRenderView.FindHead(s), Is.EqualTo(V(5, 9)));
    }

    [Test]
    public void FindHead_NullWhenSnakeMissingOrBodyEmptyOrNoId()
    {
        var s = new GameState();
        Assert.That(GameRenderView.FindHead(s), Is.Null);
        s.SnakeID = 1;
        Assert.That(GameRenderView.FindHead(s), Is.Null);
        s.Snakes[1] = new Snake { ID = 1 };
        Assert.That(GameRenderView.FindHead(s), Is.Null);
    }
}
