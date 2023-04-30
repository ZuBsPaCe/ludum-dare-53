using Godot;

public partial class EventHub : Node
{
    public static EventHub Instance { get; set; }

    [Signal] public delegate void SwitchGameStateEventHandler(GameState gameState);
    [Signal] public delegate void SwitchLevelStateEventHandler(LevelState levelState);

    [Signal] public delegate void QuestMarkerEnteredEventHandler(QuestMarker questMarker);

    public static void EmitSwitchGameState(GameState gameState)
    {
        Instance.EmitSignal(SignalName.SwitchGameState, (int) gameState);
    }

    public static void EmitSwitchLevelState(LevelState levelState)
    {
        Instance.EmitSignal(SignalName.SwitchLevelState, (int)levelState);
    }

    internal static void EmitQuestMarkerEntered(QuestMarker questMarker)
    {
        Instance.EmitSignal(SignalName.QuestMarkerEntered, questMarker);
    }
}
