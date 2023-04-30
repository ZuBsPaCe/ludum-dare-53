using Godot;

public partial class DrivingOverlay : MenuBase
{
    [Export] private PackedScene _sceneSettingsControl;
    [Export] private PackedScene _sceneQuestControl;

    private TextureRect _mapTextureRect;

    private Button _questInfoButton;

    private Quest _quest;

    public override void _Ready()
    {
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
        SetCurrentButton(_questInfoButton);
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
                    CloseMenu(false);
                    EventHub.EmitQuestAccepted(_quest.QuestMarker);
                    _quest = null;
                }
                break;

            case IngameMenuState.Deny:
                {
                    CloseMenu(false);
                    _quest = null;
                }
                break;

            case IngameMenuState.MainMenu:
                {
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
