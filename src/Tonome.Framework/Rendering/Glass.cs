using SkiaSharp;

namespace Tonome.Framework.Rendering;

/// <summary>
/// Real backdrop-blur glass: captures the frame once, then each panel samples the
/// pixels behind itself, blurs them (GPU), tints, and adds a sheen + rounded corners.
/// Falls back to a plain translucent fill if the backdrop capture is unavailable.
/// </summary>
public static class Glass
{
    public static bool Enabled { get; set; } = true;

    private static SKImage? _backdrop;
    private static SKMatrix _matrix = SKMatrix.Identity;

    /// <summary>Call once per frame AFTER drawing the wallpaper, before the UI layers.</summary>
    public static void BeginFrame(SKSurface surface, SKCanvas canvas)
    {
        _backdrop?.Dispose();
        _backdrop = null;
        try
        {
            _matrix = canvas.TotalMatrix;
            _backdrop = surface.Snapshot();
        }
        catch
        {
            _backdrop = null;
        }
    }

    public static void EndFrame()
    {
        _backdrop?.Dispose();
        _backdrop = null;
    }

    /// <summary>Draws a glass rounded rectangle at the given (canvas-space) rect.</summary>
    public static void Draw(SKCanvas canvas, SKRect rect, float cornerRadius, SKColor tint, byte alpha,
        float blurSigma = 12f, byte borderAlpha = 50, byte sheenAlpha = 18)
    {
        if (cornerRadius <= 0)
        {
            var path = new SKPath();
            path.AddRect(rect);
            DrawInner(canvas, rect, path, tint, alpha, blurSigma, borderAlpha, sheenAlpha);
            return;
        }

        var round = new SKPath();
        round.AddRoundRect(rect, cornerRadius, cornerRadius);
        DrawInner(canvas, rect, round, tint, alpha, blurSigma, borderAlpha, sheenAlpha);
    }

    /// <summary>Draws a glass bar flush to the top with rounded bottom corners only.</summary>
    public static void DrawPanel(SKCanvas canvas, SKRect rect, float cornerRadius, SKColor tint, byte alpha,
        float blurSigma = 12f, byte borderAlpha = 50, byte sheenAlpha = 18)
    {
        var path = new SKPath();
        var r = Math.Min(cornerRadius, rect.Height / 2f);
        path.MoveTo(rect.Left, rect.Bottom);
        path.LineTo(rect.Left, rect.Top + r);
        path.QuadTo(rect.Left, rect.Top, rect.Left + r, rect.Top);
        path.LineTo(rect.Right - r, rect.Top);
        path.QuadTo(rect.Right, rect.Top, rect.Right, rect.Top + r);
        path.LineTo(rect.Right, rect.Bottom);
        path.Close();
        DrawInner(canvas, rect, path, tint, alpha, blurSigma, borderAlpha, sheenAlpha);
    }

    private static void DrawInner(SKCanvas canvas, SKRect rect, SKPath clip, SKColor tint, byte alpha,
        float blurSigma, byte borderAlpha, byte sheenAlpha)
    {
        if (blurSigma <= 0)
        {
            DrawFallback(canvas, rect, clip, tint, alpha, borderAlpha);
            return;
        }

        canvas.Save();
        canvas.ClipPath(clip, SKClipOperation.Intersect, true);

        if (Enabled && _backdrop is not null)
        {
            try
            {
                var mapped = MapRect(_matrix, rect);
                var left = Math.Clamp((int)MathF.Round(mapped.Left), 0, _backdrop.Width);
                var top = Math.Clamp((int)MathF.Round(mapped.Top), 0, _backdrop.Height);
                var right = Math.Clamp((int)MathF.Round(mapped.Right), 0, _backdrop.Width);
                var bottom = Math.Clamp((int)MathF.Round(mapped.Bottom), 0, _backdrop.Height);
                var subsetRect = new SKRectI(left, top, right, bottom);
                if (subsetRect.Width >= 1 && subsetRect.Height >= 1)
                {
                    var subset = _backdrop.Subset(subsetRect);
                    if (subset is not null)
                    {
                        using var blurPaint = new SKPaint
                        {
                            ImageFilter = SKImageFilter.CreateBlur(blurSigma, blurSigma),
                            IsAntialias = true
                        };
                        canvas.ResetMatrix();
                        canvas.DrawImage(subset, subsetRect.Left, subsetRect.Top, blurPaint);
                        canvas.SetMatrix(_matrix);
                        subset.Dispose();
                    }
                }
            }
            catch
            {
                // fall through to tint-only glass
            }
        }

        using (var tintPaint = new SKPaint
        {
            Color = new SKColor(tint.Red, tint.Green, tint.Blue, alpha),
            IsAntialias = true
        })
        {
            canvas.DrawPath(clip, tintPaint);
        }

        if (sheenAlpha > 0)
        {
            var sheen = new SKRect(rect.Left + 3, rect.Top + 1, rect.Right - 3, rect.Top + rect.Height * 0.30f);
            using var sheenPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, sheenAlpha),
                IsAntialias = true
            };
            canvas.DrawRoundRect(sheen, 2, 2, sheenPaint);
        }

        if (borderAlpha > 0)
        {
            using var borderPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, borderAlpha),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            canvas.DrawPath(clip, borderPaint);
        }

        canvas.Restore();
    }

    private static void DrawFallback(SKCanvas canvas, SKRect rect, SKPath clip, SKColor tint, byte alpha, byte borderAlpha)
    {
        canvas.Save();
        canvas.ClipPath(clip, SKClipOperation.Intersect, true);

        using (var tintPaint = new SKPaint
        {
            Color = new SKColor(tint.Red, tint.Green, tint.Blue, alpha),
            IsAntialias = true
        })
        {
            canvas.DrawPath(clip, tintPaint);
        }

        using (var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, borderAlpha),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            canvas.DrawPath(clip, borderPaint);
        }

        canvas.Restore();
    }

    private static SKRect MapRect(SKMatrix m, SKRect r)
    {
        // We only ever apply uniform scale + translation in this codebase.
        var sx = m.ScaleX;
        var tx = m.TransX;
        var ty = m.TransY;
        return new SKRect(
            r.Left * sx + tx,
            r.Top * sx + ty,
            r.Right * sx + tx,
            r.Bottom * sx + ty);
    }
}
