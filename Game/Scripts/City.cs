using Godot;
using System;
using System.ComponentModel.Design;

[Tool]
public partial class City : Node3D
{
	[Export]
	private int _xSectionCount = 8;

	[Export]
	private int _ySectionCount = 8;

	[Export]
	private int _sectionTileSize = 6;

	[Export]
	private int _tileSize = 10;

	[Export]
	private NodePath _genPath;

	[Export]
	private PackedScene _sceneStreetHor;

	[Export]
	private PackedScene _sceneStreetVer;

	[Export]
	private PackedScene _sceneStreetCrossing;

	[Export]
	private Godot.Collections.Array<PackedScene> _sceneBuildings;



	[Export]
	public bool Generate
	{
		get { return false; }
		set
		{
			if (value)
				RunGenerate();
		}
	}

    private void RunGenerate()
    {
		var root = GetNode<Node3D>(_genPath);
		foreach (var child in root.GetChildren())
			child.QueueFree();

		for (int ySection = 0; ySection < _ySectionCount; ++ySection)
			for (int xSection = 0; xSection < _xSectionCount; ++xSection)
			{
				GenerateSection(root, xSection, ySection);
			}
    }

	private void GenerateSection(Node3D root, int xSection, int ySection)
	{
		for (int yTile = 0; yTile < _sectionTileSize; ++yTile)
		{
			int y = (ySection * _sectionTileSize + yTile) * _tileSize;

			for (int xTile = 0; xTile < _sectionTileSize; ++xTile)
			{
				int x = (xSection * _sectionTileSize + xTile) * _tileSize;

				Node3D tile;
				
				if (yTile == 0 && xTile == 0)
                {
                    tile = _sceneStreetCrossing.Instantiate<Node3D>();
				}
				else if (yTile == 0)
				{
                    tile = _sceneStreetHor.Instantiate<Node3D>();
				}
				else if (xTile == 0)
				{
                    tile = _sceneStreetVer.Instantiate<Node3D>();
				}
				else
				{
                    tile = _sceneBuildings[0].Instantiate<Node3D>();
				}

				

                tile.Position = new Vector3(x, 0, y);
                root.AddChild(tile);
                tile.Owner = this;



				//var streetVer = _sceneStreetVer.Instantiate<Node3D>();
				//streetVer.Position = new Vector3(x, 0, y);
				//root.AddChild(streetVer);
				//streetVer.Owner = this;
			}
		}
    }
}
