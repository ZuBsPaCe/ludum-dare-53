using Godot;
using System;

public partial class Game : Node3D
{
    private City _city;

    public override void _Ready()
    {
        _city = GetNode<City>("City");
    }

    public void CloseGameAndFree()
    {
        QueueFree();
    }
}
