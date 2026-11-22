window.initRenderJS = (instance) => {
    window.theInstance = instance;
};

// Called when the page is disposed so key handling stops affecting other pages.
window.disposeRenderJS = () => {
    window.theInstance = null;
};

// Keys the browser would otherwise use to scroll the page or move focus away from the canvas.
const swallowed = ["Tab", "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", " "];

document.addEventListener('keydown', function (event)
{
    // A held key repeats; the server already has the first press.
    if (event.repeat) {
        return;
    }

    // Only act while the snake page is active, and leave browser shortcuts (Ctrl+R, Ctrl+W...) alone.
    if (!window.theInstance || event.ctrlKey || event.metaKey || event.altKey) {
        return;
    }

    if (swallowed.includes(event.key)) {
        event.preventDefault();
    }

    window.theInstance.invokeMethodAsync('HandleKeyPress', event.key).catch(() => {});
});

// Bakes the grass image into a canvas whose four edges fade to transparent, so overlapping rotated tiles blend.
export async function buildGrassTile(canvas, img, feather) {
    await img.decode();
    const w = canvas.width, h = canvas.height, ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, w, h);
    ctx.globalCompositeOperation = 'source-over';
    ctx.drawImage(img, 0, 0, w, h);
    ctx.globalCompositeOperation = 'destination-in';
    for (const [x1, y1, x2, y2] of [[0, 0, w, 0], [0, 0, 0, h]]) {
        const g = ctx.createLinearGradient(x1, y1, x2, y2), f = feather;
        g.addColorStop(0, 'rgba(0,0,0,0)');
        g.addColorStop(f, 'rgba(0,0,0,1)');
        g.addColorStop(1 - f, 'rgba(0,0,0,1)');
        g.addColorStop(1, 'rgba(0,0,0,0)');
        ctx.fillStyle = g;
        ctx.fillRect(0, 0, w, h);
    }
    ctx.globalCompositeOperation = 'source-over';
}

// Draws the whole world's grass once (randomly rotated tiles, one pixel per world unit, fewer for huge worlds) so each frame is a single drawImage.
// scale < 1 shrinks very large worlds so the canvas stays small; the caller also passes bigger tiles so the tile count stays low.
export function buildGrassWorld(world, tile, size, tileSize, feather, scale) {
    world.width = Math.ceil(size * scale);
    world.height = Math.ceil(size * scale);
    const ctx = world.getContext('2d');
    ctx.scale(scale, scale);
    const ds = tileSize * (1 + 2 * feather);
    for (let i = 0; i * tileSize < size; i++) {
        for (let j = 0; j * tileSize < size; j++) {
            ctx.save();
            ctx.translate((i + 0.5) * tileSize, (j + 0.5) * tileSize);
            ctx.rotate(Math.random() * 2 * Math.PI);
            ctx.drawImage(tile, -ds / 2, -ds / 2, ds, ds);
            ctx.restore();
        }
    }
}
