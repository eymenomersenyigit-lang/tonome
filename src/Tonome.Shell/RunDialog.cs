using SkiaSharp;
using Tonome.Framework.Controls;
using Tonome.Framework.Types;
using Tonome.Framework.Animation;

namespace Tonome.Shell;

public class RunDialog : Panel
{
    public int ScreenWidth { get; set; } = 1920;
    public int ScreenHeight { get; set; } = 1080;
    public bool IsOpen { get; private set; }
    public string Command { get; set; } = "";

    private readonly SpringAnimation _openAnim = new(0, 0);
    private readonly List<string> _history = new();
    private int _historyIndex = -1;

    public RunDialog()
    {
        Visible = false;
        Width = 520;
        Height = 200;
        CornerRadius = 18;
    }

    public void Open()
    {
        IsOpen = true;
        Visible = true;
        _openAnim.Target = 1;
        Command = string.Empty;
        X = (ScreenWidth - Width) / 2;
        Y = ScreenHeight / 3;
    }

    public void Close()
    {
        IsOpen = false;
        _openAnim.Target = 0;
        Visible = false;
    }

    public void Execute()
    {
        if (string.IsNullOrWhiteSpace(Command)) return;

        _history.Insert(0, Command);
        _historyIndex = -1;

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = Command,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch
        {
            // Command failed - handled gracefully
        }

        Close();
    }

    public void HistoryUp()
    {
        if (_history.Count == 0) return;
        _historyIndex = Math.Min(_historyIndex + 1, _history.Count - 1);
        Command = _history[_historyIndex];
    }

    public void HistoryDown()
    {
        if (_historyIndex <= 0)
        {
            _historyIndex = -1;
            Command = "";
        }
        else
        {
            _historyIndex--;
            Command = _history[_historyIndex];
        }
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        _openAnim.Update(delta);
        if (!IsOpen && _openAnim.IsCompleted) return;

        var alpha = (byte)(160 * _openAnim.Value);

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, alpha)
        };
        canvas.DrawRect(0, 0, ScreenWidth, ScreenHeight, bgPaint);

        canvas.Save();
        canvas.Scale(_openAnim.Value, _openAnim.Value, X + Width / 2f, Y + Height / 2f);

        var rect = new SKRect(X, Y, X + Width, Y + Height);
        using var glassPaint = new SKPaint
        {
            Color = new SKColor(25, 25, 38, 240),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10),
            IsAntialias = true
        };
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, glassPaint);

        using var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 30),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, borderPaint);

        using var titleFont = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold,
                SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 16);
        using var titlePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };
        canvas.DrawText("Run Command", X + 20, Y + 32, SKTextAlign.Left, titleFont, titlePaint);

        using var inputBg = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 18),
            IsAntialias = true
        };
        var inputRect = new SKRect(X + 20, Y + 48, X + Width - 20, Y + 86);
        canvas.DrawRoundRect(inputRect, 10, 10, inputBg);

        using var cmdFont = new SKFont(null, 15);
        using var cmdPaint = new SKPaint
        {
            Color = string.IsNullOrEmpty(Command)
                ? new SKColor(255, 255, 255, 80)
                : new SKColor(255, 255, 255, 220),
            IsAntialias = true
        };
        var displayText = string.IsNullOrEmpty(Command) ? "Type a command or application name..." : Command;
        canvas.DrawText(displayText, X + 32, Y + 73, SKTextAlign.Left, cmdFont, cmdPaint);

        using var tipsFont = new SKFont(null, 11);
        using var tipsPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 220, 120),
            IsAntialias = true
        };
        canvas.DrawText("Enter: Run   ↑↓: History   Esc: Close", X + 20, Y + 118, SKTextAlign.Left, tipsFont, tipsPaint);

        using var runBtnFont = new SKFont(null, 12);
        using var runBtnPaint = new SKPaint
        {
            Color = new SKColor(0, 120, 212, 200),
            IsAntialias = true
        };
        canvas.DrawText("Run", X + Width - 60, Y + 118, SKTextAlign.Left, runBtnFont, runBtnPaint);

        canvas.Restore();
    }
}
