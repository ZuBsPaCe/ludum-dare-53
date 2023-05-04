using Godot;
using System;

public partial class GameEventHub : Node
{
    public static GameEventHub Instance { get; set; }

    [Signal] public delegate void SwitchLevelStateEventHandler(LevelState levelState);

    [Signal] public delegate void QuestMarkerEnteredEventHandler(QuestMarker questMarker, bool isStart);
    [Signal] public delegate void QuestMarkerExitedEventHandler();
    [Signal] public delegate void QuestAcceptedEventHandler(QuestMarker questMarker);

    [Signal] public delegate void FuelMarkerEnteredEventHandler();
    [Signal] public delegate void FuelMarkerExitedEventHandler();

    [Signal] public delegate void FuelChangedEventHandler(int amount);
    [Signal] public delegate void MoneyChangedEventHandler(int amount);
    [Signal] public delegate void CountdownChangedEventHandler(bool active, float countdownSecs);

    [Signal] public delegate void ShopBoughtWinEventHandler();

    [Signal] public delegate void StarPickedUpEventHandler();
    [Signal] public delegate void StarDoneEventHandler();

    [Signal] public delegate void GripBoughtEventHandler();

    [Signal] public delegate void ResetTruckEventHandler();

    public static void EmitSwitchLevelState(LevelState levelState)
    {
        Instance.EmitSignal(SignalName.SwitchLevelState, (int)levelState);
    }

    internal static void EmitQuestMarkerEntered(QuestMarker questMarker, bool isStart)
    {
        Instance.EmitSignal(SignalName.QuestMarkerEntered, questMarker, isStart);
    }

    internal static void EmitQuestMarkerExited()
    {
        Instance.EmitSignal(SignalName.QuestMarkerExited);
    }

    internal static void EmitQuestAccepted(QuestMarker questMarker)
    {
        Instance.EmitSignal(SignalName.QuestAccepted, questMarker);
    }

    internal static void EmitFuelMarkerEntered()
    {
        Instance.EmitSignal(SignalName.FuelMarkerEntered);
    }

    internal static void EmitFuelMarkerExited()
    {
        Instance.EmitSignal(SignalName.FuelMarkerExited);
    }

    internal static void EmitFuelChanged(int amount)
    {
        Instance.EmitSignal(SignalName.FuelChanged, amount);
    }

    internal static void EmitMoneyChanged(int amount)
    {
        Instance.EmitSignal(SignalName.MoneyChanged, amount);
    }

    internal static void EmitCountdownChanged(bool active, float countdownSecs)
    {
        Instance.EmitSignal(SignalName.CountdownChanged, active, countdownSecs);
    }

    internal static void EmitShopBoughtWin()
    {
        Instance.EmitSignal(SignalName.ShopBoughtWin);
    }

    internal static void EmitStarPickedUp() 
    {
        Instance.EmitSignal(SignalName.StarPickedUp);
    }

    internal static void EmitStarDone()
    {
        Instance.EmitSignal(SignalName.StarDone);
    }

    internal static void EmitGripBought()
    {
        Instance.EmitSignal(SignalName.GripBought);
    }

    internal static void EmitResetTruck()
    {
        Instance.EmitSignal(SignalName.ResetTruck);
    }
}
