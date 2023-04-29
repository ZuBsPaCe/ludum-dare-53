using Godot;
using System;

public partial class Game : Node3D
{
    [Export] private PackedScene _sceneCity;

    private City _city;

    public override void _Ready()
    {
        _city = _sceneCity.Instantiate<City>();
        AddChild(_city);
    }

    public void CloseGameAndFree()
    {
        QueueFree();
    }
}
