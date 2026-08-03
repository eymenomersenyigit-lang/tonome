using Tonome.Framework;
using Tonome.Framework.Controls;
using Tonome.Framework.Rendering;
using Tonome.Framework.Theming;
using Tonome.Framework.Input;
using Tonome.Compositor;
using Tonome.Compositor.Workspace;
using Tonome.Shell;
using Tonome.Shell.Panels;
using Tonome.Shell.Dash;
using Tonome.Shell.Launcher;
using Tonome.Shell.Notifications;

var logPath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/tmp", "tonome-session.log");

void Log(string message)
{
    Console.WriteLine($"[tonome] {message}");
    try
    {
        File.AppendAllText(logPath, $"[{DateTime.Now:O}] {message}{Environment.NewLine}");
    }
    catch
    {
    }
}

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Log("Unhandled exception:");
    Log(e.ExceptionObject?.ToString() ?? "unknown");
};

Log("Session starting...");
Log($"Version: {Environment.Version} | OS: {Environment.OSVersion} | Display: {Environment.GetEnvironmentVariable("DISPLAY")} | Wayland: {Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")}");

try
{
    Console.WriteLine(@"  ╔══════════════════════════════════════════════╗
  ║        to[no]ME! Session v0.3              ║
  ║     Tonome Desktop Environment Ready        ║
  ╚══════════════════════════════════════════════╝");

    var app = new TonomeApplication(1920, 1080, "to[no]ME! Desktop");
    var theme = new ThemeManager();
    var shortcuts = new ShortcutManager();
    shortcuts.RegisterDefaultShortcuts();

    var compositor = new TonomeCompositor(app);
    var workspaceManager = new WorkspaceManager(4);

    var panel = new SystemPanel
    {
        DisplayWidth = 1920,
        Width = 1920,
        Height = 44
    };

    panel.AddTrayIcon("Network", "wifi");
    panel.AddTrayIcon("Sound", "volume");
    panel.AddTrayIcon("Battery", "battery");

    var dash = new AppDash { ScreenWidth = 1920 };
    dash.AddApp("Terminal", "", () => { });
    dash.AddApp("Files", "", () => { });
    dash.AddApp("Browser", "", () => { });
    dash.AddApp("Settings", "", () => { });
    dash.AddApp("Store", "", () => { });

    var launcher = new AppLauncher
    {
        ScreenWidth = 1920,
        ScreenHeight = 1080
    };
    launcher.AddApp("Terminal", "System", () => { });
    launcher.AddApp("Files", "System", () => { });
    launcher.AddApp("Browser", "Internet", () => { });
    launcher.AddApp("Settings", "System", () => { });
    launcher.AddApp("Store", "System", () => { });
    launcher.AddApp("Calculator", "Utilities", () => { });
    launcher.AddApp("Calendar", "Office", () => { });
    launcher.AddApp("Text Editor", "Utilities", () => { });

    var notifications = new NotificationCenter { ScreenWidth = 1920 };

    var runDialog = new RunDialog
    {
        ScreenWidth = 1920,
        ScreenHeight = 1080
    };

    shortcuts.OnRunCommand += () => runDialog.Open();
    shortcuts.OnDesktopSwitch += () => Console.WriteLine("[Super+Tab] Desktop switcher");
    shortcuts.OnWindowSwitch += () => Console.WriteLine("[Alt+Tab] Window switcher");
    shortcuts.OnAppLauncher += () => launcher.Toggle();
    shortcuts.OnNotificationToggle += () => notifications.Toggle();
    shortcuts.OnSearch += (query) => launcher.Search(query);

    LiveWallpaperEngine? wallpaper = null;
    var bootSplash = new BootSplash();

    app.OnStarted += () =>
    {
        compositor.Start();
        Console.WriteLine($"Tonome Compositor started on {compositor.GetType().Name}");

        wallpaper = new LiveWallpaperEngine(app.Renderer!);

        app.Renderer!.OnRender = (canvas, delta, w, h) =>
        {
            wallpaper?.Render(canvas, w, h);

            Glass.BeginFrame(app.Renderer!.Surface, canvas);
            try
            {
                if (!bootSplash.IsFinished)
                    bootSplash.Render(canvas, w, h);

                panel.Render(canvas, delta);
                dash.Render(canvas, delta);
                notifications.Render(canvas, delta);
                launcher.Render(canvas, delta);
                runDialog.Render(canvas, delta);
            }
            finally
            {
                Glass.EndFrame();
            }
        };

        notifications.ShowNotification("Welcome", "to[no]ME! Desktop is ready", "System", false);
    };

    app.OnFrameUpdate += (delta) =>
    {
        workspaceManager.SwitchAnimation.Update(delta);
        wallpaper?.Update((float)delta);
    };

    app.OnShutdown += () =>
    {
        compositor.Stop();
        Console.WriteLine("Tonome Session ended.");
    };

    Log("Running main loop...");
    app.Run();
    Log("Session exited normally.");
}
catch (Exception ex)
{
    Log("FATAL session crash:");
    Log(ex.ToString());
    throw;
}
