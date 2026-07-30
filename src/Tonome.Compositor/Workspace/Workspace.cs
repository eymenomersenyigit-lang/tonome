using SkiaSharp;
using Tonome.Framework.Types;
using Tonome.Framework.Controls;

namespace Tonome.Compositor.Workspace;

public class Workspace
{
    public int Id { get; }
    public string Name { get; set; }
    public List<CompositorWindow> Windows { get; } = new();
    public Color BackgroundColor { get; set; } = new(20, 20, 30);
    public string? WallpaperPath { get; set; }

    public Workspace(int id, string name = "Desktop")
    {
        Id = id;
        Name = name;
    }

    public void AddWindow(CompositorWindow window)
    {
        if (!Windows.Contains(window))
            Windows.Add(window);
    }

    public void RemoveWindow(CompositorWindow window)
    {
        Windows.Remove(window);
    }

    public void RenderBackground(SKCanvas canvas, double delta)
    {
        canvas.Clear(BackgroundColor.ToSkia());
    }

    public int WindowCount => Windows.Count;
}
