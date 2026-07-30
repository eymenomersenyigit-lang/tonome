using SkiaSharp;
using Tonome.Framework.Rendering;

namespace Tonome.Shell;

public class LiveWallpaperEngine
{
    private readonly TonomeRenderer _renderer;
    private SKBitmap? _backgroundImage;
    private float _time;

    // Shader-like gradient colors
    private readonly SKColor[] _palette1 = {
        new(10, 5, 20),
        new(20, 10, 40),
        new(40, 15, 60),
        new(60, 20, 50),
    };

    private readonly SKColor[] _palette2 = {
        new(5, 10, 30),
        new(15, 30, 50),
        new(30, 50, 70),
        new(20, 40, 60),
    };

    private bool _usePalette2;

    public LiveWallpaperEngine(TonomeRenderer renderer)
    {
        _renderer = renderer;
    }

    public void SetBackground(string imagePath)
    {
        try
        {
            _backgroundImage = SKBitmap.Decode(imagePath);
        }
        catch
        {
            _backgroundImage = null;
        }
    }

    public void Update(float delta)
    {
        _time += delta * 0.015f;
        if (_time > MathF.PI * 2) _time -= MathF.PI * 2;
    }

    public void Render(SKCanvas canvas, int width, int height)
    {
        if (_backgroundImage != null)
        {
            using var bitmapPaint = new SKPaint { IsAntialias = true };
        canvas.DrawBitmap(_backgroundImage,
                new SKRect(0, 0, width, height), bitmapPaint);
            return;
        }

        var colors = _usePalette2 ? _palette2 : _palette1;

        using var paint = new SKPaint { IsAntialias = true };

        var waveCount = 4;
        for (var w = 0; w < waveCount; w++)
        {
            var waveSpeed = 0.3f + w * 0.1f;
            var waveAmp = 20 + w * 8;
            var wavePhase = _time * waveSpeed + w * 1.2f;

            using var path = new SKPath();
            path.MoveTo(0, height);

            for (var x = 0; x <= width; x += 2)
            {
                var y = height / 2f
                    + MathF.Sin(x * 0.003f + wavePhase) * waveAmp
                    + MathF.Sin(x * 0.007f + wavePhase * 0.7f) * waveAmp * 0.5f
                    + MathF.Sin(x * 0.001f + _time * 0.2f) * 30;
                path.LineTo(x, y);
            }

            path.LineTo(width, height);
            path.Close();

            var c = colors[w % colors.Length];
            paint.Color = new SKColor(c.Red, c.Green, c.Blue, (byte)(30 + w * 15));
            canvas.DrawPath(path, paint);
        }

        for (var i = 0; i < 40; i++)
        {
            var px = (MathF.Sin(_time * 0.1f + i * 1.7f) * 0.5f + 0.5f) * width;
            var py = (MathF.Sin(_time * 0.08f + i * 2.3f + MathF.Sin(_time * 0.03f + i)) * 0.5f + 0.5f) * height;
            var size = 1 + MathF.Sin(_time * 0.5f + i) * 0.5f + 0.5f;
            var starBright = (byte)(100 + MathF.Sin(_time + i * 1.1f) * 50 + 50);

            paint.Color = new SKColor(255, 255, 255, starBright);
            canvas.DrawCircle(px, py, size, paint);
        }

        _usePalette2 = MathF.Sin(_time * 0.02f) > 0;
    }
}
