using Godot;
using System;

public partial class SettingsControl : MenuControlBase
{
    HSlider _musicSlider;
    HSlider _soundSlider;

    public override void _Ready()
    {
        Button musicMinusButton = GetNode<Button>("%MusicMinusButton");
        Button musicPlusButton = GetNode<Button>("%MusicPlusButton");
        Button soundMinusButton = GetNode<Button>("%SoundMinusButton");
        Button soundPlusButton = GetNode<Button>("%SoundPlusButton");

        _musicSlider = GetNode<HSlider>("%MusicSlider");
        _soundSlider = GetNode<HSlider>("%SoundSlider");
        var fullscreenCheckBox = GetNode<CheckBox>("%FullscreenCheckBox");

        _musicSlider.Value = UserSettings.MusicVolume;
        _soundSlider.Value = UserSettings.SoundVolume;
        fullscreenCheckBox.ButtonPressed = UserSettings.Fullscreen;

        _musicSlider.ValueChanged += (value) => Music_ValueChanged(value);
        _soundSlider.ValueChanged += (value) => Sound_ValueChanged(value);
        fullscreenCheckBox.Toggled += (value) => Fullscreen_Toggled(value);

        musicMinusButton.Pressed += MusicMinusButton_Pressed;
        musicPlusButton.Pressed += MusicPlusButton_Pressed;
        soundMinusButton.Pressed += SoundMinusButton_Pressed;
        soundPlusButton.Pressed += SoundPlusButton_Pressed;
    }

    private void SoundPlusButton_Pressed()
    {
        UserSettings.SoundVolume = Mathf.Clamp(UserSettings.SoundVolume + 0.025f, 0, 1);
        _soundSlider.Value = UserSettings.SoundVolume;
    }

    private void SoundMinusButton_Pressed()
    {
        UserSettings.SoundVolume = Mathf.Clamp(UserSettings.SoundVolume - 0.025f, 0, 1);
        _soundSlider.Value = UserSettings.SoundVolume;
    }

    private void MusicPlusButton_Pressed()
    {
        UserSettings.MusicVolume = Mathf.Clamp(UserSettings.MusicVolume + 0.025f, 0, 1);
        _musicSlider.Value = UserSettings.MusicVolume;
    }

    private void MusicMinusButton_Pressed()
    {
        UserSettings.MusicVolume = Mathf.Clamp(UserSettings.MusicVolume - 0.025f, 0, 1);
        _musicSlider.Value = UserSettings.MusicVolume;
    }

    private void Fullscreen_Toggled(bool value)
    {
        UserSettings.Fullscreen = value;
    }

    private void Sound_ValueChanged(double value)
    {
        UserSettings.SoundVolume = (float) value;
        Sounds.PlaySound(SoundType.Crash);
    }

    private void Music_ValueChanged(double value)
    {
        UserSettings.MusicVolume = (float) value;
    }
}
