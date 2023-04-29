using Godot;

public class Quest
{
    public Quest(Vector2I coord)
    {
        Coord = coord;
    }

    public Vector2I Coord { get; }

    public Node3D QuestMarker { get; set; }
    public Sprite2D QuestSprite { get; internal set; }
}
