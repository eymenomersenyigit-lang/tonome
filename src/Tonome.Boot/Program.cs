using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var screenWidth = Console.WindowWidth;
var screenHeight = Console.WindowHeight;

var logo = new[]
{
    @"  ████████╗ ██████╗     ███╗   ██╗ ██████╗ ███╗   ███╗███████╗",
    @"  ╚══██╔══╝██╔═══██╗    ████╗  ██║██╔═══██╗████╗ ████║██╔════╝",
    @"     ██║   ██║   ██║    ██╔██╗ ██║██║   ██║██╔████╔██║█████╗  ",
    @"     ██║   ██║   ██║    ██║╚██╗██║██║   ██║██║╚██╔╝██║██╔══╝  ",
    @"     ██║   ╚██████╔╝    ██║ ╚████║╚██████╔╝██║ ╚═╝ ██║███████╗",
    @"     ╚═╝    ╚═════╝     ╚═╝  ╚═══╝ ╚═════╝ ╚═╝     ╚═╝╚══════╝"
};

var version = new[]
{
    @"  ╔══════════════════════════════════════╗",
    @"  ║       to[no]ME! v0.1 - Tonome       ║",
    @"  ╚══════════════════════════════════════╝"
};

var ringChars = new[] { '◜', '◝', '◞', '◟' };
var ringFrame = new[]
{
    @"         ╭─────────────────────────────────────╮",
    @"         │                                     │",
    @"         ╰─────────────────────────────────────╯"
};

void AutoScale()
{
    Console.Clear();
    screenWidth = Console.WindowWidth;
    screenHeight = Console.WindowHeight;
}

void DrawLogo(int frame)
{
    var topPadding = Math.Max(0, (screenHeight - 20) / 3);

    foreach (var line in logo)
    {
        var padding = Math.Max(0, (screenWidth - line.Length) / 2);
        Console.SetCursorPosition(padding, topPadding + Array.IndexOf(logo, line));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(line);
    }

    var verY = topPadding + logo.Length + 1;
    foreach (var line in version)
    {
        var padding = Math.Max(0, (screenWidth - line.Length) / 2);
        Console.SetCursorPosition(padding, verY + Array.IndexOf(version, line));
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(line);
    }
}

void DrawSpinner(int frame)
{
    var ringChar = ringChars[frame % ringChars.Length];
    var spinnerY = Math.Max(0, screenHeight - 5);

    var ringLine = $"               {ringChar}  LOADING  {ringChar}";
    var padding = Math.Max(0, (screenWidth - ringLine.Length) / 2);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.SetCursorPosition(padding, spinnerY);
    Console.Write(ringLine);

    var barWidth = Math.Min(40, screenWidth - 20);
    var progress = (frame % 100) / 100.0;
    var filled = (int)(barWidth * progress);
    var barPadding = Math.Max(0, (screenWidth - barWidth - 4) / 2);

    Console.SetCursorPosition(barPadding, spinnerY + 1);
    Console.Write("[");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(new string('█', filled));
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write(new string('░', barWidth - filled));
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("]");
}

Console.CursorVisible = false;
Console.BackgroundColor = ConsoleColor.Black;
Console.Clear();

var sw = Stopwatch.StartNew();
var frame = 0;

while (sw.Elapsed.TotalSeconds < 5)
{
    AutoScale();
    DrawLogo(frame);
    DrawSpinner(frame);

    frame++;
    Thread.Sleep(100);

    if (Console.KeyAvailable)
    {
        Console.ReadKey(true);
        break;
    }
}

Console.ForegroundColor = ConsoleColor.Green;
var bootPadding = Math.Max(0, (screenWidth - 30) / 2);
Console.SetCursorPosition(bootPadding, screenHeight - 2);
Console.Write("Boot complete. Starting Tonome Desktop...");
Thread.Sleep(1500);

Console.ResetColor();
Console.Clear();
Console.CursorVisible = true;
