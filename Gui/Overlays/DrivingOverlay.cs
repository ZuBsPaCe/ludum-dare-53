using Godot;
using System;

public partial class DrivingOverlay : MenuBase
{
    [Export] private PackedScene _sceneSettingsControl;
    [Export] private PackedScene _sceneQuestControl;

    private TextureRect _mapTextureRect;

    private Button _questInfoButton;

    private Quest _quest;

    private Label _fuelLabel;
    private Label _moneyLabel;

    private TextureRect _countDownIcon;
    private Label _countDownLabel;


    public override void _Ready()
    {
        State.QuestOverlayActive = false;

        _mapTextureRect = GetNode<TextureRect>("%MapTextureRect");



        Control buttonBar = GetNode<MarginContainer>("%ButtonBar");
        InitButtonBar(buttonBar);

        _questInfoButton = GetNode<Button>("%QuestInfoButton");
        Button acceptButton = GetNode<Button>("%AcceptButton");
        Button denyButton = GetNode<Button>("%DenyButton");
        Button settingsButton = GetNode<Button>("%SettingsButton");
        Button mainMenuButton = GetNode<Button>("%MainMenuButton");

        InitButton(_questInfoButton, IngameMenuState.QuestInfo);
        InitButton(acceptButton, IngameMenuState.Accept);
        InitButton(denyButton, IngameMenuState.Deny);
        InitButton(settingsButton, IngameMenuState.Settings);
        InitButton(mainMenuButton, IngameMenuState.MainMenu);

        _moneyLabel = GetNode<Label>("%MoneyLabel");
        _fuelLabel = GetNode<Label>("%FuelLabel");

        _countDownIcon = GetNode<TextureRect>("%CountdownIcon");
        _countDownLabel = GetNode<Label>("%CountdownLabel");

        _countDownIcon.Visible = false;
        _countDownLabel.Visible = false;

        EventHub.Instance.FuelChanged += (amount) => EventHub_FuelChanged(amount);
        EventHub.Instance.MoneyChanged += (amount) => EventHub_MoneyChanged(amount);
        EventHub.Instance.CountdownChanged += (active, secs) => EventHub_CountdownChanged(active, secs);
    }

    public void Setup(ViewportTexture _cityMapTex)
    {
        _mapTextureRect.Texture = _cityMapTex;
    }

    public void ShowQuestMenu(Quest quest)
    {
        if (_quest != null)
        {
            return;
        }

        State.QuestOverlayActive = true;

        _quest = quest;
        ShowButtonBar();
        SetCurrentButton(_questInfoButton);
    }

    private void EventHub_MoneyChanged(int amount)
    {
        _moneyLabel.Text = amount.ToString();
    }

    private void EventHub_FuelChanged(int amount)
    {
        _fuelLabel.Text = amount.ToString();
    }

    private void EventHub_CountdownChanged(bool active, float secs)
    {
        _countDownIcon.Visible = active;
        _countDownLabel.Visible = active;

        _countDownLabel.Text = Mathf.CeilToInt(secs).ToString() + "s";
    }

    protected override Control InstantiateMenuButtonControl(int state)
    {
        switch ((IngameMenuState)state)
        {
            case IngameMenuState.QuestInfo:
                var questControl = _sceneQuestControl.Instantiate<QuestControl>();
                questControl.Setup(_quest);
                return questControl;

            case IngameMenuState.Settings:
                return _sceneSettingsControl.Instantiate<Control>();
        }

        return null;
    }

    protected override void MenuButtonPressed(int state)
    {
        switch ((IngameMenuState)state)
        {
            case IngameMenuState.Accept:
                {
                    State.QuestOverlayActive = false;
                    CloseMenu(false);
                    EventHub.EmitQuestAccepted(_quest.QuestMarker);
                    _quest = null;
                }
                break;

            case IngameMenuState.Deny:
                {
                    State.QuestOverlayActive = false;
                    CloseMenu(false);
                    _quest = null;
                }
                break;

            case IngameMenuState.MainMenu:
                {
                    State.QuestOverlayActive = false;
                    CloseMenu();
                    EventHub.EmitSwitchGameState(GameState.MainMenu);
                }
                break;
        }
    }

    //public override void _Input(InputEvent ev)
    //{
    //    if (ev.IsPressed() && !ev.IsEcho() && ev.IsAction("Pause"))
    //    {
    //        CloseMenu(false);
    //        EventHub.EmitSwitchGameState(GameState.Game);
    //    }
    //}
}
