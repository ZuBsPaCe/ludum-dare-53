using Godot;

public partial class CityMap : SubViewport
{
    [Export] private float _textureSize = 16;
    [Export] private PackedScene _sceneStreet;
    [Export] private PackedScene _sceneBuilding;
    [Export] private PackedScene _scenePlayer;
    [Export] private PackedScene _sceneQuest;

    private int _tileSize;

    private float _scale;
    private float _scaledTileSize;
    private Sprite2D _playerSprite;

    private float _textureScale;

    public void Setup(Map<TileType, Tile> map, int tileSize)
    {
        _tileSize = tileSize;

        float xScale = Size.X / (map.Width * tileSize);
        float yScale = Size.Y / (map.Height * tileSize);

        _scale = Mathf.Min(xScale, yScale);
        _scaledTileSize = _scale * tileSize;

        _textureScale = tileSize / _textureSize;


        for (int yTile = 0; yTile < map.Height; ++yTile)
        {
            for (int xTile = 0; xTile < map.Width; ++xTile)
            {
                TileType tileType = map.GetType(xTile, yTile);
                Sprite2D sprite;

                switch (tileType)
                {
                    case TileType.Building:
                        sprite = _sceneBuilding.Instantiate<Sprite2D>();
                        break;

                    case TileType.Street:
                        sprite = _sceneStreet.Instantiate<Sprite2D>();
                        break;

                    case TileType.Boundary:
                        continue;

                    default:
                        Debug.Fail($"Unknown enum [{tileType}]");
                        continue;
                }

                AddChild(sprite);
                sprite.Position = new Vector2(xTile * _scaledTileSize, yTile * _scaledTileSize);
                sprite.Scale = new Vector2(_textureScale, _textureScale);
            }
        }
    }

    public void UpdatePlayerPos(Vector3 pos, float radAngle)
    {
        if (_playerSprite == null)
        {
            _playerSprite = _scenePlayer.Instantiate<Sprite2D>();
            AddChild(_playerSprite);
            _playerSprite.Scale = new Vector2(_textureScale, _textureScale);
        }

        _playerSprite.Position = GetScalePos(pos);
        _playerSprite.Rotation = radAngle;
    }

    public void AddQuest(Quest quest)
    {
        Sprite2D questSprite = _sceneQuest.Instantiate<Sprite2D>();
        AddChild(questSprite);
        questSprite.Scale = new Vector2(_textureScale, _textureScale);

        questSprite.Position = GetScalePos(quest.QuestMarker.GlobalPosition);

        quest.QuestSprite = questSprite;
    }

    private Vector2 GetScalePos(Vector3 pos)
    {
        return new Vector2(pos.X / _tileSize* _scaledTileSize, pos.Z / _tileSize* _scaledTileSize);
    }
}
