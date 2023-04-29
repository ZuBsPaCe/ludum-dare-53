using Godot;
using System;
using System.Collections.Generic;

public partial class Level : Node3D
{
    [Export] private PackedScene _sceneStateMachine;
    [Export] private PackedScene _sceneCity;

    private City _city;
    private StateMachine _levelStateMachine;


    private HashSet<TileType> _streetTileTypes = new();


    public override void _Ready()
    {
        _streetTileTypes.Add(TileType.StreetHor);
        _streetTileTypes.Add(TileType.StreetVer);
        _streetTileTypes.Add(TileType.StreetCrossing);
        
        EventHub.Instance.SwitchLevelState += EventHub_SwitchLevelState;

        // Initialize LevelState StateMachine
        _levelStateMachine = _sceneStateMachine.Instantiate<StateMachine>();
        AddChild(_levelStateMachine, false, InternalMode.Front);
        _levelStateMachine.Setup(LevelState.Init, SwitchLevelState);

        GD.Randomize();
    }

    private void EventHub_SwitchLevelState(LevelState levelState)
    {
        _levelStateMachine.SetState(levelState);
    }

    private void SwitchLevelState(StateMachine stateMachine)
    {
        var enumValue = stateMachine.GetState<LevelState>();

        switch (enumValue)
        {
            case LevelState.Init:
                {
                    if (stateMachine.Action == StateMachineAction.Start)
                    {
                        Debug.Assert(_city == null);

                        _city = _sceneCity.Instantiate<City>();
                        AddChild(_city);

                        EventHub.EmitSwitchLevelState(LevelState.Running);
                    }
                }
                break;

            case LevelState.Running:
                {
                    if (stateMachine.Action == StateMachineAction.Start)
                    {
                        for (int i = 0; i < 10; ++i)
                        {
                            if (_city.TryGetRandomCoord(_streetTileTypes, out Vector2I coord))
                            {
                                Quest quest = new(coord);
                                _city.AddQuest(quest);
                            }
                        }
                    }
                }
                break;

            default:
                {
                    Debug.Fail($"Unknown enum [{enumValue}]");

                }
                break;
        }
    }

    public void CloseGameAndFree()
    {
        QueueFree();
    }
}