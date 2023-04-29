using Godot;
using Godot.Collections;

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
	private Godot.Collections.Dictionary<string, Variant> _levelData;


	[Export]
	public bool GenerateDebug
	{
		get { return false; }
		set
		{
			if (value)
				RunGenerate(true);
		}
	}


    [Export]
    public bool GenerateRelease
    {
        get { return false; }
        set
        {
            if (value)
                RunGenerate(false);
        }
    }

	private Map<TileType, Tile> _map;


    public override void _Ready()
    {
		if (Engine.IsEditorHint())
		{ 
			return; 
		}

        var root = GetNode<Node3D>(_genPath);
        foreach (var child in root.GetChildren())
            child.QueueFree();


		if (_levelData == null || _levelData.Count == 0)
			return;

        var tileTypeArray = (Array<int>)_levelData["TileTypeArray"];

        _map = new((int)_levelData["MapWidth"], (int)_levelData["MapHeight"], TileType.Building);

        for (int index = 0; index < _map.Size; ++index)
        {
            _map.SetTypeIndex(index, (TileType)tileTypeArray[index]);
        }

        GenerateTiles(root, _map);
    }

    private void RunGenerate(bool debug)
    {
		GD.Print($"Running Generate. Debug: [{debug}]");

		var root = GetNode<Node3D>(_genPath);
		foreach (var child in root.GetChildren())
			child.QueueFree();

		Map<TileType, Tile> map = new(_xSectionCount * _sectionTileSize, _ySectionCount * _sectionTileSize, TileType.Building);

		for (int ySection = 0; ySection < _ySectionCount; ++ySection)
			for (int xSection = 0; xSection < _xSectionCount; ++xSection)
			{
				GenerateSection(map, xSection, ySection);
			}

		Array<int> tileTypeArray = new();

		for (int index = 0; index < map.Size; ++index)
		{
			tileTypeArray.Add((int) map.GetTypeIndexed(index));
        }

		_levelData = new();
		
		_levelData["TileTypeArray"] = tileTypeArray;
		_levelData["MapWidth"] = map.Width;
		_levelData["MapHeight"] = map.Height;

		if (debug)
		{
			GenerateTiles(root, map);
		}
    }

	private void GenerateSection(Map<TileType, Tile> map, int xSection, int ySection)
	{
		for (int yIndex = 0; yIndex < _sectionTileSize; ++yIndex)
		{
			int yTile = (ySection * _sectionTileSize + yIndex);
            

			for (int xIndex = 0; xIndex < _sectionTileSize; ++xIndex)
			{
				int xTile = xSection * _sectionTileSize + xIndex;

				TileType tileType;

				if (yIndex == 0 && xIndex == 0)
                {
					tileType = TileType.StreetCrossing;
				}
				else if (yIndex == 0)
				{
					tileType = TileType.StreetHor;
				}
				else if (xIndex == 0)
				{
					tileType = TileType.StreetVer;
				}
				else
				{
					tileType = TileType.Building;
				}

				map.SetType(xTile, yTile, tileType);
				map.SetItem(xTile, yTile, new Tile());
			}
		}
    }

	private void GenerateTiles(Node3D root, Map<TileType, Tile> map)
	{
		for (int yTile = 0; yTile < map.Height; ++yTile)
		{
            int y = yTile * _tileSize;

            for (int xTile = 0; xTile < map.Width; ++xTile)
			{
                int x = xTile * _tileSize;

				TileType tileType = map.GetType(xTile, yTile);

                Node3D tile;

                switch (tileType)
                {
                    case TileType.Building:
                        tile = _sceneBuildings[0].Instantiate<Node3D>();
                        break;

                    case TileType.StreetVer:
                        tile = _sceneStreetVer.Instantiate<Node3D>();
                        break;

                    case TileType.StreetHor:
                        tile = _sceneStreetHor.Instantiate<Node3D>();
                        break;

                    case TileType.StreetCrossing:
                        tile = _sceneStreetCrossing.Instantiate<Node3D>();
                        break;

                    default:
                        Debug.Fail($"Unknown TileType [{tileType}]");
						continue;
                }

                map.SetItem(xTile, yTile, new Tile());

                tile.Position = new Vector3(x, 0, y);
                root.AddChild(tile);
            }
        }
    }
}