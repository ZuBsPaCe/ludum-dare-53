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
    private Tween _fuelBlinkTween;

    private Label _moneyLabel;

    private TextureRect _countDownIcon;
    private Label _countDownLabel;
    private Tween _countDownBlinkTween;

    private bool _closed;


    public override void _Ready()
    {
        State.OverlayActive = false;

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

        // TODO: Should be initialized in Level
        // Initialize GameEventHub Singleton
        GameEventHub.Instance = new GameEventHub();
        AddChild(GameEventHub.Instance, false, InternalMode.Front);


        GameEventHub.Instance.FuelChanged += (amount) => EventHub_FuelChanged(amount);
        GameEventHub.Instance.MoneyChanged += (amount) => EventHub_MoneyChanged(amount);
        GameEventHub.Instance.CountdownChanged += (active, secs) => EventHub_CountdownChanged(active, secs);
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

        _quest = quest;
        ShowButtonBar();
    }

    private void EventHub_MoneyChanged(int amount)
    {
        _moneyLabel.Text = amount.ToString();
    }

    private void EventHub_FuelChanged(int amount)
    {
        _fuelLabel.Text = amount.ToString();

        if (amount <= 10 && _fuelBlinkTween == null)
        {
            _fuelBlinkTween = _fuelLabel.CreateTween();
            _fuelBlinkTween.SetLoops();
            _fuelBlinkTween.TweenProperty(_fuelLabel, "modulate", Colors.Red, 0.1f);
            _fuelBlinkTween.TweenInterval(0.4f);
            _fuelBlinkTween.TweenProperty(_fuelLabel, "modulate", Colors.White, 0.1f);
            _fuelBlinkTween.TweenInterval(0.4f);
        }
        else if (amount > 10 && _fuelBlinkTween != null)
        {
            _fuelBlinkTween.Kill();
            _fuelBlinkTween = null;

            _fuelLabel.Modulate = Colors.White;
        }
    }

    private void EventHub_CountdownChanged(bool active, float secs)
    {
        _countDownIcon.Visible = active;
        _countDownLabel.Visible = active;

        _countDownLabel.Text = Mathf.CeilToInt(secs).ToString();

        if (secs == 0)
        {
            if (active)
            {
                _countDownBlinkTween.Kill();
                _countDownLabel.Modulate = Colors.Red;
            }
        }
        else if (secs <= 10 && _countDownBlinkTween == null)
        {
            _countDownBlinkTween = _countDownLabel.CreateTween();
            _countDownBlinkTween.SetLoops();
            _countDownBlinkTween.TweenProperty(_countDownLabel, "modulate", Colors.Red, 0.1f);
            _countDownBlinkTween.TweenInterval(0.4f);
            _countDownBlinkTween.TweenProperty(_countDownLabel, "modulate", Colors.White, 0.1f);
            _countDownBlinkTween.TweenInterval(0.4f);
        }
        else if (secs > 10 && _countDownBlinkTween != null)
        {
            _countDownBlinkTween.Kill();
            _countDownBlinkTween = null;

            _countDownLabel.Modulate = Colors.White;
        }
    }

    protected override MenuControlBase InstantiateMenuButtonControl(int state)
    {
        switch ((IngameMenuState)state)
        {
            case IngameMenuState.QuestInfo:
                var questControl = _sceneQuestControl.Instantiate<QuestControl>();
                questControl.Setup(_quest);
                return questControl;

            case IngameMenuState.Settings:
                return _sceneSettingsControl.Instantiate<SettingsControl>();
        }

        return null;
    }

    protected override void MenuButtonPressed(int state)
    {
        switch ((IngameMenuState)state)
        {
            case IngameMenuState.Accept:
                {
                    State.OverlayActive = false;
                    CloseMenu(false);
                    GameEventHub.EmitQuestAccepted(_quest.QuestMarker);
                    _quest = null;
                }
                break;

            case IngameMenuState.Deny:
                {
                    State.OverlayActive = false;
                    CloseMenu(false);
                    _quest = null;
                }
                break;

            case IngameMenuState.MainMenu:
                {
                    State.OverlayActive = false;
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
