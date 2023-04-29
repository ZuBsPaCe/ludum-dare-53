using Godot;
using Godot.Collections;
using System.IO;

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
	private Array<PackedScene> _sceneBuildings;



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

		Map<TileType, Tile> map = new(_xSectionCount * _sectionTileSize, _ySectionCount * _sectionTileSize, TileType.Building);

		for (int ySection = 0; ySection < _ySectionCount; ++ySection)
			for (int xSection = 0; xSection < _xSectionCount; ++xSection)
			{
				GenerateSection(map, root, xSection, ySection);
			}


		Array<int> tileTypeArray = new();

		for (int index = 0; index < map.Size; ++index)
		{
			tileTypeArray.Add((int) map.GetTypeIndexed(index));
        }

        Dictionary<string, Variant> levelData = new()
        {
            { "TileTypeArray", tileTypeArray },
            { "MapWidth", map.Width },
            { "MapHeight", map.Height }
        };

        string levelText = Json.Stringify(levelData);
		File.WriteAllText("Level.json", levelText);
    }

	private void GenerateSection(Map<TileType, Tile> map, Node3D root, int xSection, int ySection)
	{
		for (int yIndex = 0; yIndex < _sectionTileSize; ++yIndex)
		{
			int yTile = (ySection * _sectionTileSize + yIndex);
            int y = yTile * _tileSize;

			for (int xIndex = 0; xIndex < _sectionTileSize; ++xIndex)
			{
				int xTile = xSection * _sectionTileSize + xIndex;
                int x = xTile * _tileSize;

				TileType tileType;
				Node3D tile;
				
				if (yIndex == 0 && xIndex == 0)
                {
					tileType = TileType.StreetCrossing;
                    tile = _sceneStreetCrossing.Instantiate<Node3D>();
				}
				else if (yIndex == 0)
				{
					tileType = TileType.StreetHor;
                    tile = _sceneStreetHor.Instantiate<Node3D>();
				}
				else if (xIndex == 0)
				{
					tileType = TileType.StreetVer;
                    tile = _sceneStreetVer.Instantiate<Node3D>();
				}
				else
				{
					tileType = TileType.Building;
                    tile = _sceneBuildings[0].Instantiate<Node3D>();
				}

				map.SetType(xTile, yTile, tileType);
				map.SetItem(xTile, yTile, new Tile());

                tile.Position = new Vector3(x, 0, y);
                root.AddChild(tile);
                tile.Owner = this;
			}
		}
    }
}
