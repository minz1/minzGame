using Godot;
using System;

public class MovingPlatform : KinematicBody2D
{
    [Export] private float IdleDuration = 1.0f;
    [Export] private Vector2 MoveTo = (Vector2.Right * 192) + (Vector2.Up * 96);
    [Export] private float Speed = 3.0f;
    private const float InterpolationConstant = 0.075f;

    private Vector2 Follow = Vector2.Zero;
    private Tween MoveTween;

    public override void _Ready()
    {
        MoveTween = GetNode<Tween>("Tween");

        float duration = MoveTo.Length() / (Speed * 30);
        MoveTween.InterpolateProperty(this, "Follow", Vector2.Zero, MoveTo, duration, Tween.TransitionType.Linear, Tween.EaseType.InOut, IdleDuration);
        MoveTween.InterpolateProperty(this, "Follow", MoveTo, Vector2.Zero, duration, Tween.TransitionType.Linear, Tween.EaseType.InOut, (duration + (IdleDuration * 2)));
        MoveTween.Start();
    }

    public Vector2 GetVelocity(float delta)
    {
        return (1 / delta) * (Position.LinearInterpolate(Follow, InterpolationConstant) - Position);
    }

    public override void _PhysicsProcess(float delta)
    {
        Position = Position.LinearInterpolate(Follow, InterpolationConstant);
    }
}
