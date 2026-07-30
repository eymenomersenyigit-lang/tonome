namespace Tonome.Framework.Animation;

public class SpringAnimation
{
    public float Value { get; private set; }
    public float Velocity { get; private set; }
    public float Target { get; set; }
    public float Stiffness { get; set; } = 180f;
    public float Damping { get; set; } = 12f;
    public float Mass { get; set; } = 1f;
    public bool IsCompleted => Math.Abs(Value - Target) < 0.01f && Math.Abs(Velocity) < 0.01f;

    public SpringAnimation(float initial = 0f, float target = 1f)
    {
        Value = initial;
        Target = target;
    }

    public void Update(double delta)
    {
        var dt = (float)delta;
        var force = -Stiffness * (Value - Target);
        var dampingForce = -Damping * Velocity;
        var acceleration = (force + dampingForce) / Mass;
        Velocity += acceleration * dt;
        Value += Velocity * dt;

        if (IsCompleted)
        {
            Value = Target;
            Velocity = 0f;
        }
    }

    public void SnapTo(float value)
    {
        Value = value;
        Target = value;
        Velocity = 0f;
    }
}
