using System.Diagnostics;

public class State
{
    private static int _fuel;
    private static int _money;
    private static bool _countdownActive;
    private static float _countdownSecs;

    public static int Fuel
    {
        get { return _fuel; }
        set
        {
            int newValue = value;
            if (newValue < 0)
            {
                newValue = 0;
            }

            if (newValue != _fuel)
            {
                _fuel = newValue;
                GameEventHub.EmitFuelChanged(_fuel);
            }
        }
    }

    public static int Money
    {
        get { return _money; }
        set
        {
            int newValue = value;
            if (newValue < 0)
            {
                newValue = 0;
            }

            if (newValue != _money)
            {
                _money = newValue;
                GameEventHub.EmitMoneyChanged(_money);
            }
        }
    }

    public static bool CountdownActive
    {
        get
        {
            return _countdownActive;
        }
        set
        {

            _countdownActive = value;
            GameEventHub.EmitCountdownChanged(_countdownActive, _countdownSecs);
        }
    }

    public static float CountdownSecs
    {
        get
        {
            return _countdownSecs;
        }
        set
        {
            float newValue = value;
            if (newValue < 0)
            {
                newValue = 0;
            }

            if (newValue != _countdownSecs)
            {
                _countdownSecs = newValue;
                GameEventHub.EmitCountdownChanged(_countdownActive, _countdownSecs);
            }
        }
    }

    public static bool ShopWinBought { get; set; }
    public static Stopwatch LevelTime { get; set; } = new();

    public static bool OverlayActive { get; set; }

    public static int EasyQuestsDone { get; set; } = 0;
    public static int MediumQuestsDone { get; set; } = 0;
    public static int HardQuestsDone { get; set; } = 0;

    public static Map<TileType, Tile> Map { get; set; }
    public static int TileSize { get; set; }

    public static float StarTime { get; set; }

    public static bool GripUpgrade1 { get; set; }
    public static bool GripUpgrade2 { get; set; }

    public static bool SpeedUpgrade1 { get; set; }
    public static bool SpeedUpgrade2 { get; set; }

    public static bool TankUpgrade1 { get; set; }
    public static int TankMaxSize { get; set; }

    public static bool UsingJoypad { get; internal set; }
}
