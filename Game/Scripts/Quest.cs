using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public class Quest
{
    private static List<string> _easyQuestsOrig = new()
    {
        "Please deliver this letter to Grandma. She lives nearby.",
        "My dog needs to poo-poo. Could you go for a short walk?",
        "Can you bring this parcel to my neighbour? Please take care, it's fragile!",
        "My son forgot to take his sandwich. Please help me!",
        "Grandma needs to take her medicine. She lives nearby. Ok?",
        "I'm moving to a new apartment. Please carry down all my stuff. Extra?? No way, I'm nearly broke.",
        "That other stupid delivery service sent those packages to the wrong place. I hope you can do it.",
        "You wan't to know what's in it? Nah, can't tell you. Yes it's alive! Don't open it!"
    };

    private static List<string> _mediumQuestsOrig = new()
    {
        "I made the laundry for my son. He never has time for it and always sits in front of his computer doing God knows what. He lives a few blocks away. Thanks.",
        "Please tell my boss that I quit. I can give you his Adress.",
        "Something to deliver? Nah, I think you have me confused with someone else. But I think I know someone who has a job.",
        "Can you take me to school please? I overslept... Please! Or I'll miss my math lessons!",
        "I run a pizza service but my car broke down! Can you deliver it? Yes, you can take a bite.",
        "Here's a letter for my friend. Yes I know about E-Mail, but I'm too old to care.",
        "There's a party over there, but they ran out of beer! Imagine that!",
        "Yes, this is a cow. No more questions, please.",
        "I don't need this stuff anymore. You can dump them there."
    };

    private static List<string> _hardQuestsOrig = new()
    {
        "Pssst, listen... You take care of this and I promise, you won't regret it. But don't tell anyone.",
        "Secret, shall I tell you? Grand Master of Jedi Order am I. Important this job is.",
        "Please give this birthay present to my friend, will you?. Haven't seen him in while. Lives on the other end of the city. Ok?",
        "Go! Go! Go! The police is after me! We share the money, I promise!",
        "Arrghhh! ****** Snake!! Ahhh, bite hurts!!! Hospital.... Fast! Arrghhh!",
        "Taxi! What you're not a taxi driver? I don't care, just take the money. But you better be fast!",
        "They say you're fast. But are you *that* fast? I wanna see it!"
    };

    private static Array<string> _easyQuests = new();
    private static Array<string> _mediumQuests = new();
    private static Array<string> _hardQuests = new();

    private string _label;

    public Quest(Vector2I startCoord, Vector2I targetCoord)
    {
        StartCoord = startCoord;
        TargetCoord = targetCoord;

        GetInfo((targetCoord - startCoord).Length(), out int secs, out int money, out QuestLevel level);
        Seconds = secs;
        Money = money;
        QuestLevel = level;
    }

    public void Teardown()
    {
        QuestMarker.QueueFree();
        QuestSprite.QueueFree();
    }

    public Vector2I StartCoord { get; }
    public Vector2I TargetCoord { get; }

    public QuestMarker QuestMarker { get; set; }
    public Sprite2D QuestSprite { get; set; }

    public string Label
    {
        get
        {
            if (string.IsNullOrEmpty(_label))
            {
                _label = GetLabel();
            }

            return _label;
        }
    }

    public int Seconds { get; }
    public int Money { get; }
    public QuestLevel QuestLevel { get; }

    private void GetInfo(float distance, out int secs, out int money, out QuestLevel level)
    {
        List<int> shortMoney = new() { 50, 70, 90 };
        List<int> mediumMoney = new() { 120, 140, 160 };
        List<int> largeMoney = new() { 200, 250, 300 };



        if (distance < 12)
        {
            float support = 1f - Mathf.Clamp(State.EasyQuestsDone / 5f, 0, 1);

            secs = 20 + (int)(10 * support);
            money = shortMoney.GetRandomItem();
            level = QuestLevel.Easy;
        }
        else if (distance < 25)
        {
            float support = 1f - Mathf.Clamp(State.MediumQuestsDone / 5f, 0, 1);

            secs = 30 + (int)(15 * support);
            level = QuestLevel.Medium;
            money = mediumMoney.GetRandomItem();
        }
        else
        {
            float support = 1f - Mathf.Clamp(State.HardQuestsDone / 5f, 0, 1);

            secs = 40 + (int)(20 * support);
            level = QuestLevel.Hard;
            money = largeMoney.GetRandomItem();
        }

        if (UserSettings.EasyMode)
        {
            secs += 20;
        }
    }

    private string GetLabel()
    {
        Array<string> fromArray;
        List<string> origArray;

        switch (QuestLevel)
        {
            case QuestLevel.Easy:
                fromArray = _easyQuests;
                origArray = _easyQuestsOrig;
                break;

            case QuestLevel.Medium:
                fromArray = _mediumQuests;
                origArray = _mediumQuestsOrig;
                break;

            case QuestLevel.Hard:
                fromArray = _hardQuests;
                origArray = _hardQuestsOrig;
                break;

            default:
                return "...";
        }
                    
        if (fromArray.Count == 0)
        {
            fromArray.AddRange(origArray);
        }
        int i = GD.RandRange(0, fromArray.Count - 1);
        string label = fromArray[i];
        fromArray.RemoveAt(i);

        return label;
    }
}
