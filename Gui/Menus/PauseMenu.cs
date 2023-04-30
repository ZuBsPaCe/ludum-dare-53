using Godot;

public partial class PauseMenu : MenuBase
{
    [Export] private PackedScene _sceneSettingsControl;

    public override void _Ready()
    {
        Control buttonBar = GetNode<MarginContainer>("%ButtonBar");

        InitButtonBar(buttonBar);
        ShowButtonBar();

        Button continueButton = GetNode<Button>("%ContinueButton");
        Button settingsButton = GetNode<Button>("%SettingsButton");
        Button mainMenuButton = GetNode<Button>("%MainMenuButton");

        InitButton(continueButton, PauseMenuState.Continue);
        InitButton(settingsButton, PauseMenuState.Settings);
        InitButton(mainMenuButton, PauseMenuState.MainMenu);
    }

    protected override Control InstantiateMenuButtonControl(int state)
    {
        switch ((PauseMenuState)state)
        {
            case PauseMenuState.Settings:
                return _sceneSettingsControl.Instantiate<Control>();
        }

        return null;
    }

    protected override void MenuButtonPressed(int state)
    {
        switch ((PauseMenuState)state)
        {
            case PauseMenuState.Continue:
                {
                    CloseMenu();
                    EventHub.EmitSwitchGameState(GameState.Game);
                }
                break;

            case PauseMenuState.MainMenu:
                {
                    CloseMenu();
                    EventHub.EmitSwitchGameState(GameState.MainMenu);
                }
                break;
        }
    }

    public override void _Input(InputEvent ev)
    {
        if (ev.IsPressed() && !ev.IsEcho() && ev.IsAction("Pause"))
        {
            CloseMenu();
            EventHub.EmitSwitchGameState(GameState.Game);
        }
    }
}
