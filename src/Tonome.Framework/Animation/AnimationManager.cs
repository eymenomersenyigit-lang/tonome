namespace Tonome.Framework.Animation;

public class AnimationManager
{
    private readonly List<SpringAnimation> _animations = new();

    public SpringAnimation Create(float initial, float target)
    {
        var anim = new SpringAnimation(initial, target);
        _animations.Add(anim);
        return anim;
    }

    public void UpdateAll(double delta)
    {
        for (var i = _animations.Count - 1; i >= 0; i--)
        {
            _animations[i].Update(delta);
            if (_animations[i].IsCompleted)
                _animations.RemoveAt(i);
        }
    }

    public void Clear() => _animations.Clear();
}
