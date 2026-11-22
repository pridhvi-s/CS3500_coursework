using GUI.Components.Pages;

namespace GUI.Tests;

public class GameRenderViewTests
{
    [TestCase(0, 1.0)]
    [TestCase(-5, 1.0)]
    [TestCase(125, 0.5)]
    [TestCase(250, 0.0)]
    [TestCase(99999, 0.0)]
    public void FadeAlpha_ClampsAndIsLinear(double elapsed, double expected) =>
        Assert.That(GameRenderView.FadeAlpha(elapsed), Is.EqualTo(expected).Within(1e-9));

    [Test]
    public void ArrowFor_OnScreenIsNull_OffScreenPointsAndShrinks()
    {
        var head = new Vector2D();
        Assert.That(GameRenderView.ArrowFor(new Vector2D { X = 10, Y = 10 }, head, 1, 1000), Is.Null);
        var near = GameRenderView.ArrowFor(new Vector2D { X = 600, Y = 0 }, head, 1, 1000)!.Value;
        var far = GameRenderView.ArrowFor(new Vector2D { X = 2000, Y = 0 }, head, 1, 1000)!.Value;
        Assert.That(near.Angle, Is.EqualTo(0).Within(1e-9));
        Assert.That(near.X, Is.EqualTo(970).Within(1e-9));
        Assert.That(far.Size, Is.LessThan(near.Size));
    }

    [Test]
    public void EyeOffsets_MovingRight_AreVerticalAndOpposite()
    {
        var (l, r) = GameRenderView.EyeOffsets(new Vector2D { X = 1, Y = 0 }, 3);
        Assert.That(l.X, Is.EqualTo(0).Within(1e-9));
        Assert.That(r.X, Is.EqualTo(0).Within(1e-9));
        Assert.That(l.Y, Is.EqualTo(-r.Y).Within(1e-9));
        Assert.That(Math.Abs(l.Y), Is.EqualTo(3).Within(1e-9));
    }

    [Test]
    public void EyeOffsets_ZeroDir_NotNaN()
    {
        var (l, r) = GameRenderView.EyeOffsets(new Vector2D(), 3);
        Assert.That(double.IsNaN(l.X) || double.IsNaN(l.Y) || double.IsNaN(r.X) || double.IsNaN(r.Y), Is.False);
    }

    [Test]
    public void GrassScaleAndTiles_BoundTheWorkForHugeBoards()
    {
        Assert.That(GameRenderView.GrassScale(2000), Is.EqualTo(1.0));
        Assert.That(GameRenderView.GrassTileSize(2000), Is.EqualTo(GameRenderView.GrassTile));
        int huge = 1_000_000;
        Assert.That(GameRenderView.GrassScale(huge) * huge, Is.EqualTo(GameRenderView.GrassMaxPx).Within(1));
        Assert.That(huge / GameRenderView.GrassTileSize(huge), Is.LessThanOrEqualTo(GameRenderView.GrassMaxTilesPerSide + 1e-9));
        Assert.DoesNotThrow(() => GameRenderView.GrassScale(0));
    }
}
