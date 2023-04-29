using Godot.Collections;
using Godot;
using System.IO;

public class State
{
	private static Map<TileType, Tile> _map;

    public static void StartLevel()
    {
        string levelText = File.ReadAllText("Level.json");
        var levelData = (Godot.Collections.Dictionary<string, Variant>)Json.ParseString(levelText);

        var tileTypeArray = (Array<int>)levelData["TileTypeArray"];

        _map = new((int)levelData["MapWidth"], (int)levelData["MapHeight"], TileType.Building);

        for (int index = 0; index < _map.Size; ++index)
        {
            _map.SetTypeIndex(index, (TileType)tileTypeArray[index]);
            _map.SetItemIndexed(index, new Tile());
        }
    }
}
