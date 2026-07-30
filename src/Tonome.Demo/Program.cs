using Tonome.Framework;
using Tonome.Framework.Controls;
using Tonome.Framework.Theming;
using Tonome.Framework.Input;

var app = new TonomeApplication(1280, 720, "Tonome Desktop Demo");
var theme = new ThemeManager();
var shortcuts = new ShortcutManager();
shortcuts.RegisterDefaultShortcuts();

var desktop = new Panel
{
    Width = 1280,
    Height = 720,
    BackgroundColor = new Tonome.Framework.Types.Color(20, 20, 30)
};

var dash = new Dash { ScreenWidth = 1280 };
dash.AddAppIcon("Terminal");
dash.AddAppIcon("Files");
dash.AddAppIcon("Browser");
dash.AddAppIcon("Settings");
dash.AddAppIcon("Store");

var demoWindow = new Window
{
    X = 100,
    Y = 80,
    Width = 500,
    Height = 350,
    CornerRadius = 16,
    Title = "Tonome Glass Window",
    GlassEnabled = true
};

var titleLabel = new Label
{
    Text = "Welcome to to[no]ME!",
    TextSize = 22,
    Bold = true,
    X = 20,
    Y = 50,
    Width = 460,
    Height = 30,
    Center = true,
    ForegroundColor = new Tonome.Framework.Types.Color(255, 255, 255)
};

var descLabel = new Label
{
    Text = "This is a native C# desktop environment.",
    TextSize = 14,
    X = 20,
    Y = 90,
    Width = 460,
    Height = 24,
    Center = true,
    ForegroundColor = new Tonome.Framework.Types.Color(200, 200, 220)
};

var glassToggle = new Button
{
    Text = "Toggle Glass",
    X = 60,
    Y = 140,
    Width = 160,
    Height = 38,
    CornerRadius = 10
};
glassToggle.OnClick += () => demoWindow.GlassEnabled = !demoWindow.GlassEnabled;

var closeBtn = new Button
{
    Text = "Close",
    X = 280,
    Y = 140,
    Width = 160,
    Height = 38,
    CornerRadius = 10,
    BackgroundColor = new Tonome.Framework.Types.Color(180, 40, 40)
};
closeBtn.OnClick += () => app.Window.Close();

var infoLabel = new Label
{
    Text = "• Anti-pixel SSAA rendering (2x scale)\n• Glass blur effects\n• Spring-based jelly animation ready\n• Rounded corners everywhere",
    TextSize = 13,
    X = 20,
    Y = 200,
    Width = 460,
    Height = 120,
    ForegroundColor = new Tonome.Framework.Types.Color(180, 180, 200)
};

demoWindow.AddChild(titleLabel);
demoWindow.AddChild(descLabel);
demoWindow.AddChild(glassToggle);
demoWindow.AddChild(closeBtn);
demoWindow.AddChild(infoLabel);

desktop.AddChild(demoWindow);
desktop.AddChild(dash);

app.Renderer!.OnRender = (canvas, delta, w, h) =>
{
    desktop.Render(canvas, delta);
};

app.OnStarted += () => Console.WriteLine("Tonome Desktop started.");
app.OnShutdown += () => Console.WriteLine("Tonome Desktop shutdown.");

shortcuts.OnRunCommand += () => Console.WriteLine("[Super+R] Run dialog requested");
shortcuts.OnDesktopSwitch += () => Console.WriteLine("[Super+Tab] Desktop switch requested");
shortcuts.OnWindowSwitch += () => Console.WriteLine("[Alt+Tab] Window switch requested");
shortcuts.OnAppLauncher += () => Console.WriteLine("[Super+Space] App launcher requested");

Console.WriteLine(@"  ╔══════════════════════════════════════╗
  ║      to[no]ME! Desktop Demo v0.1    ║
  ║  Tonome Framework loaded. Running... ║
  ╚══════════════════════════════════════╝");

app.Run();
