using Godot;
using System;

public partial class FuelStationMenu : MenuBase
{
    [Export] private PackedScene _sceneSettingsControl;
    [Export] private PackedScene _fuelControl;
    [Export] private PackedScene _shopControl;

    private TextureRect _mapTextureRect;

    private Label _fuelLabel;
    private Label _moneyLabel;

    private TextureRect _countDownIcon;
    private Label _countDownLabel;

    private bool _closed;


    public override void _Ready()
    {
        _mapTextureRect = GetNode<TextureRect>("%MapTextureRect");

        Control buttonBar = GetNode<MarginContainer>("%ButtonBar");
        InitButtonBar(buttonBar);

        Button continueButton = GetNode<Button>("%ContinueButton");
        Button fuelButton = GetNode<Button>("%FuelButton");
        Button shopButton = GetNode<Button>("%ShopButton");
        Button settingsButton = GetNode<Button>("%SettingsButton");
        Button mainMenuButton = GetNode<Button>("%MainMenuButton");

        InitButton(continueButton, FuelStationMenuState.Continue);
        InitButton(fuelButton, FuelStationMenuState.Fuel);
        InitButton(shopButton, FuelStationMenuState.Shop);
        InitButton(settingsButton, FuelStationMenuState.Settings);
        InitButton(mainMenuButton, FuelStationMenuState.MainMenu);

        ShowButtonBar();
    }

    protected override Control InstantiateMenuButtonControl(int state)
    {
        switch ((FuelStationMenuState)state)
        {
            case FuelStationMenuState.Fuel:
                return _fuelControl.Instantiate<FuelControl>();

            case FuelStationMenuState.Shop:
                return _shopControl.Instantiate<ShopControl>();

            case FuelStationMenuState.Settings:
                return _sceneSettingsControl.Instantiate<Control>();
        }

        return null;
    }

    protected override void MenuButtonPressed(int state)
    {
        switch ((FuelStationMenuState)state)
        {
            case FuelStationMenuState.Continue:
                {
                    State.OverlayActive = false;
                    CloseMenu();
                }
                break;

            case FuelStationMenuState.MainMenu:
                {
                    State.OverlayActive = false;
                    CloseMenu();
                    EventHub.EmitSwitchGameState(GameState.MainMenu);
                }
                break;
        }
    }
}