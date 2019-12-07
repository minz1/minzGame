using Godot;
using System;

public class MinzGame : Node2D
{
    private Label FPSLabel;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        FPSLabel = GetNode<Label>("FPSLabel");
        FPSLabel.Text = "FPS:";
    }

    public override void _Process(float delta)
    {
        FPSLabel.Text = $"FPS: {Math.Round(1 / delta)}";
    }
}
