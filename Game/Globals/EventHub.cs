using Godot;
using System;

public partial class EventHub : Node
{
    public static EventHub Instance { get; set; }

    [Signal] public delegate void SwitchGameStateEventHandler(GameState gameState);

    [Signal] public delegate void MusicVolumeChangedEventHandler(float volume);
    [Signal] public delegate void SoundVolumeChangedEventHandler(float volume);
    [Signal] public delegate void FullscreenChangedEventHandler(bool fullscreen);

    public static void EmitSwitchGameState(GameState gameState)
    {
        Instance.EmitSignal(SignalName.SwitchGameState, (int) gameState);
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
}
