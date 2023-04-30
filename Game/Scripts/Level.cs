using Godot;
using System;
using System.Collections.Generic;

public partial class Level : Node3D
{
    [Export] private PackedScene _sceneStateMachine;
    [Export] private PackedScene _sceneCity;


    private City _city;
    private CityMap _cityMap;
    private DrivingOverlay _drivingOverlay;

    private StateMachine _levelStateMachine;


    private List<Quest> _quests = new();


    public override void _Ready()
    {
        _cityMap = GetNode<CityMap>("CityMap");
        _drivingOverlay = GetNode<DrivingOverlay>("DrivingOverlay");

        _drivingOverlay.Setup(_cityMap.GetTexture());

        EventHub.Instance.SwitchLevelState += EventHub_SwitchLevelState;
        EventHub.Instance.QuestMarkerEntered += EventHub_QuestMarkerEntered;


        // Initialize LevelState StateMachine
        _levelStateMachine = _sceneStateMachine.Instantiate<StateMachine>();
        AddChild(_levelStateMachine, false, InternalMode.Front);
        _levelStateMachine.Setup(LevelState.Init, SwitchLevelState);

    }

    private void EventHub_SwitchLevelState(LevelState levelState)
    {
        _levelStateMachine.SetState(levelState);
    }

    private void EventHub_QuestMarkerEntered(QuestMarker questMarker)
    {
        Debug.Assert(_quests.Contains(questMarker.Quest));

        _quests.Remove(questMarker.Quest);
        questMarker.Quest.Teardown();

        CreateQuest();
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

                        _city.Setup(_cityMap);


                        EventHub.EmitSwitchLevelState(LevelState.Running);
                    }
                }
                break;

            case LevelState.Running:
                {
                    if (stateMachine.Action == StateMachineAction.Start)
                    {
                        CreateQuest();
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

    public void CreateQuest()
    {
        if (_city.TryGetRandomCoord(TileType.Street, out Vector2I coord))
        {
            Quest quest = new(coord);
            _quests.Add(quest);

            _city.AddQuest(quest);
        }
    }
}