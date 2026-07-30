using Tonome.Framework;
using Tonome.Framework.Controls;
using Tonome.Framework.Theming;
using Tonome.Framework.Input;
using Tonome.Compositor;
using Tonome.Compositor.Workspace;

Console.WriteLine(@"  ╔══════════════════════════════════════════════╗
  ║        to[no]ME! Session v0.1              ║
  ║     Tonome Compositor + Shell Starting...   ║
  ╚══════════════════════════════════════════════╝");

var app = new TonomeApplication(1920, 1080, "to[no]ME! Desktop");
var theme = new ThemeManager();
var shortcuts = new ShortcutManager();
shortcuts.RegisterDefaultShortcuts();

var compositor = new TonomeCompositor(app);
var workspaceManager = new WorkspaceManager(4);

var desktop = new Panel
{
    Width = 1920,
    Height = 1080,
    BackgroundColor = new Tonome.Framework.Types.Color(20, 20, 30)
};

var dash = new Dash { ScreenWidth = 1920 };
dash.AddAppIcon("Terminal");
dash.AddAppIcon("Files");
dash.AddAppIcon("Browser");
dash.AddAppIcon("Settings");
dash.AddAppIcon("Store");

var infoWindow = new Window
{
    X = 100,
    Y = 80,
    Width = 420,
    Height = 280,
    CornerRadius = 16,
    Title = "to[no]ME! Desktop",
    GlassEnabled = true
};

var welcomeLabel = new Label
{
    Text = "Welcome to to[no]ME!",
    TextSize = 20,
    Bold = true,
    X = 20, Y = 50,
    Width = 380, Height = 28,
    Center = true
};

var infoLabel = new Label
{
    Text = "Tonome Compositor is running.\n\n" +
           "• Super+R: Run dialog\n" +
           "• Alt+Tab: 3D Window Switcher\n" +
           "• Super+Tab: Desktop Switcher\n" +
           "• Super+D: Show Desktop\n" +
           "• Super+Q: Close Window\n" +
           "• Super+Space: App Launcher",
    TextSize = 13,
    X = 20, Y = 85,
    Width = 380, Height = 160
};

var closeBtn = new Button
{
    Text = "Close",
    X = 310, Y = 230,
    Width = 90, Height = 36,
    CornerRadius = 10,
    BackgroundColor = new Tonome.Framework.Types.Color(180, 40, 40)
};
closeBtn.OnClick += () => app.Window.Close();

infoWindow.AddChild(welcomeLabel);
infoWindow.AddChild(infoLabel);
infoWindow.AddChild(closeBtn);

var compositorWindowId = compositor.GetType().GetHashCode();
desktop.AddChild(infoWindow);
desktop.AddChild(dash);

app.Renderer!.OnRender = (canvas, delta, w, h) =>
{
    desktop.Render(canvas, delta);
};

app.OnStarted += () =>
{
    compositor.Start();
    Console.WriteLine($"Tonome Compositor started on {compositor.GetType().Name}");
};

app.OnFrameUpdate += (delta) =>
{
    workspaceManager.SwitchAnimation.Update(delta);
};

app.OnShutdown += () =>
{
    compositor.Stop();
    Console.WriteLine("Tonome Session ended.");
};

shortcuts.OnRunCommand += () => Console.WriteLine("[Super+R] Run dialog");
shortcuts.OnDesktopSwitch += () => Console.WriteLine("[Super+Tab] Desktop switcher");
shortcuts.OnWindowSwitch += () => Console.WriteLine("[Alt+Tab] Window switcher");
shortcuts.OnAppLauncher += () => Console.WriteLine("[Super+Space] App launcher");

app.Run();
