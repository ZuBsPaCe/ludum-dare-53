using Godot;

public partial class MainMenu : MenuBase
{
    [Export] private PackedScene _sceneSettingsControl;

    public override void _Ready()
    {
        Control buttonBar = GetNode<MarginContainer>("%ButtonBar");
        InitButtonBar(buttonBar);
        ShowButtonBar(0.25f);

        Button startButton = GetNode<Button>("%StartButton");
        Button settingsButton = GetNode<Button>("%SettingsButton");
        Button exitButton = GetNode<Button>("%ExitButton");

        InitButton(startButton, MainMenuState.Start);
        InitButton(settingsButton, MainMenuState.Settings);
        InitButton(exitButton, MainMenuState.Exit);
    }

    protected override Control InstantiateMenuButtonControl(int state)
    {
        switch ((MainMenuState) state)
        {
            case MainMenuState.Settings:
                return _sceneSettingsControl.Instantiate<Control>();
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
