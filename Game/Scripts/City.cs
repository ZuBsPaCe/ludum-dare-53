using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class City : Node3D
{
    [Export] private int _seed = 0;
	[Export] private bool _debugGenerator;

	[Export] private int _xSectionCount = 8;
	[Export] private int _ySectionCount = 8;

	[Export] private int _sectionTileSize = 6;

	[Export] private int _tileSize = 10;

	[Export] private Vector2I _downtownCoord;
	[Export] private float _downtownRadius = 25;

	[Export] private Rect2I _parkCoords;
	[Export] private Array<Vector2I> _fuelSections;

	[Export] private NodePath _genPath;

	[Export] private PackedScene _sceneStreetHor;
	[Export] private PackedScene _sceneStreetVer;
	[Export] private PackedScene _sceneStreetCrossing;
	[Export] private PackedScene _sceneStreetTeeShape;
	[Export] private PackedScene _sceneStreetDeadEnd;
	[Export] private PackedScene _sceneStreetCorner;

	[Export] private PackedScene _sceneFloor;

    [Export] private PackedScene _sceneBoundaryStraight;
    [Export] private PackedScene _sceneBoundaryCorner;

	[Export] private PackedScene _sceneWater;
	[Export] private PackedScene _scenePark;

	[Export] private PackedScene _sceneFuelStation;

    [Export] private Array<PackedScene> _sceneBuildings;
    [Export] private Array<Material> _concreteMaterials;
	[Export] private Material _baseMaterial;
	[Export] private Material _windowMaterial;
	[Export] private Material _redMaterial;
	[Export] private Material _blackMaterial;

    [Export] private PackedScene _sceneQuestMarker;
    [Export] private PackedScene _sceneFuelMarker;

	[Export] private Material _questMarkerEasyMaterial;
	[Export] private Material _questMarkerMediumMaterial;
	[Export] private Material _questMarkerHardMaterial;

	[Export] private Array<PackedScene> _sceneCars;
	[Export] private Array<Material> _carsMaterials;

	[Export] private PackedScene _sceneStar;

	[Export] private PackedScene _sceneTree;

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
	private List<Car> _cars = new();

	private Vector3 _playerTruckResetPos;
	private Vector3 _playerTruckResetRot;

	private Node3D _genRoot;

	private const float _inSightRange = 300f;

    public override void _Ready()
    {
		if (Engine.IsEditorHint())
		{ 
			return; 
		}

		_playerTruck = GetNode<PlayerTruck>("%PlayerTruck");
		_playerTruckResetPos = _playerTruck.GlobalPosition;
		_playerTruckResetRot = _playerTruck.GlobalRotation;

        var playerRot = _playerTruckResetRot;
        playerRot.Y += GD.Randf() * Mathf.Tau;
		_playerTruck.GlobalRotation = playerRot;

        _genRoot = GetNode<Node3D>(_genPath);
        foreach (var child in _genRoot.GetChildren())
            child.QueueFree();


		if (_levelData == null || _levelData.Count == 0)
			return;

		var tileTypeArray = (Array<int>)_levelData["TileTypeArray"];

		_map = new((int)_levelData["MapWidth"], (int)_levelData["MapHeight"], TileType.Building);

		for (int index = 0; index < _map.Size; ++index)
		{
			_map.SetTypeIndex(index, (TileType)tileTypeArray[index]);
		}

#if DEBUG
		if (_debugGenerator)
            _map = RunGenerate(false);
#endif

		GD.Randomize();

		GenerateTiles(_genRoot, _map);
		GenerateCars(_genRoot, _map);

		for (int i = 0; i < 2; ++i)
            CreateStar(_genRoot, _map);

		State.Map = _map;
		State.TileSize = _tileSize;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        _cityMap.UpdatePlayerPos(_playerTruck.GlobalPosition, -_playerTruck.GlobalRotation.Y);

		int removedCount = 0;
		Vector3 playerGlobalPosition = _playerTruck.GlobalPosition;
        for (int i = _cars.Count - 1; i >= 0; i--)
		{
            Car car = _cars[i];
            if (car.NotMovedTime > 120 && (playerGlobalPosition - car.GlobalPosition).Length() > 400 || car.GlobalPosition.Y < -20)
            {
				_cars.RemoveAt(i);
				car.QueueFree();
				removedCount += 1;
			}
		}

		for (int i = 0; i < removedCount; ++i)
		{
			for (int j = 0; j < 10; ++j)
			{
				if (State.Map.TryGetRandomCoord(TileType.Street, out Vector2I coord) &&
					(playerGlobalPosition - GetCenterPosInCoord(coord)).Length() > _inSightRange)
				{
					CreateCar(_genRoot, State.Map, coord.X, coord.Y);
					break;
				}
			}
		}


        {
            int xTile = Mathf.RoundToInt(PlayerPos.X / State.TileSize);
            int yTile = Mathf.RoundToInt(PlayerPos.Z / State.TileSize);

            if (State.Map.IsType(xTile, yTile, TileType.Street))
            {
				var newResetPos = GetCenterPosInCoord(new Vector2I(xTile, yTile));
				newResetPos.Y = _playerTruckResetPos.Y;
				_playerTruckResetPos = newResetPos;
            }
        }
    }

	public Vector3 PlayerPos
	{
		get { return _playerTruck.GlobalPosition; }
	}

	public float PlayerSpeed
	{
		get { return _playerTruck.LinearVelocity.Length(); }
	}
	
	public bool TruckIsFlipped
	{
		get
		{
			return Mathf.Abs(_playerTruck.RotationDegrees.Z) > 10.0 || Mathf.Abs(_playerTruck.RotationDegrees.X) > 10.0;
		}
	}

	public int TileSize
	{
		get { return _tileSize; }
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
		Material material;

        switch (quest.QuestLevel)
        {
            case QuestLevel.Medium:
				material = _questMarkerMediumMaterial;
                break;
            case QuestLevel.Hard:
				material = _questMarkerHardMaterial;
                break;
            default:
				material = _questMarkerEasyMaterial;
				Debug.Assert(quest.QuestLevel == QuestLevel.Easy);
                break;
        }

        QuestMarker questMarker = _sceneQuestMarker.Instantiate<QuestMarker>();
		AddChild(questMarker);
		questMarker.Setup(quest, isStart, material);
		questMarker.GlobalPosition = GetRandomPosInCoord(coord);


        quest.QuestMarker = questMarker;

		_cityMap.AddQuest(quest);
    }

    public void AddFuelMarker(Vector2I coord)
	{
        Node3D fuelMarker = _sceneFuelMarker.Instantiate<Node3D>();
		AddChild(fuelMarker);
        fuelMarker.GlobalPosition = GetCenterPosInCoord(coord);
	}

	public void ResetPlayerTruck()
	{
		var resetRot = _playerTruckResetRot;
		resetRot.Y += GD.Randf() * Mathf.Tau;

        _playerTruck.GlobalPosition = _playerTruckResetPos;
		_playerTruck.GlobalRotation = resetRot;
        _playerTruck.LinearVelocity = Vector3.Zero;
		_playerTruck.AngularVelocity = Vector3.Zero;
	}

	private Vector3 GetRandomPosInCoord(Vector2I coord)
    {
		return GetCenterPosInCoord(coord) + new Vector3(1, 0, 0).Normalized().Rotated(Vector3I.Up, GD.Randf() * Mathf.Tau) * (float) GD.RandRange(_tileSize * 0.1f, _tileSize * 0.3f);
    }

	private Vector3 GetCenterPosInCoord(Vector2I coord)
	{
		return
			new Vector3(
				coord.X * _tileSize,
				0,
				coord.Y * _tileSize);

		// Braucht es nicht.. Verstehe nicht wieso.
			//+ new Vector3(
			//	0.5f * _tileSize,
			//	0,
			//	0.5f * _tileSize);
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

        foreach (Vector2I fuelSection in _fuelSections)
        {
			Debug.Assert(fuelSection.X >= 0 && fuelSection.X < _xSectionCount);
			Debug.Assert(fuelSection.Y >= 0 && fuelSection.Y < _ySectionCount);

			GenerateFuelStationInSection(map, fuelSection.X, fuelSection.Y);
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

	private void GenerateFuelStationInSection(Map<TileType, Tile> map, int xSection, int ySection)
	{
		List<Vector2I> validFuelCoords = new();

		for (int yIndex = 0; yIndex < _sectionTileSize; ++yIndex)
		{
			int yTile = (ySection * _sectionTileSize + yIndex);

			for (int xIndex = 0; xIndex < _sectionTileSize; ++xIndex)
			{
				int xTile = xSection * _sectionTileSize + xIndex;

				if (map.IsType(xTile, yTile, TileType.Building))
				{
					if (map.GetNeighbour4Count(xTile, yTile, TileType.Street) > 0 || map.GetInvalidNeighbour4Count(xTile, yTile) > 0)
					{
						validFuelCoords.Add(new Vector2I(xTile, yTile));
					}
				}
			}
		}

		map.SetType(validFuelCoords.GetRandomItem(), TileType.Fuel);
	}

    private void GenerateTiles(Node3D root, Map<TileType, Tile> map)
	{
		List<PackedScene> orderedBuildings = _sceneBuildings.OrderBy(building => (int) building._Get("Height")).ToList();

		Vector2I? parkPos = null;
		List<Vector2I> parkCoords = new();

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
                        parkCoords.Add(new Vector2I(xTile, yTile));
						continue;

                    case TileType.Building:
                        float distance = (_downtownCoord - new Vector2I(xTile, yTile)).Length();

                        float factor = Mathf.Clamp(distance / _downtownRadius, 0.0f, 1.0f);

                        int minIndex = Mathf.Max((int)(factor * orderedBuildings.Count - 3), 0);
                        int maxIndex = minIndex + 2;

						if (minIndex <= 2 && GD.Randf() < 0.2)
						{
							tile = _sceneFloor.Instantiate<Node3D>();

							int trees = GD.RandRange(0, 4);
                            for (int i = 0; i < trees; ++i)
                            {
                                Node3D tree = _sceneTree.Instantiate<Node3D>();
                                root.AddChild(tree);
                                tree.Position = GetRandomPosInCoord(new Vector2I(xTile, yTile));
                            }
                        }
						else
						{
                            tile = orderedBuildings[GD.RandRange(minIndex, maxIndex)].Instantiate<Node3D>();
                            CustomizeMaterials(tile);
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

					case TileType.Fuel:
                        tile = _sceneFuelStation.Instantiate<Node3D>();

                        if (map.IsTypeAtDir4(xTile, yTile, Direction4.W, TileType.Street))
						{
							tile.RotateY(Mathf.Tau * 0.5f);
						}
						else if (map.IsTypeAtDir4(xTile, yTile, Direction4.N, TileType.Street))
						{
							tile.RotateY(Mathf.Tau * 0.25f);
						}
						else if (map.IsTypeAtDir4(xTile, yTile, Direction4.S, TileType.Street))
						{
							tile.RotateY(Mathf.Tau * 0.75f);
						}

						AddFuelMarker(new Vector2I(xTile, yTile));

                        CustomizeMaterials(tile);
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

			for (int i = 0; i < 50; ++i)
			{
				Node3D tree = _sceneTree.Instantiate<Node3D>();
				root.AddChild(tree);
				tree.Position = GetRandomPosInCoord(parkCoords.GetRandomItem());
			}
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

	private void GenerateCars(Node3D root, Map<TileType, Tile> map)
	{
		for (int yTile = 0; yTile < map.Height; ++yTile)
		{
			int y = yTile * _tileSize;

			for (int xTile = 0; xTile < map.Width; ++xTile)
			{
				int x = xTile * _tileSize;

				if (xTile == 13 && yTile == 13)
				{
					continue;
				}

				if (GD.Randf() < 0.6)
				{
					continue;
				}

				TileType tileType = map.GetType(xTile, yTile);

				if (tileType == TileType.Street)
                {
                    CreateCar(root, map, xTile, yTile);
                }
            }
        }
    }

    private void CreateCar(Node3D root, Map<TileType, Tile> map, int xTile, int yTile)
    {
        Vector3 position = GetCenterPosInCoord(new Vector2I(xTile, yTile)) + Vector3.Up * 5;

        Car car = _sceneCars.PickRandom().Instantiate<Car>();
        root.AddChild(car);

        _cars.Add(car);

        car.GetNode<MeshInstance3D>("Car").MaterialOverride = _carsMaterials.PickRandom();

        car.GlobalPosition = position;

        List<Direction4> possibleDirs = new();
        if (map.IsTypeAtDir4(xTile, yTile, Direction4.N, TileType.Street))
        {
            possibleDirs.Add(Direction4.N);
        }
        if (map.IsTypeAtDir4(xTile, yTile, Direction4.E, TileType.Street))
        {
            possibleDirs.Add(Direction4.E);
        }
        if (map.IsTypeAtDir4(xTile, yTile, Direction4.S, TileType.Street))
        {
            possibleDirs.Add(Direction4.S);
        }
        if (map.IsTypeAtDir4(xTile, yTile, Direction4.W, TileType.Street))
        {
            possibleDirs.Add(Direction4.W);
        }

        Direction4 dir = possibleDirs.GetRandomItem();
        switch (dir)
        {
            case Direction4.N:
                car.Rotate(Vector3.Up, Mathf.Tau * 0.5f);
                break;
            case Direction4.E:
                car.Rotate(Vector3.Up, Mathf.Tau * 0.25f);
                break;
            case Direction4.S:
                break;
            case Direction4.W:
                car.Rotate(Vector3.Up, Mathf.Tau * 0.75f);
                break;
        }
    }

	public void CreateStar()
	{
		CreateStar(_genRoot, _map);
	}

	private void CreateStar(Node3D root, Map<TileType, Tile> map)
	{
		Vector3 playerGlobalPosition = PlayerPos;

        for (int j = 0; j < 50; ++j)
        {
            if (map.TryGetRandomCoord(TileType.Street, out Vector2I coord) &&
                (playerGlobalPosition - GetCenterPosInCoord(coord)).Length() > _inSightRange)
            {
                Vector3 position = GetCenterPosInCoord(new Vector2I(coord.X, coord.Y)) + Vector3.Up * 2;

                Star star = _sceneStar.Instantiate<Star>();
                root.AddChild(star);
				star.GlobalPosition = position;

                break;
            }
        }
    }

    private void CustomizeMaterials(Node3D tile)
    {
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
                else if (child.Name.ToString().Contains("Red"))
				{
                    meshInstance.MaterialOverride = _redMaterial;
				}
                else if (child.Name.ToString().Contains("Black"))
				{
                    meshInstance.MaterialOverride = _blackMaterial;
				}
                else
                {
                    meshInstance.MaterialOverride = _concreteMaterials.PickRandom();
                }
            }
        }
    }

}
