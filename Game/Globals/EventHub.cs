using Godot;

public partial class EventHub : Node
{
    public static EventHub Instance { get; set; }

    [Signal] public delegate void SwitchGameStateEventHandler(GameState gameState);

    public static void EmitSwitchGameState(GameState gameState)
    {
        Instance.EmitSignal(SignalName.SwitchGameState, (int) gameState);
    }
}
