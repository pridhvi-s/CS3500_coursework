using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;

namespace GUI.Components.Pages;

/// <summary>
/// View: draws the connect menu or the game world from a <see cref="GameState"/> snapshot.
/// It never changes game state. The camera is centered on our snake's head.
/// </summary>
public class GameRenderView
{
    private const double CanvasSize = 1000;
    private const double SnakeWidth = 10;
    private const double FoodSize = 16;
    private const double WallWidth = 50;
    private static readonly string[] SnakeColors = { "blue", "yellow", "black", "forestgreen", "white" };

    public const double FadeMs = 250;
    private const double BurstMs = 300;
    private const int MaxScoreRows = 10;
    private const double AppleAspect = 561.0 / 640.0;

    /// <summary>The apple &lt;img&gt; element, set by the page; apples are skipped until it is set.</summary>
    public ElementReference? AppleImage { get; set; }

    /// <summary>
    /// The pre-rendered grass for the whole world (one pixel per world unit), built by the page once the
    /// board size is known; plain green is used until it is set.
    /// </summary>
    public ElementReference? GrassWorld { get; set; }

    /// <summary>Side of one grass tile in world units.</summary>
    public const double GrassTile = 60;

    /// <summary>Longest side, in pixels, of the pre-rendered grass canvas; bigger worlds are drawn scaled down.</summary>
    public const double GrassMaxPx = 4096;

    /// <summary>Most grass tiles along one side; bigger worlds use bigger tiles so building stays fast.</summary>
    public const double GrassMaxTilesPerSide = 40;

    /// <summary>Pixels per world unit of the grass canvas: 1 up to <see cref="GrassMaxPx"/> world units, smaller after.</summary>
    public static double GrassScale(int boardSize) => Math.Min(1.0, GrassMaxPx / Math.Max(1, boardSize));

    /// <summary>Side of one grass tile in world units: <see cref="GrassTile"/>, or larger so a side never needs more than the max tiles.</summary>
    public static double GrassTileSize(int boardSize) => Math.Max(GrassTile, boardSize / GrassMaxTilesPerSide);

    /// <summary>Fraction of the tile width, per side, that fades out; neighbouring tiles overlap by this much.</summary>
    public const double GrassFeather = 0.2;

    /// <summary>When each dead snake was first seen dead; cleared on respawn.</summary>
    private readonly Dictionary<int, DateTime> deathTimes = new();

    /// <summary>Opacity of a snake that died <paramref name="elapsedMs"/> ago: 1 down to 0 over <see cref="FadeMs"/>.</summary>
    public static double FadeAlpha(double elapsedMs) => Math.Clamp(1 - elapsedMs / FadeMs, 0, 1);

    /// <summary>Two eye offsets from the head, perpendicular to <paramref name="dir"/>, <paramref name="spread"/> each side.</summary>
    public static ((double X, double Y) Left, (double X, double Y) Right) EyeOffsets(Vector2D dir, double spread)
    {
        double len = Math.Sqrt((double)dir.X * dir.X + (double)dir.Y * dir.Y);
        double dx = len == 0 ? 1 : dir.X / len, dy = len == 0 ? 0 : dir.Y / len;
        return ((-dy * spread, dx * spread), (dy * spread, -dx * spread));
    }

    /// <summary>Last seen location of every apple, so an apple that vanishes (eaten) can burst where it was.</summary>
    private readonly Dictionary<int, Vector2D> seenFoods = new();

    private readonly List<(Vector2D Loc, DateTime Start)> bursts = new();

    /// <summary>Scratch lists reused every frame to avoid allocating.</summary>
    private readonly List<int> staleIds = new();
    private readonly List<Snake> scoreRows = new();

    /// <summary>
    /// Where to draw an arrow for an off-screen <paramref name="target"/>: null if it is on screen, else the
    /// canvas-edge position, the angle it points, and a size that shrinks with world distance.
    /// </summary>
    public static (double X, double Y, double Angle, double Size)? ArrowFor(Vector2D target, Vector2D head, double zoom, double canvasSize)
    {
        var (sx, sy) = WorldToScreen(target, head, zoom, canvasSize);
        if (sx >= 0 && sx <= canvasSize && sy >= 0 && sy <= canvasSize)
        {
            return null;
        }

        double c = canvasSize / 2, dx = sx - c, dy = sy - c;
        double k = (c - 30) / Math.Max(Math.Abs(dx), Math.Abs(dy));
        double worldDist = Math.Sqrt(dx * dx + dy * dy) / zoom;
        return (c + dx * k, c + dy * k, Math.Atan2(dy, dx), Math.Clamp(30 - worldDist / 40, 8, 30));
    }

    /// <summary>The snapshot the next frame will draw; filled by the page from the controller.</summary>
    public GameState renderState = new GameState();

    /// <summary>Last known head position; used when our snake is not in the state (yet).</summary>
    private Vector2D lastHead;

    /// <summary>Maps a world position to canvas pixels, keeping <paramref name="head"/> at the canvas center.</summary>
    public static (double X, double Y) WorldToScreen(Vector2D world, Vector2D head, double zoom, double canvasSize) =>
        ((world.X - head.X) * zoom + canvasSize / 2, (world.Y - head.Y) * zoom + canvasSize / 2);

    /// <summary>The last vertex of our snake's body, or null if we have no snake with a body yet.</summary>
    public static Vector2D? FindHead(GameState s)
    {
        if (s.SnakeID is int id && s.Snakes.TryGetValue(id, out Snake? snake) && snake.Body.Count > 0)
        {
            return snake.Body[^1];
        }

        return null;
    }

    public async Task RenderGameAsync(Canvas2DContext ctx)
    {
        await ctx.BeginBatchAsync();
        try
        {
            if (renderState.CurrentState == GameState.State.ConnectMenu)
            {
                seenFoods.Clear();
                bursts.Clear();
                lastHead = default;
                await RenderMenuAsync(ctx);
            }
            else
            {
                await RenderWorldAsync(ctx);
            }
        }
        finally
        {
            await ctx.EndBatchAsync();
        }
    }

    private async Task RenderMenuAsync(Canvas2DContext ctx)
    {
        ConnectMenu menu = renderState.Menu;

        await ctx.SetFillStyleAsync("#1e1e2e");
        await ctx.FillRectAsync(0, 0, CanvasSize, CanvasSize);

        await ctx.SetFillStyleAsync("white");
        await ctx.SetFontAsync("bold 48px sans-serif");
        await ctx.FillTextAsync("Snake", 400, 200);

        string[] labels = { "Server IP", "Port", "Name" };
        string[] values = { menu.Ip, menu.Port, menu.Name };
        for (int i = 0; i < 3; i++)
        {
            double y = 300 + i * 100;
            bool focused = (int)menu.Focused == i;

            await ctx.SetFillStyleAsync("#aaaaaa");
            await ctx.SetFontAsync("18px sans-serif");
            await ctx.FillTextAsync(labels[i], 300, y - 8);

            await ctx.SetFillStyleAsync(focused ? "#3a3a5a" : "#2a2a3a");
            await ctx.FillRectAsync(300, y, 400, 50);
            await ctx.SetStrokeStyleAsync(focused ? "#66aaff" : "#555555");
            await ctx.SetLineWidthAsync(focused ? 3 : 1);
            await ctx.StrokeRectAsync(300, y, 400, 50);

            await ctx.SetFillStyleAsync("white");
            await ctx.SetFontAsync("24px monospace");
            await ctx.FillTextAsync(values[i] + (focused ? "_" : string.Empty), 312, y + 33);
        }

        if (menu.Error != null)
        {
            await ctx.SetFillStyleAsync("#ff5555");
            await ctx.SetFontAsync("18px sans-serif");
            await ctx.FillTextAsync(menu.Error, 300, 630);
        }

        await ctx.SetFillStyleAsync("#aaaaaa");
        await ctx.SetFontAsync("16px sans-serif");
        await ctx.FillTextAsync("Tab: next field    Enter: connect", 300, 680);
    }

    private async Task RenderWorldAsync(Canvas2DContext ctx)
    {
        GameState s = renderState;
        double zoom = s.Zoom;
        if (FindHead(s) is Vector2D h)
        {
            lastHead = h;
        }

        Vector2D head = lastHead;

        await ctx.SetFillStyleAsync("lightblue");
        await ctx.FillRectAsync(0, 0, CanvasSize, CanvasSize);

        // The playable world, centered on (0, 0).
        var (wx, wy) = WorldToScreen(new Vector2D { X = -s.BoardSize / 2, Y = -s.BoardSize / 2 }, head, zoom, CanvasSize);
        await ctx.SetFillStyleAsync("#6fa84f");
        await ctx.FillRectAsync(wx, wy, s.BoardSize * zoom, s.BoardSize * zoom);
        if (GrassWorld is ElementReference grass)
        {
            // One draw of the visible part of the pre-rendered world.
            double x0 = Math.Max(0, -wx / zoom), x1 = Math.Min(s.BoardSize, (CanvasSize - wx) / zoom);
            double y0 = Math.Max(0, -wy / zoom), y1 = Math.Min(s.BoardSize, (CanvasSize - wy) / zoom);
            if (x1 > x0 && y1 > y0)
            {
                double gs = GrassScale(s.BoardSize);
                await ctx.DrawImageAsync(grass, x0 * gs, y0 * gs, (x1 - x0) * gs, (y1 - y0) * gs, wx + x0 * zoom, wy + y0 * zoom, (x1 - x0) * zoom, (y1 - y0) * zoom);
            }
        }

        // Walls: the segment p1..p2 expanded by half the wall width on every side.
        await ctx.SetFillStyleAsync("#555555");
        foreach (Wall wall in s.Walls.Values)
        {
            int minX = Math.Min(wall.Point1.X, wall.Point2.X), maxX = Math.Max(wall.Point1.X, wall.Point2.X);
            int minY = Math.Min(wall.Point1.Y, wall.Point2.Y), maxY = Math.Max(wall.Point1.Y, wall.Point2.Y);
            var (x, y) = WorldToScreen(new Vector2D { X = (int)(minX - WallWidth / 2), Y = (int)(minY - WallWidth / 2) }, head, zoom, CanvasSize);
            await ctx.FillRectAsync(x, y, (maxX - minX + WallWidth) * zoom, (maxY - minY + WallWidth) * zoom);
        }

        DateTime now = DateTime.UtcNow;
        staleIds.Clear();
        foreach (int k in seenFoods.Keys)
        {
            if (!s.Foods.ContainsKey(k))
            {
                staleIds.Add(k);
            }
        }

        foreach (int id in staleIds)
        {
            bursts.Add((seenFoods[id], now));
            seenFoods.Remove(id);
        }

        foreach (Food f in s.Foods.Values)
        {
            seenFoods[f.ID] = f.Location;
        }

        bursts.RemoveAll(b => (now - b.Start).TotalMilliseconds > BurstMs);

        if (AppleImage is ElementReference apple)
        {
            await ctx.SetShadowColorAsync("rgba(0,0,0,0.4)");
            await ctx.SetShadowBlurAsync((float)(4 * zoom));
            await ctx.SetShadowOffsetXAsync((float)(2 * zoom));
            await ctx.SetShadowOffsetYAsync((float)(2 * zoom));
            foreach (Food food in s.Foods.Values.Where(f => !f.Died))
            {
                // Idle: each apple pulses and bobs, out of phase with the others.
                double phase = Environment.TickCount64 / 300.0 + food.ID;
                double ah = FoodSize * zoom * (1 + 0.08 * Math.Sin(phase)), aw = ah * AppleAspect;
                var (x, y) = WorldToScreen(food.Location, head, zoom, CanvasSize);
                await ctx.DrawImageAsync(apple, x - aw / 2, y - ah / 2 + Math.Sin(phase) * 1.5 * zoom, aw, ah);
            }

            await ctx.SetShadowColorAsync("transparent");
            await ctx.SetShadowBlurAsync(0);
            await ctx.SetShadowOffsetXAsync(0);
            await ctx.SetShadowOffsetYAsync(0);
        }

        // Eaten: an expanding ring plus six particles, fading out.
        foreach (var (loc, start) in bursts)
        {
            double p = (now - start).TotalMilliseconds / BurstMs;
            var (bx, by) = WorldToScreen(loc, head, zoom, CanvasSize);
            await ctx.SetGlobalAlphaAsync((float)(1 - p));
            await ctx.SetStrokeStyleAsync("#ff4444");
            await ctx.SetLineWidthAsync((float)(2 * zoom));
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(bx, by, (6 + 18 * p) * zoom, 0, 2 * Math.PI);
            await ctx.StrokeAsync();
            await ctx.SetFillStyleAsync("#ff6644");
            for (int k = 0; k < 6; k++)
            {
                double ang = k * Math.PI / 3;
                await ctx.BeginPathAsync();
                await ctx.ArcAsync(bx + Math.Cos(ang) * 22 * p * zoom, by + Math.Sin(ang) * 22 * p * zoom, 2.5 * zoom * (1 - p), 0, 2 * Math.PI);
                await ctx.FillAsync();
            }
        }

        await ctx.SetGlobalAlphaAsync(1);

        staleIds.Clear();
        foreach (int k in deathTimes.Keys)
        {
            if (!s.Snakes.ContainsKey(k))
            {
                staleIds.Add(k);
            }
        }

        foreach (int id in staleIds)
        {
            deathTimes.Remove(id);
        }

        await ctx.SetLineCapAsync(LineCap.Round);
        await ctx.SetLineJoinAsync(LineJoin.Round);
        foreach (Snake snake in s.Snakes.Values)
        {
            if (snake.Alive)
            {
                deathTimes.Remove(snake.ID);
            }
            else
            {
                deathTimes.TryAdd(snake.ID, DateTime.UtcNow);
            }

            double alpha = snake.Alive ? 1 : FadeAlpha((DateTime.UtcNow - deathTimes[snake.ID]).TotalMilliseconds);
            if (snake.Body.Count < 2 || alpha <= 0)
            {
                continue;
            }

            await ctx.SetGlobalAlphaAsync((float)alpha);
            await ctx.BeginPathAsync();
            var (sx, sy) = WorldToScreen(snake.Body[0], head, zoom, CanvasSize);
            await ctx.MoveToAsync(sx, sy);
            for (int i = 1; i < snake.Body.Count; i++)
            {
                var (px, py) = WorldToScreen(snake.Body[i], head, zoom, CanvasSize);
                await ctx.LineToAsync(px, py);
            }

            // Dark wider line with a shadow underneath = outline; colored line on top.
            await ctx.SetShadowColorAsync("rgba(0,0,0,0.4)");
            await ctx.SetShadowBlurAsync((float)(6 * zoom));
            await ctx.SetShadowOffsetXAsync((float)(2 * zoom));
            await ctx.SetShadowOffsetYAsync((float)(2 * zoom));
            await ctx.SetStrokeStyleAsync("#222222");
            await ctx.SetLineWidthAsync((float)((SnakeWidth + 3) * zoom));
            await ctx.StrokeAsync();

            await ctx.SetShadowColorAsync("transparent");
            await ctx.SetShadowBlurAsync(0);
            await ctx.SetShadowOffsetXAsync(0);
            await ctx.SetShadowOffsetYAsync(0);
            await ctx.SetStrokeStyleAsync(snake.Alive ? SnakeColors[((snake.ID % 5) + 5) % 5] : "red");
            await ctx.SetLineWidthAsync((float)(SnakeWidth * zoom));
            await ctx.StrokeAsync();

            // Googly eyes: pupils wobble independently.
            var (hx, hy) = WorldToScreen(snake.Body[^1], head, zoom, CanvasSize);
            var (left, right) = EyeOffsets(snake.Direction, 3);
            double t = Environment.TickCount64 / 150.0;
            int n = 0;
            foreach (var (ox, oy) in new[] { left, right })
            {
                double ex = hx + ox * zoom, ey = hy + oy * zoom;
                if (!snake.Alive)
                {
                    // Dead: an X instead of a googly eye.
                    double r = 3 * zoom;
                    await ctx.SetStrokeStyleAsync("black");
                    await ctx.SetLineWidthAsync((float)(1.5 * zoom));
                    await ctx.BeginPathAsync();
                    await ctx.MoveToAsync(ex - r, ey - r);
                    await ctx.LineToAsync(ex + r, ey + r);
                    await ctx.MoveToAsync(ex + r, ey - r);
                    await ctx.LineToAsync(ex - r, ey + r);
                    await ctx.StrokeAsync();
                    continue;
                }

                await ctx.SetFillStyleAsync("white");
                await ctx.BeginPathAsync();
                await ctx.ArcAsync(ex, ey, 4 * zoom, 0, 2 * Math.PI);
                await ctx.FillAsync();
                await ctx.SetFillStyleAsync("black");
                await ctx.BeginPathAsync();
                await ctx.ArcAsync(ex + Math.Sin(t + n * 2) * 1.5 * zoom, ey + Math.Cos(t * 1.3 + n * 2) * 1.5 * zoom, 2 * zoom, 0, 2 * Math.PI);
                await ctx.FillAsync();
                n++;
            }

            if (snake.ID != s.SnakeID)
            {
                await ctx.SetTextAlignAsync(TextAlign.Center);
                await ctx.SetFontAsync("bold 14px sans-serif");
                await ctx.SetStrokeStyleAsync("black");
                await ctx.SetLineWidthAsync(3);
                await ctx.StrokeTextAsync(snake.Name, hx, hy - 14 * zoom);
                await ctx.SetFillStyleAsync("white");
                await ctx.FillTextAsync(snake.Name, hx, hy - 14 * zoom);
            }
        }

        await ctx.SetGlobalAlphaAsync(1);

        // White arrows at the screen edge toward living snakes that are off screen.
        await ctx.SetFillStyleAsync("white");
        foreach (Snake other in s.Snakes.Values.Where(o => o.ID != s.SnakeID && o.Alive && o.Body.Count > 0))
        {
            if (ArrowFor(other.Body[^1], head, zoom, CanvasSize) is not var (ax, ay, ang, size))
            {
                continue;
            }

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(ax + Math.Cos(ang) * size, ay + Math.Sin(ang) * size);
            await ctx.LineToAsync(ax + Math.Cos(ang + 2.5) * size * 0.6, ay + Math.Sin(ang + 2.5) * size * 0.6);
            await ctx.LineToAsync(ax + Math.Cos(ang - 2.5) * size * 0.6, ay + Math.Sin(ang - 2.5) * size * 0.6);
            await ctx.ClosePathAsync();
            await ctx.FillAsync();
        }

        // Scoreboard, top right: highest scores first.
        scoreRows.Clear();
        foreach (Snake o in s.Snakes.Values)
        {
            if (!o.Disconnected)
            {
                scoreRows.Add(o);
            }
        }

        // Ties break by id so equal scores do not swap places from frame to frame.
        scoreRows.Sort((a, b) => a.Score != b.Score ? b.Score.CompareTo(a.Score) : a.ID.CompareTo(b.ID));
        var rows = scoreRows;
        int rowCount = Math.Min(rows.Count, MaxScoreRows);
        double bx0 = CanvasSize - 210;
        await ctx.SetFillStyleAsync("rgba(0,0,0,0.5)");
        await ctx.FillRectAsync(bx0, 10, 200, 28 + rowCount * 22);
        await ctx.SetFontAsync("bold 16px sans-serif");
        await ctx.SetFillStyleAsync("white");
        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.FillTextAsync("Scores", bx0 + 10, 30);
        await ctx.SetFontAsync("14px sans-serif");
        for (int i = 0; i < rowCount; i++)
        {
            await ctx.SetFillStyleAsync(rows[i].ID == s.SnakeID ? "#ffe066" : "white");
            await ctx.SetTextAlignAsync(TextAlign.Left);
            await ctx.FillTextAsync(rows[i].Name, bx0 + 10, 52 + i * 22);
            await ctx.SetTextAlignAsync(TextAlign.Right);
            await ctx.FillTextAsync(rows[i].Score.ToString(), bx0 + 190, 52 + i * 22);
        }

        await ctx.SetTextAlignAsync(TextAlign.Left);
    }
}
