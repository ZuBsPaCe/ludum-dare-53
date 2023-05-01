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
    private Quest _currentQuest;


    private AudioStreamPlayer _slowMusic;
    private AudioStreamPlayer _mediumMusic;
    private AudioStreamPlayer _fastMusic;
    private AudioStreamPlayer _starMusic;

    private AudioStreamPlayer _currentMusic;


    public override void _Ready()
    {
        _cityMap = GetNode<CityMap>("CityMap");
        _drivingOverlay = GetNode<DrivingOverlay>("DrivingOverlay");

        _drivingOverlay.Setup(_cityMap.GetTexture());

        
        _slowMusic = GetNode<AudioStreamPlayer>("%SlowBeat");
        _mediumMusic = GetNode<AudioStreamPlayer>("%MediumBeat");
        _fastMusic = GetNode<AudioStreamPlayer>("%FastBeat");
        _starMusic = GetNode<AudioStreamPlayer>("%StarMusic");


        EventHub.Instance.SwitchLevelState += EventHub_SwitchLevelState;
        EventHub.Instance.QuestMarkerEntered += EventHub_QuestMarkerEntered;
        EventHub.Instance.QuestAccepted += EventHub_QuestAccepted;


        // Initialize LevelState StateMachine
        _levelStateMachine = _sceneStateMachine.Instantiate<StateMachine>();
        AddChild(_levelStateMachine, false, InternalMode.Front);
        _levelStateMachine.Setup(LevelState.Init, SwitchLevelState);


        State.Fuel = 65;
        State.Money = 100;
    }

    public override void _Process(double delta)
    {
        if (State.CountdownActive)
        {
            State.CountdownSecs -= (float) delta;
        }
    }

    private void EventHub_SwitchLevelState(LevelState levelState)
    {
        _levelStateMachine.SetState(levelState);
    }

    private void EventHub_QuestMarkerEntered(QuestMarker questMarker, bool isStart)
    {
        Debug.Assert(_quests.Contains(questMarker.Quest));

        if (isStart)
        {
            _drivingOverlay.ShowQuestMenu(questMarker.Quest);
        }
        else
        {
            _currentMusic?.Stop();

            State.CountdownActive = false;

            if (State.CountdownSecs > 0)
            {
                Sounds.PlaySound(SoundType.Won);
                State.Money += _currentQuest.Money;
            }
            else
            {
                Sounds.PlaySound(SoundType.Lost);
            }

            _currentQuest.Teardown();
            _currentQuest = null;

            CreateQuests();
        }
    }

    private void EventHub_QuestAccepted(QuestMarker questMarker)
    {
        Debug.Assert(_currentQuest == null);

        Quest quest = questMarker.Quest;
        quest.QuestMarker.QueueFree();
        quest.QuestSprite.QueueFree();

        foreach (Quest otherQuests in _quests)
        {
            if (otherQuests != quest)
                otherQuests.Teardown();
        }
        _quests.Clear();


        _city.AddQuestMarker(quest, quest.TargetCoord, false);

        _currentQuest = quest;

        State.CountdownSecs = quest.Seconds;
        State.CountdownActive = true;

        switch (quest.QuestLevel)
        {
            case QuestLevel.Easy:
                _currentMusic = _slowMusic;
                break;
            case QuestLevel.Medium:
                _currentMusic = _mediumMusic;
                break;
            case QuestLevel.Hard:
                _currentMusic = _fastMusic;
                break;
            default:
                _currentMusic = null;
                break;
        }

        _currentMusic?.Play();
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
                        CreateQuests();
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

    public void CreateQuests()
    {
        HashSet<TileType> validTypes = new()
        {
            TileType.Street,
            TileType.Park
        };

        for (int i = 0; i < 50; ++i)
        {
            if (_city.TryGetRandomCoord(validTypes, out Vector2I coord) &&
                _city.TryGetRandomCoord(validTypes, out Vector2I targetCoord))
            {
                Quest quest = new(coord, targetCoord);
                _quests.Add(quest);

                _city.AddQuestMarker(quest, quest.StartCoord, true);
            }
        }
    }
}