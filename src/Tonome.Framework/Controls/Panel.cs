using SkiaSharp;

namespace Tonome.Framework.Controls;

public class Panel : Control
{
    public List<Control> Children { get; } = new();

    public void AddChild(Control child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public void RemoveChild(Control child)
    {
        child.Parent = null;
        Children.Remove(child);
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        if (!Visible) return;

        DrawBackground(canvas);

        foreach (var child in Children)
        {
            if (child.Visible)
                child.Render(canvas, delta);
        }
    }
}
