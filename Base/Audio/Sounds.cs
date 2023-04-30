using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

public partial class Sounds : Node
{
    public static Sounds Instance { get; set; }

    private static Dictionary<int, AudioStreamPlayer> _sounds = new();

    private static Dictionary<string, int> _audioBusIndex = new();

    public static void RegisterSound<T>(T soundType, AudioStreamPlayer player) where T : Enum
    {
        int intType = Unsafe.As<T, int>(ref soundType);

        _sounds.Add(intType, player);
    }

    public static void PlaySound<T>(T soundType) where T : Enum
    {
        int intType = Unsafe.As<T, int>(ref soundType);

        _sounds[intType].Play();
    }

    public static void SetVolume(string busName, double factor)
    {
        factor = Mathf.Clamp(factor, 0, 1);

        if (!_audioBusIndex.TryGetValue(busName, out int busIndex))
        {
            busIndex = AudioServer.GetBusIndex(busName);
        }

        AudioServer.SetBusVolumeDb(busIndex, (float) Mathf.LinearToDb(factor));
    }
}
