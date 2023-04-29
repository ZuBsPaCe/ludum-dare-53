using Godot;

public partial class Main : Node
{
    [Export] private PackedScene _sceneGameStateMachine;
    [Export] private PackedScene _sceneMainMenu;
    [Export] private PackedScene _sceneGame;
    [Export] private PackedScene _scenePauseMenu;

    private StateMachine _gameStateMachine;

    private Game _game;

    public override void _Ready()
    {
        // Initialize Sound Singleton
        Sounds.Instance = new Sounds();
        AddChild(Sounds.Instance, false, InternalMode.Front);


        // Initialize EventHub Singleton
        EventHub.Instance = new EventHub();
        AddChild(EventHub.Instance, false, InternalMode.Front);
        EventHub.Instance.SwitchGameState += EventHub_SwitchGameState;


        // Initialize GameState StateMachine
        _gameStateMachine = _sceneGameStateMachine.Instantiate<StateMachine>();
        AddChild(_gameStateMachine, false, InternalMode.Front);
        _gameStateMachine.Setup(GameState.MainMenu, SwitchGameState);
    }

    private void EventHub_SwitchGameState(GameState gameState)
    {
        _gameStateMachine.SetState(gameState);
    }

    private void SwitchGameState(StateMachine stateMachine)
    {
        switch (stateMachine.GetState<GameState>())
        {
            case GameState.MainMenu:
                if (stateMachine.Action == StateMachineAction.Start)
                {
                    if (_game != null)
                    {
                        _game.CloseGameAndFree();
                        _game = null;
                    }

                    GetTree().Paused = true;

                    MainMenu mainMenu = _sceneMainMenu.Instantiate<MainMenu>();
                    AddChild(mainMenu);
                }
                break;

            case GameState.Game:
                if (stateMachine.Action == StateMachineAction.Start)
                {
                    if (_game == null)
                    {
                        _game = _sceneGame.Instantiate<Game>();
                        AddChild(_game);
                    }

                    GetTree().Paused = false;
                }
                break;

            case GameState.PauseMenu:
                if (stateMachine.Action == StateMachineAction.Start)
                {
                    GetTree().Paused = true;

                    PauseMenu pauseMenu = _scenePauseMenu.Instantiate<PauseMenu>();
                    AddChild(pauseMenu);
                }
                break;
        }
    }

    public override void _UnhandledKeyInput(InputEvent ev)
    {
        if (_gameStateMachine.IsState(GameState.Game) && ev.IsPressed() && !ev.IsEcho() && ev.IsAction("Pause"))
        {
            EventHub.EmitSwitchGameState(GameState.PauseMenu);
        }
    }
}
