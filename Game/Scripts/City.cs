using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

[Tool]
public partial class City : Node3D
{
    [Export] private int _seed = 0;

	[Export] private int _xSectionCount = 8;
	[Export] private int _ySectionCount = 8;

	[Export] private int _sectionTileSize = 6;

	[Export] private int _tileSize = 10;

	[Export] private Vector2I _downtownCoord;
	[Export] private float _downtownRadius = 25;

	[Export] private Rect2I _parkCoords;

	[Export] private NodePath _genPath;

	[Export] private PackedScene _sceneStreetHor;
	[Export] private PackedScene _sceneStreetVer;
	[Export] private PackedScene _sceneStreetCrossing;
	[Export] private PackedScene _sceneStreetTeeShape;
	[Export] private PackedScene _sceneStreetDeadEnd;
	[Export] private PackedScene _sceneStreetCorner;

    [Export] private PackedScene _sceneBoundaryStraight;
    [Export] private PackedScene _sceneBoundaryCorner;

	[Export] private PackedScene _sceneWater;
	[Export] private PackedScene _scenePark;

    [Export] private Array<PackedScene> _sceneBuildings;
    [Export] private Array<Material> _concreteMaterials;
	[Export] private Material _baseMaterial;
	[Export] private Material _windowMaterial;

    [Export] private PackedScene _sceneQuestMarker;


	[Export] private Godot.Collections.Dictionary<string, Variant> _levelData;


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
	private CityMap _cityMap;
	private PlayerTruck _playerTruck;


    public override void _Ready()
    {
		if (Engine.IsEditorHint())
		{ 
			return; 
		}

		_playerTruck = GetNode<PlayerTruck>("%PlayerTruck");

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

		//_map = RunGenerate(false);

        GenerateTiles(root, _map);
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        _cityMap.UpdatePlayerPos(_playerTruck.GlobalPosition, -_playerTruck.GlobalRotation.Y);
    }

    public void Setup(CityMap cityMap)
	{
		_cityMap = cityMap;
		_cityMap.Setup(_map, _tileSize);
    }

	public bool TryGetRandomCoord(HashSet<TileType> tileTypes, out Vector2I coord)
	{
		return _map.TryGetRandomCoord(tileTypes, out coord);
	}

    public bool TryGetRandomCoord(TileType tileType, out Vector2I coord)
    {
        return _map.TryGetRandomCoord(tileType, out coord);
    }

    public void AddQuestMarker(Quest quest, Vector2I coord, bool isStart)
    {
        QuestMarker questMarker = _sceneQuestMarker.Instantiate<QuestMarker>();
		AddChild(questMarker);
		questMarker.Setup(quest, isStart);
		questMarker.GlobalPosition = GetRandomPosInCoord(coord);

		quest.QuestMarker = questMarker;

		_cityMap.AddQuest(quest);
    }

	private Vector3 GetRandomPosInCoord(Vector2I coord)
    {
        return new Vector3(coord.X * _tileSize + GD.RandPosInt() % _tileSize, 0, coord.Y * _tileSize + GD.RandPosInt() % _tileSize);
    }

    private Map<TileType, Tile> RunGenerate(bool debug)
    {
		GD.Seed((ulong) _seed);

		GD.Print($"Running Generate. Debug: [{debug}]");

		var root = GetNode<Node3D>(_genPath);
		foreach (var child in root.GetChildren())
			child.QueueFree();

		int width = _xSectionCount * _sectionTileSize;
		int height = _ySectionCount * _sectionTileSize;

        Map<TileType, Tile> map = new(width, height, TileType.Building);

		for (int ySection = 0; ySection < _ySectionCount; ++ySection)
			for (int xSection = 0; xSection < _xSectionCount; ++xSection)
			{
				GenerateSection(map, xSection, ySection);
			}


		{
			// Streets on the bottom and right boundary
			width += 1;
			height += 1;

			// Boundary tiles
			width += 2;
			height += 2;

			Map<TileType, Tile> mapWithBorder = new(width, height, TileType.Building);
			mapWithBorder.SetAllTypes(TileType.Boundary);

			for (int y = 0; y < height; ++y)
				mapWithBorder.SetType(width - 1, y, TileType.Boundary);

			for (int x = 0; x < width; ++x)
				mapWithBorder.SetType(x, height - 1, TileType.Boundary);

			for (int y = 1; y < height - 1; ++y)
			{
				mapWithBorder.SetType(1, y, TileType.Street);
				mapWithBorder.SetType(width - 2, y, TileType.Street);
			}

			for (int x = 1; x < width - 1; ++x)
			{
				mapWithBorder.SetType(x, 1, TileType.Street);
				mapWithBorder.SetType(x, height - 2, TileType.Street);
			}

			for (int y = 0; y < map.Height; ++y)
				for (int x = 0; x < map.Width; ++x)
					mapWithBorder.SetType(x + 1, y + 1, map.GetType(x, y));

			map = mapWithBorder;
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

		return map;
    }

	private void GenerateSection(Map<TileType, Tile> map, int xSection, int ySection)
	{
		bool hasHorStreet = GD.Randf() < 0.8f;
		bool hasVerStreet = !hasHorStreet || GD.Randf() < 0.8f;

		for (int yIndex = 0; yIndex < _sectionTileSize; ++yIndex)
		{
			int yTile = (ySection * _sectionTileSize + yIndex);
            

			for (int xIndex = 0; xIndex < _sectionTileSize; ++xIndex)
			{
				int xTile = xSection * _sectionTileSize + xIndex;

				TileType tileType;

                if (_parkCoords.HasPoint(xTile, yTile))
				{
					tileType = TileType.Park;
				}
                else if (yIndex == 0 && hasVerStreet || xIndex == 0 && hasHorStreet)
                {
					tileType = TileType.Street;
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
		List<PackedScene> orderedBuildings = _sceneBuildings.OrderBy(building => (int) building._Get("Height")).ToList();

		Vector2I? parkPos = null;

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
					case TileType.Park:
						if (parkPos == null)
						{
							parkPos = new Vector2I(x, y);
						}
						continue;

                    case TileType.Building:
						float distance = (_downtownCoord - new Vector2I(xTile, yTile)).Length();

						float factor = Mathf.Clamp(distance / _downtownRadius, 0.0f, 1.0f);

						int minIndex = Mathf.Max((int)(factor * orderedBuildings.Count - 3), 0);
						int maxIndex = minIndex + 2;

                        tile = orderedBuildings[GD.RandRange(minIndex, maxIndex)].Instantiate<Node3D>();

						foreach (var child in tile.GetChildren())
						{
							if (child is MeshInstance3D meshInstance)
							{
								if (child.Name.ToString().Contains("Window"))
								{
									meshInstance.MaterialOverride = _windowMaterial;
								}
								else if (child.Name.ToString().Contains("Base"))
								{
									meshInstance.MaterialOverride = _baseMaterial;
								}
								else
								{
									meshInstance.MaterialOverride = _concreteMaterials.PickRandom();
								}
							}
						}
						break;

                    case TileType.Street:
						bool topStreet = map.IsTypeAtDir4(xTile, yTile, Direction4.N, TileType.Street);
						bool bottomStreet = map.IsTypeAtDir4(xTile, yTile, Direction4.S, TileType.Street);

						bool rightStreet = map.IsTypeAtDir4(xTile, yTile, Direction4.E, TileType.Street);
						bool leftStreet = map.IsTypeAtDir4(xTile, yTile, Direction4.W, TileType.Street);

						if (topStreet && bottomStreet && rightStreet && leftStreet)
						{
							tile = _sceneStreetCrossing.Instantiate<Node3D>();
						}
                        else if (topStreet && bottomStreet && !rightStreet && !leftStreet)
						{
							tile = _sceneStreetVer.Instantiate<Node3D>();
						}
						else if (!topStreet && !bottomStreet && rightStreet && leftStreet)
						{
							tile = _sceneStreetHor.Instantiate<Node3D>();
						}
						else if (!topStreet && bottomStreet && rightStreet && !leftStreet)
						{
							// BR
							tile = _sceneStreetCorner.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.5f);
						}
						else if (!topStreet && bottomStreet && !rightStreet && leftStreet)
						{
							// BL
							tile = _sceneStreetCorner.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.25f);
						}
                        else if (topStreet && !bottomStreet && !rightStreet && leftStreet)
                        {
                            // TL
                            tile = _sceneStreetCorner.Instantiate<Node3D>();
                        }
                        else if (topStreet && !bottomStreet && rightStreet && !leftStreet)
                        {
                            // TR
                            tile = _sceneStreetCorner.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.75f);
                        }
						else if (!topStreet && bottomStreet && rightStreet && leftStreet)
						{
							// Tee Bottom
							tile = _sceneStreetTeeShape.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.5f);
						}
                        else if (topStreet && !bottomStreet && rightStreet && leftStreet)
                        {
                            // Tee Top
                            tile = _sceneStreetTeeShape.Instantiate<Node3D>();
                        }
                        else if (topStreet && bottomStreet && rightStreet && !leftStreet)
                        {
                            // Tee Right
                            tile = _sceneStreetTeeShape.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.75f);
                        }
						else if (topStreet && bottomStreet && !rightStreet && leftStreet)
						{
							// Tee Left
							tile = _sceneStreetTeeShape.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.25f);
						}
						else
						{
							//Debug.Fail("Unknown street tile");
							continue;
						}
						break;

                    case TileType.Boundary:
						bool topBoundary = map.IsTypeAtDir4(xTile, yTile, Direction4.N, TileType.Boundary);
						bool bottomBoundary = map.IsTypeAtDir4(xTile, yTile, Direction4.S, TileType.Boundary);

						bool leftBoundary = map.IsTypeAtDir4(xTile, yTile, Direction4.E, TileType.Boundary);
						bool rightBoundary = map.IsTypeAtDir4(xTile, yTile, Direction4.W, TileType.Boundary);

						if (topBoundary && bottomBoundary && !leftBoundary && !rightBoundary)
						{
							tile = _sceneBoundaryStraight.Instantiate<Node3D>();

                            if (map.IsValid(xTile + 1, yTile))
                                tile.RotateY(Mathf.Tau * 0.75f);
                            else
                                tile.RotateY(Mathf.Tau * 0.25f);
                        }
						else if (!topBoundary && !bottomBoundary && leftBoundary && rightBoundary)
						{
							tile = _sceneBoundaryStraight.Instantiate<Node3D>();

							if (map.IsValid(xTile, yTile + 1))
								tile.RotateY(Mathf.Tau * 0.5f);
						}
						else if (!topBoundary && bottomBoundary && !leftBoundary && rightBoundary)
						{
							// TL
							tile = _sceneBoundaryCorner.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.5f);
						}
						else if (!topBoundary && bottomBoundary && leftBoundary && !rightBoundary)
						{
							// TR
							tile = _sceneBoundaryCorner.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.75f);
						}
						else if (topBoundary && !bottomBoundary && leftBoundary && !rightBoundary)
						{
							// BR
							tile = _sceneBoundaryCorner.Instantiate<Node3D>();
						}
						else if (topBoundary && !bottomBoundary && !leftBoundary && rightBoundary)
						{
							// BL
							tile = _sceneBoundaryCorner.Instantiate<Node3D>();
							tile.RotateY(Mathf.Tau * 0.25f);
						}
						else
						{
							//Debug.Fail("Unknown boundary tile");
							continue;
						}
                        break;

                    default:
                        Debug.Fail($"Unknown TileType [{tileType}]");
						continue;
                }

                map.SetItem(xTile, yTile, new Tile());

                root.AddChild(tile);
                tile.Position = new Vector3(x, 0, y);
            }
        }


		if (parkPos != null)
		{
			Node3D park = _scenePark.Instantiate<Node3D>();
			root.AddChild(park);
			park.Position = new Vector3(parkPos.Value.X, 0, parkPos.Value.Y) - new Vector3(10, 0, 10);
		}


		int waterRings = 3;

        for (int xSection = -waterRings; xSection < _xSectionCount + waterRings; ++xSection)
			for (int ySection = -waterRings; ySection < _ySectionCount + waterRings; ++ySection)
			{
				if (xSection >= 0 && xSection < _xSectionCount && ySection >= 0 && ySection < _ySectionCount)
					continue;

                Node3D waterSection = _sceneWater.Instantiate<Node3D>();

                root.AddChild(waterSection);
                waterSection.Position = new Vector3((xSection + 0.5f) * _sectionTileSize, 0, (ySection + 0.5f) * _sectionTileSize) * _tileSize;
            }
    }
}
