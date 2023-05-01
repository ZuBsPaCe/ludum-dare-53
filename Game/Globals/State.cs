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
                EventHub.EmitFuelChanged(_fuel);
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
                EventHub.EmitMoneyChanged(_money);
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
            EventHub.EmitCountdownChanged(_countdownActive, _countdownSecs);
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
                EventHub.EmitCountdownChanged(_countdownActive, _countdownSecs);
            }
        }
    }


    public static bool QuestOverlayActive { get; set; }
}
