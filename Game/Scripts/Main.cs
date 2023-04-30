using Godot;
using System;

public partial class Main : Node
{
    [Export] private PackedScene _sceneStateMachine;
    [Export] private PackedScene _sceneMainMenu;
    [Export] private PackedScene _sceneLevel;
    [Export] private PackedScene _scenePauseMenu;

    private StateMachine _gameStateMachine;

    private Level _game;

    public override void _Ready()
    {
        // Initialize Sound Singleton
        Sounds.Instance = new Sounds();
        AddChild(Sounds.Instance, false, InternalMode.Front);


        // Initialize EventHub Singleton
        EventHub.Instance = new EventHub();
        AddChild(EventHub.Instance, false, InternalMode.Front);
        EventHub.Instance.SwitchGameState += EventHub_SwitchGameState;
        EventHub.Instance.MusicVolumeChanged += EventHub_MusicVolumeChanged;
        EventHub.Instance.SoundVolumeChanged += EventHub_SoundVolumeChanged;
        EventHub.Instance.FullscreenChanged += EventHub_FullscreenChanged;


        // Initialize GameState StateMachine
        _gameStateMachine = _sceneStateMachine.Instantiate<StateMachine>();
        AddChild(_gameStateMachine, false, InternalMode.Front);
        _gameStateMachine.Setup(GameState.MainMenu, SwitchGameState);


        // Initialize UserSettings Singleton
        UserSettings.Instance = new UserSettings();
        AddChild(Sounds.Instance, false, InternalMode.Front);

        UserSettings.Load();
    }

    private void EventHub_SwitchGameState(GameState gameState)
    {
        _gameStateMachine.SetState(gameState);
    }

    private void EventHub_FullscreenChanged(bool fullscreen)
    {
        if (fullscreen)
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        else
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
    }

    private void EventHub_SoundVolumeChanged(float volume)
    {
        Sounds.SetVolume("Motor", volume);
        Sounds.SetVolume("Sound", volume);
    }

    private void EventHub_MusicVolumeChanged(float volume)
    {
        Sounds.SetVolume("Music", volume);
    }

    private void SwitchGameState(StateMachine stateMachine)
    {
        var enumValue = stateMachine.GetState<GameState>();

        switch (enumValue)
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
                        _game = _sceneLevel.Instantiate<Level>();
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

            default:
                {
                    Debug.Fail($"Unknown enum [{enumValue}]");

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
