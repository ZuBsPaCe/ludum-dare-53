using Godot;
using System.Collections.Generic;

public partial class Level : Node3D
{
    [Export] private PackedScene _sceneStateMachine;
    [Export] private PackedScene _sceneCity;
    [Export] private PackedScene _sceneFuelStationMenu;


    private City _city;
    private CityMap _cityMap;
    private DrivingOverlay _drivingOverlay;
    private Notification _notification;

    private StateMachine _levelStateMachine;


    private List<Quest> _quests = new();
    private Quest _currentQuest;


    private AudioStreamPlayer _slowMusic;
    private AudioStreamPlayer _mediumMusic;
    private AudioStreamPlayer _fastMusic;
    private AudioStreamPlayer _starMusic;

    private AudioStreamPlayer _currentMusic;


    private Quest _canEnterQuest;
    private bool _canEnterShop;
    private bool _enterNotificationShown;


    public override void _Ready()
    {
        State.ShopWinBought = false;
        State.LevelTime.Restart();


        _cityMap = GetNode<CityMap>("CityMap");
        _drivingOverlay = GetNode<DrivingOverlay>("DrivingOverlay");

        _drivingOverlay.Setup(_cityMap.GetTexture());

        _notification = GetNode<Notification>("Notification");

        
        _slowMusic = GetNode<AudioStreamPlayer>("%SlowBeat");
        _mediumMusic = GetNode<AudioStreamPlayer>("%MediumBeat");
        _fastMusic = GetNode<AudioStreamPlayer>("%FastBeat");
        _starMusic = GetNode<AudioStreamPlayer>("%StarMusic");


        // TODO: Must be initialized in DrivingOverlay, because ready ist called first there
        // BUT: GameEventHub must be free along with Level, otherwise it won't disconnect
        // event handlers...
        //
        //// Initialize GameEventHub Singleton
        //GameEventHub.Instance = new GameEventHub();
        //AddChild(GameEventHub.Instance, false, InternalMode.Front);


        GameEventHub.Instance.SwitchLevelState += EventHub_SwitchLevelState;
        GameEventHub.Instance.QuestMarkerEntered += EventHub_QuestMarkerEntered;
        GameEventHub.Instance.QuestMarkerExited += EventHub_QuestMarkerExited;
        GameEventHub.Instance.QuestAccepted += EventHub_QuestAccepted;

        GameEventHub.Instance.FuelMarkerEntered += EventHub_FuelMarkerEntered;
        GameEventHub.Instance.FuelMarkerExited += EventHub_FuelMarkerExited;
        GameEventHub.Instance.FuelChanged += EventHub_FuelChanged;

        GameEventHub.Instance.ShopBoughtWin += EventHub_ShopBoughtWin;


        // Initialize LevelState StateMachine
        _levelStateMachine = _sceneStateMachine.Instantiate<StateMachine>();
        AddChild(_levelStateMachine, false, InternalMode.Front);
        _levelStateMachine.Setup(LevelState.Init, SwitchLevelState);

        // Force refresh...
        State.Fuel = 1;
        State.Money = 1;

        State.Fuel = 65;
        State.Money = 100;
    }

    public override void _Process(double delta)
    {
        if (State.CountdownActive)
        {
            State.CountdownSecs -= (float) delta;
        }

        if (!_enterNotificationShown)
        {
            if (!State.OverlayActive)
            {
                if (_city.PlayerSpeed < 1)
                {
                    if (_canEnterQuest != null || _canEnterShop)
                    {
                        _notification.ShowNotification(NotificationType.Info, "Press E", true);
                        _enterNotificationShown = true;
                    }
                }
            }
        }
        else
        {
            if (_city.PlayerSpeed >= 1)
            {
                _notification.Unhold();
                _enterNotificationShown = false;
            }
            else
            {
                if (!State.OverlayActive)
                {
                    if (Input.IsActionJustPressed("Use"))
                    {
                        if (_canEnterQuest != null)
                        {
                            _notification.Unhold();
                            _enterNotificationShown = false;

                            State.OverlayActive = true;
                            _drivingOverlay.ShowQuestMenu(_canEnterQuest);
                        }
                        else if (_canEnterShop)
                        {
                            _notification.Unhold();
                            _enterNotificationShown = false;

                            State.OverlayActive = true;
                            FuelStationMenu fuelStationMenu = _sceneFuelStationMenu.Instantiate<FuelStationMenu>();
                            AddChild(fuelStationMenu);
                        }
                    }
                }
            }
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
            _canEnterQuest = questMarker.Quest;
        }
        else
        {
            _currentMusic?.Stop();

            State.CountdownActive = false;

            if (State.CountdownSecs > 0)
            {
                _notification.ShowNotification(NotificationType.Won, $"Good Job! +{_currentQuest.Money} bucks.");
                Sounds.PlaySound(SoundType.Won);
                State.Money += _currentQuest.Money;
            }
            else
            {
                _notification.ShowNotification(NotificationType.Lost, $"Too late...");
                Sounds.PlaySound(SoundType.Lost);
            }

            _currentQuest.Teardown();
            _currentQuest = null;

            CreateQuests();
        }
    }

    private void EventHub_QuestMarkerExited()
    {
        _canEnterQuest = null;
    }

    private void EventHub_FuelMarkerEntered()
    {
        _canEnterShop = true;
    }

    private void EventHub_FuelMarkerExited()
    {
        _canEnterShop = false;
    }

    private void EventHub_FuelChanged(int amount)
    {
        if (amount == 0)
            _notification.ShowNotification(NotificationType.Lost, "Ran out of fuel... Try again...");
    }

    private void EventHub_QuestAccepted(QuestMarker questMarker)
    {
        Debug.Assert(_currentQuest == null);

        _canEnterQuest = null;

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

    private void EventHub_ShopBoughtWin()
    {
        // Not used
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

                        GameEventHub.EmitSwitchLevelState(LevelState.Running);
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

        int failures = 0;
        int count = GD.RandRange(4, 10);

        while (_quests.Count < count && failures < count * 10)
        {
            if (_city.TryGetRandomCoord(validTypes, out Vector2I startCoord) &&
                _city.TryGetRandomCoord(validTypes, out Vector2I targetCoord))
            {
                float minDistance;

                Vector3 playerPos = _city.PlayerPos;
                Vector2 playerPos2 = new Vector2(playerPos.X, playerPos.Z);
                float playerDistance = (playerPos2 - startCoord * _city.TileSize).Length();
                minDistance = playerDistance;


                foreach (Quest otherQuest in _quests)
                {
                    float currentDistance = (otherQuest.StartCoord - startCoord).Length() * _city.TileSize;
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                    }
                }


                if (minDistance >= 100)
                {
                    Quest quest = new(startCoord, targetCoord);
                    _quests.Add(quest);

                    _city.AddQuestMarker(quest, quest.StartCoord, true);
                }
                else
                {
                    failures += 1;
                }
            }
        }
    }
}