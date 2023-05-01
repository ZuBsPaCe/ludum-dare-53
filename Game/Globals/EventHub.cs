﻿using Godot;

public partial class EventHub : Node
{
    public static EventHub Instance { get; set; }

    [Signal] public delegate void SwitchGameStateEventHandler(GameState gameState);
    [Signal] public delegate void SwitchLevelStateEventHandler(LevelState levelState);

    [Signal] public delegate void MusicVolumeChangedEventHandler(float volume);
    [Signal] public delegate void SoundVolumeChangedEventHandler(float volume);
    [Signal] public delegate void FullscreenChangedEventHandler(bool fullscreen);

    [Signal] public delegate void QuestMarkerEnteredEventHandler(QuestMarker questMarker, bool isStart);
    [Signal] public delegate void QuestAcceptedEventHandler(QuestMarker questMarker);

    [Signal] public delegate void FuelChangedEventHandler(int amount);
    [Signal] public delegate void MoneyChangedEventHandler(int amount);
    [Signal] public delegate void CountdownChangedEventHandler(bool active, float countdownSecs);

    public static void EmitSwitchGameState(GameState gameState)
    {
        Instance.EmitSignal(SignalName.SwitchGameState, (int) gameState);
    }

    public static void EmitSwitchLevelState(LevelState levelState)
    {
        Instance.EmitSignal(SignalName.SwitchLevelState, (int)levelState);
    }

    public static void EmitMusicVolumeChanged(float volume)
    {
        Instance.EmitSignal(SignalName.MusicVolumeChanged, volume);
    }

    public static void EmitSoundVolumeChanged(float volume)
    {
        Instance.EmitSignal(SignalName.SoundVolumeChanged, volume);
    }

    public static void EmitFullscreenChanged(bool fullscreen)
    {
        Instance.EmitSignal(SignalName.FullscreenChanged, fullscreen);
    }

    internal static void EmitQuestMarkerEntered(QuestMarker questMarker, bool isStart)
    {
        Instance.EmitSignal(SignalName.QuestMarkerEntered, questMarker, isStart);
    }

    internal static void EmitQuestAccepted(QuestMarker questMarker)
    {
        Instance.EmitSignal(SignalName.QuestAccepted, questMarker);
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
}
