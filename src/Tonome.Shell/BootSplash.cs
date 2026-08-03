using SkiaSharp;

namespace Tonome.Shell;

/// <summary>Fullscreen splash shown for the first moments of the session, then fades into the desktop.</summary>
public class BootSplash
{
    private readonly DateTime _start = DateTime.Now;
    public double DurationSeconds { get; set; } = 3.0;

    public bool IsFinished => (DateTime.Now - _start).TotalSeconds >= DurationSeconds;

    public void Render(SKCanvas canvas, int width, int height)
    {
        var t = (DateTime.Now - _start).TotalSeconds;
        if (t >= DurationSeconds) return;

        var progress = (float)(t / DurationSeconds);

        float alpha;
        if (progress < 0.18f) alpha = progress / 0.18f;
        else if (progress > 0.82f) alpha = Math.Clamp((1f - progress) / 0.18f, 0f, 1f);
        else alpha = 1f;

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(8, 8, 18, (byte)(245 * alpha))
        };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        var cx = width / 2f;
        var cy = height / 2f;

        var pop = 1f + MathF.Sin(MathF.Min(progress * 2.2f, 1f) * MathF.PI) * 0.12f;

        canvas.Save();
        canvas.Translate(cx, cy);
        canvas.Scale(pop, pop);

        using var logoFont = new SKFont(
            SKTypeface.FromFamilyName("DejaVu Sans", SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 76);
        using var logoPaint = new SKPaint
        {
            Color = new SKColor(120, 222, 255, (byte)(255 * alpha)),
            IsAntialias = true
        };
        const string logo = "to[no]ME!";
        canvas.DrawText(logo, -logoFont.MeasureText(logo) / 2f, 0, SKTextAlign.Left, logoFont, logoPaint);

        using var subFont = new SKFont(null, 22);
        using var subPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, (byte)(200 * alpha)),
            IsAntialias = true
        };
        const string sub = "TONOME DESKTOP";
        canvas.DrawText(sub, -subFont.MeasureText(sub) / 2f, 52, SKTextAlign.Left, subFont, subPaint);

        canvas.Restore();

        using var ringPaint = new SKPaint
        {
            Color = new SKColor(120, 222, 255, (byte)(230 * alpha)),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        var ringRect = new SKRect(cx - 26, cy + 96, cx + 26, cy + 148);
        canvas.DrawArc(ringRect, (float)(t * 200 % 360f), 265, false, ringPaint);

        using var barBgPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 26)
        };
        var barRect = new SKRect(cx - 130, cy + 162, cx + 130, cy + 164);
        canvas.DrawRoundRect(barRect, 1, 1, barBgPaint);

        using var barFillPaint = new SKPaint
        {
            Color = new SKColor(120, 222, 255, (byte)(220 * alpha))
        };
        var fillW = 260f * Math.Clamp(progress, 0f, 1f);
        if (fillW > 0)
        {
            var fillRect = new SKRect(cx - 130, cy + 162, cx - 130 + fillW, cy + 164);
            canvas.DrawRoundRect(fillRect, 1, 1, barFillPaint);
        }
    }
}
