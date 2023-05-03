using Godot;
using System.Collections.Generic;

public class Quest
{
    public static List<string> _easyQuests = new()
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

    public static List<string> _mediumQuests = new()
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

    public static List<string> _hardQuests = new()
    {
        "Pssst, listen... You take care of this and I promise, you won't regret it. But don't tell anyone.",
        "Secret, shall I tell you? Grand Master of Jedi Order am I. Important this job is.",
        "Please give this birthay present to my friend, will you?. Haven't seen him in while. Lives on the other end of the city. Ok?",
        "Go! Go! Go! The police is after me! We share the money, I promise!",
        "Arrghhh! ****** Snake!! Ahhh, bite hurts!!! Hospital.... Fast! Arrghhh!",
        "Taxi! What you're not a taxi driver? I don't care, just take the money. But you better be fast!",
        "They say you're fast. But are you *that* fast? I wanna see it!"
    };

    public Quest(Vector2I startCoord, Vector2I targetCoord)
    {
        StartCoord = startCoord;
        TargetCoord = targetCoord;

        Label = GetLabel((targetCoord - startCoord).Length(), out int secs, out int money, out QuestLevel level);
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

    public string Label { get; }
    public int Seconds { get; }
    public int Money { get; }
    public QuestLevel QuestLevel { get; }

    private string GetLabel(float distance, out int secs, out int money, out QuestLevel level)
    {
        List<int> shortMoney = new() { 30, 40, 50, 80 };
        List<int> mediumMoney = new() { 80, 100, 120 };
        List<int> largeMoney = new() { 150, 200, 250 };


        float support = 1f - Mathf.Clamp(State.QuestsDone / 8f, 0, 1);

        if (distance < 12)
        {
            secs = 20 + (int)(30 * support);
            money = shortMoney.GetRandomItem();
            level = QuestLevel.Easy;
            return _easyQuests.GetRandomItem();
        }
        
        if (distance < 25)
        {
            secs = 30 + (int)(30 * support);
            level = QuestLevel.Medium;
            money = mediumMoney.GetRandomItem();
            return _mediumQuests.GetRandomItem();
        }

        secs = 40 + (int)(30 * support);
        level = QuestLevel.Hard;
        money = largeMoney.GetRandomItem();
        return _hardQuests.GetRandomItem();
    }
}
