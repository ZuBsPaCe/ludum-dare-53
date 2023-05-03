using Godot;

public partial class MainMenu : MenuBase
{
    [Export] private PackedScene _sceneSettingsControl;
    [Export] private PackedScene _titleControl;

    public override void _Ready()
    {
        Control buttonBar = GetNode<MarginContainer>("%ButtonBar");
        InitButtonBar(buttonBar);

        Button startButton = GetNode<Button>("%StartButton");
        Button settingsButton = GetNode<Button>("%SettingsButton");
        Button exitButton = GetNode<Button>("%ExitButton");

        InitButton(startButton, MainMenuState.Start);
        InitButton(settingsButton, MainMenuState.Settings);
        InitButton(exitButton, MainMenuState.Exit);

        ShowButtonBar(0.25f);
    }

    protected override MenuControlBase InstantiateMenuButtonControl(int state)
    {
        switch ((MainMenuState) state)
        {
            case MainMenuState.Start:
                return _titleControl.Instantiate<TitleControl>();

            case MainMenuState.Settings:
                return _sceneSettingsControl.Instantiate<SettingsControl>();
        }

        return null;
    }

    protected override void MenuButtonPressed(int state)
    {
        switch ((MainMenuState) state)
        {
            case MainMenuState.Start:
                {
                    CloseMenu();
                    EventHub.EmitSwitchGameState(GameState.Game);
                }
                break;

            case MainMenuState.Exit:
                {
                    GetTree().Quit();
                }
                break;
        }
    }
}
