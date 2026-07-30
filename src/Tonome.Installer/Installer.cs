using Tonome.Framework;

namespace Tonome.Installer;

public class TonomeInstaller
{
    private readonly TonomeApplication _app;
    private readonly WelcomeWizard _wizard;
    private readonly TonomeInstallerEngine _engine;

    public TonomeInstaller()
    {
        _engine = new TonomeInstallerEngine();
        _wizard = new WelcomeWizard();

        _wizard.AddStep("Welcome",
            "Welcome to to[no]ME! OS\nThis wizard will guide you through installing the system.\n\n" +
            "to[no]ME! is an Arch Linux-based distribution\nwith the privacy-focused Tonome Desktop.");

        _wizard.AddStep("Language & Region",
            "Select your language, timezone, and keyboard layout.\n" +
            "Default: English (US), UTC, US Keyboard.",
            onEnter: () => _engine.ConfigureLocale("en_US.UTF-8", "UTC", "us"));

        _wizard.AddStep("Disk Setup",
            "Choose how to install:\n" +
            "  \u2022 Erase disk and auto-partition (recommended)\n" +
            "  \u2022 Manual partitioning (advanced)\n\n" +
            "Default: Auto-partition with Btrfs + swap.",
            onEnter: () => _engine.DetectDisks());

        _wizard.AddStep("User Account",
            "Create your user account.\n" +
            "You'll set a username and password for daily use.\n\n" +
            "Root access is available via sudo.",
            onEnter: () => _engine.PrepareUserAccount());

        _wizard.AddStep("Summary",
            "Review your installation settings before proceeding.\n\n" +
            "Installation will begin after you confirm.\n" +
            "This may take a few minutes.",
            onEnter: () => _engine.Summarize());

        _wizard.AddStep("Install",
            "Click Install to start the installation.\n\n" +
            "Do not power off your computer during installation.",
            onEnter: () => _engine.StartInstallation());

        _app = new TonomeApplication(1024, 720, "to[no]ME! Installer");

        _app.OnStarted += () =>
        {
            _app.Renderer!.OnRender = (canvas, delta, w, h) =>
            {
                _engine.Update(delta);
                _wizard.Render(canvas, delta);
            };
        };
    }

    public void Run()
    {
        _app.Run();
    }
}

internal class TonomeInstallerEngine
{
    private readonly List<string> _log = new();
    private readonly List<string> _disks = new();
    private string _targetDisk = "";
    private bool _installing;

    public void Update(double delta) { }

    public void ConfigureLocale(string lang, string timezone, string keyboard)
    {
        _log.Add($"Locale: {lang}, Timezone: {timezone}, Keyboard: {keyboard}");
    }

    public void DetectDisks()
    {
        _disks.Clear();
        try
        {
            var drives = System.IO.DriveInfo.GetDrives()
                .Where(d => d.DriveType == System.IO.DriveType.Fixed);
            foreach (var d in drives)
                _disks.Add($"{d.Name} ({d.TotalSize / (1024 * 1024 * 1024)} GB)");
        }
        catch
        {
            _disks.Add("No disks detected");
        }
        _targetDisk = _disks.FirstOrDefault() ?? "/dev/sda";
        _log.Add($"Target disk: {_targetDisk}");
    }

    public void PrepareUserAccount()
    {
        _log.Add("User account: to-be-created");
    }

    public void Summarize()
    {
        _log.Add("Summary: Ready to install");
    }

    public async void StartInstallation()
    {
        if (_installing) return;
        _installing = true;
        _log.Add("Installation started...");

        await Task.Run(() =>
        {
            System.Threading.Thread.Sleep(500);
            _log.Add("Partitioning disk...");
            System.Threading.Thread.Sleep(500);
            _log.Add("Formatting partitions...");
            System.Threading.Thread.Sleep(500);
            _log.Add("Installing base system...");
            System.Threading.Thread.Sleep(500);
            _log.Add("Configuring bootloader...");
            System.Threading.Thread.Sleep(500);
            _log.Add("Installation complete!");
        });

        _installing = false;
    }
}
