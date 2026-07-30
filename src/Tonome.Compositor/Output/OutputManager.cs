using Tonome.Framework.Types;

namespace Tonome.Compositor.Output;

public class OutputInfo
{
    public string Name { get; set; } = "";
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public int PhysicalWidth { get; set; }
    public int PhysicalHeight { get; set; }
    public int RefreshRate { get; set; } = 60;
    public float Scale { get; set; } = 1.0f;
    public bool IsPrimary { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public string Connector { get; set; } = "";
    public string? Edid { get; set; }
    public bool SupportsHdr { get; set; }

    public Size Size => new(Width, Height);
}

public class OutputManager
{
    private readonly List<OutputInfo> _outputs = new();

    public IReadOnlyList<OutputInfo> Outputs => _outputs;
    public OutputInfo? PrimaryOutput => _outputs.FirstOrDefault(o => o.IsPrimary);

    public void DetectOutputs()
    {
        _outputs.Clear();

        _outputs.Add(new OutputInfo
        {
            Name = "eDP-1",
            Width = 1920,
            Height = 1080,
            RefreshRate = 60,
            Scale = 1.0f,
            IsPrimary = true,
            Connector = "eDP-1",
            SupportsHdr = false
        });
    }

    public void AddOutput(OutputInfo output)
    {
        _outputs.Add(output);
        if (output.IsPrimary)
        {
            var idx = _outputs.Count - 1;
            for (var i = 0; i < idx; i++)
                _outputs[i].IsPrimary = false;
        }
    }

    public void RemoveOutput(string name)
    {
        _outputs.RemoveAll(o => o.Name == name);
    }

    public OutputInfo? FindOutput(string name)
    {
        return _outputs.FirstOrDefault(o => o.Name == name);
    }

    public void ApplyColorSettings(float brightness, float contrast, float saturation)
    {
        foreach (var output in _outputs)
        {
            ApplyOutputColor(output, brightness, contrast, saturation);
        }
    }

    private static void ApplyOutputColor(OutputInfo output, float brightness, float contrast, float saturation)
    {
    }

    public Size GetTotalSize()
    {
        if (_outputs.Count == 0)
            return new Size(1920, 1080);

        var maxX = _outputs.Max(o => o.X + o.Width);
        var maxY = _outputs.Max(o => o.Y + o.Height);
        return new Size(maxX, maxY);
    }
}
