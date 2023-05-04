using Godot;
using System.Collections;
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

    private float _flipTime;
    private bool _flipped;
    private bool _forceResetTruck;

    private bool _ranOutOfFuel;
    private float _ranOutOfFuelTimer;

    private const float _markerEnterSpeed = 5f;

    public override void _Ready()
    {
        State.ShopWinBought = false;
        State.LevelTime.Restart();
        State.EasyQuestsDone = 0;
        State.MediumQuestsDone = 0;
        State.HardQuestsDone = 0;
        State.CountdownActive = false;
        State.CountdownSecs = 0;
        State.StarTime = 0;
        State.GripUpgrade1 = false;
        State.GripUpgrade2 = false;
        State.SpeedUpgrade1 = false;
        State.SpeedUpgrade2 = false;
        State.TankUpgrade1 = false;
        State.TankMaxSize = 65;


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

		GameEventHub.Instance.StarPickedUp += GameEventHub_StarPickedUp;
		GameEventHub.Instance.StarDone += GameEventHub_StarDone;

        GameEventHub.Instance.ResetTruck += GameEventHub_ResetTruck;


        // Initialize LevelState StateMachine
        _levelStateMachine = _sceneStateMachine.Instantiate<StateMachine>();
        AddChild(_levelStateMachine, false, InternalMode.Front);
        _levelStateMachine.Setup(LevelState.Init, SwitchLevelState);

        // Force refresh...
        State.Fuel = 1;
        State.Money = 1;

        State.Fuel = State.TankMaxSize;
        State.Money = 50;
    }

    public override void _Process(double delta)
    {
        bool resetTruck = false;

        if (_forceResetTruck)
        {
            resetTruck = true;
            _forceResetTruck = false;
        }
        else if (_city.PlayerPos.Y < -20)
        {
            resetTruck = true;
        }
        else
        {
            if (_city.TruckIsFlipped && _city.PlayerSpeed < 5)
            {
                _flipTime += (float) delta;

                if (!_flipped && _flipTime >= 5) 
                {
                    _flipped = true;

                    string key = State.UsingJoypad ? "X" : "E";
                    _notification.ShowNotification(NotificationType.Info, $"Press {key} to reset your truck", true);
                }

                if (_flipped && Input.IsActionJustPressed("Use"))
                {
                    resetTruck = true;
                }
            }
            else
            {
                if (_flipped)
                {
                    _flipped = false;
                    _notification.Unhold();
                }

                _flipTime = 0;
            }
        }

        if (resetTruck)
        {
            _city.ResetPlayerTruck();
            _flipTime = 0;
            _flipped = false;

            int repairCost;
            if (State.Money <= 100)
            {
                repairCost = 20;
            }
            else if (State.Money <= 300)
            {
                repairCost = 40;
            }
            else
            {
                repairCost = 60;
            }

            string message = $"Repairs: -{repairCost} bucks";

            if (State.Money == 0)
            {
                List<string> msg = new()
                {
                    "Again?!",
                    "You should look for another job...",
                    "You ever heard of brakes?",
                    "Not again...",
                    "Don't drink while driving...",
                    "Don't text while driving..."
                };

                message = msg.GetRandomItem();
            }

            _notification.Unhold();
            _notification.ShowNotification(NotificationType.Lost, message);
            State.Money -= repairCost;

            return;
        }

        if (State.Fuel == 0 && !State.OverlayActive)
        {
            if (_city.PlayerSpeed < 5)
            {
                _ranOutOfFuelTimer += (float)delta;
            }
            else
            {
                _ranOutOfFuelTimer = 0;
            }

            if (!_ranOutOfFuel && _ranOutOfFuelTimer > 1.5f)
            {
                _ranOutOfFuel = true;

                string key = State.UsingJoypad ? "Start" : "Esc";
                _notification.ShowNotification(NotificationType.Lost, $"Ran out of fuel... Press {key} and try again...", true);
            }
        }
        else
        {
            _ranOutOfFuelTimer = 0;

            if (_ranOutOfFuel && State.Fuel > 0)
            {
                _ranOutOfFuel = false;
                _notification.Unhold();
            }
        }


        if (State.CountdownActive)
        {
            State.CountdownSecs -= (float) delta;
        }

        if (!_enterNotificationShown)
        {
            if (!State.OverlayActive)
            {
                if (_city.PlayerSpeed < _markerEnterSpeed)
                {
                    if (_canEnterQuest != null || _canEnterShop)
                    {
                        string key = State.UsingJoypad ? "X" : "E";
                        _notification.ShowNotification(NotificationType.Info, $"Press {key}", true);
                        _enterNotificationShown = true;
                    }
                }
            }
        }
        else
        {
            if (_city.PlayerSpeed >= _markerEnterSpeed)
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

                switch (_currentQuest.QuestLevel)
                {
                    case QuestLevel.Easy:
                        State.EasyQuestsDone += 1;
                        break;
                    case QuestLevel.Medium:
                        State.MediumQuestsDone += 1;
                        break;
                    case QuestLevel.Hard:
                        State.HardQuestsDone += 1;
                        break;
                    default:
                        Debug.Fail();
                        break;
                }

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
        // unused
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

        PlayQuestMusic();
    }

    private void PlayQuestMusic()
    {
        if (_currentQuest == null)
        {
            return;
        }

        switch (_currentQuest.QuestLevel)
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

        if (!_starMusic.Playing)
        {
            _currentMusic?.Play();
        }
    }

    private void EventHub_ShopBoughtWin()
    {
        // Not used
    }

    private void GameEventHub_StarPickedUp()
	{
        _currentMusic?.Stop();
        _currentMusic = null;

        _starMusic.Stop();
        _starMusic.Play();

        State.StarTime = 20;

        _city.CreateStar();
    }

    private void GameEventHub_StarDone()
    {
        _starMusic.Stop();
        PlayQuestMusic();
    }

    private void GameEventHub_ResetTruck()
    {
        _forceResetTruck = true;
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