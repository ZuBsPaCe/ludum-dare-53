using Godot;
using System;

public partial class Car : Node3D
{

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	private bool TryGetCoord(out Vector2I coord)
	{
		int xTile = (int)(GlobalPosition.X / State.TileSize);
		int yTile = (int)(GlobalPosition.Y / State.TileSize);

		if (State.Map.IsValid(xTile, yTile))
		{
			coord = new Vector2I(xTile, yTile);
			return true;
		}

		coord = Vector2I.Zero;
		return false;
    }
}
