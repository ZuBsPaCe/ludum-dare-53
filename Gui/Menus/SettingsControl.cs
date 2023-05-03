using Godot;

public partial class SettingsControl : MenuControlBase
{
    private Button _musicMinusButton;

    public override void _Ready()
    {
        _musicMinusButton = GetNode<Button>("%MusicMinusButton");
        var musicSlider = GetNode<HSlider>("%MusicSlider");
        var soundSlider = GetNode<HSlider>("%SoundSlider");
        var fullscreenCheckBox = GetNode<CheckBox>("%FullscreenCheckBox");

        musicSlider.Value = UserSettings.MusicVolume;
        soundSlider.Value = UserSettings.SoundVolume;
        fullscreenCheckBox.ButtonPressed = UserSettings.Fullscreen;

        musicSlider.ValueChanged += (value) => Music_ValueChanged(value);
        soundSlider.ValueChanged += (value) => Sound_ValueChanged(value);
        fullscreenCheckBox.Toggled += (value) => Fullscreen_Toggled(value);
    }

    private void Fullscreen_Toggled(bool value)
    {
        UserSettings.Fullscreen = value;
    }

    private void Sound_ValueChanged(double value)
    {
        UserSettings.SoundVolume = (float) value;
    }

    private void Music_ValueChanged(double value)
    {
        UserSettings.MusicVolume = (float) value;
    }
}
