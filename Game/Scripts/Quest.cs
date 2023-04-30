using Godot;
using System.Collections.Generic;

public class Quest
{
    public Quest(Vector2I startCoord, Vector2I targetCoord)
    {
        StartCoord = startCoord;
        TargetCoord = targetCoord;

        Label = GetLabel((targetCoord - startCoord).Length());
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


    private string GetLabel(float distance)
    {
        List<string> shortLabels = new();
        List<string> mediumLabels = new();
        List<string> largeLabels = new();

        shortLabels.Add("Please deliver this letter to Grandma. She lives nearby.");
        shortLabels.Add("My dog needs to poo-poo. Could you go for a short walk?");
        shortLabels.Add("Can you bring this parcel to my neighbour? Please take care, it's fragile!");

        mediumLabels.Add("I made the laundry for my son. He never has time for them and always sits in front of his computer doing God knows what. He lives a few blocks away. Thanks.");
        mediumLabels.Add("Please tell my boss that I quit. I can give you his Adress.");
        mediumLabels.Add("Something to deliver? Nah, I think you have me confused with someone else. But I think I know someone who has a job.");

        largeLabels.Add("Pssst, listen... You take care of this and I promise, you won't regret it. But don't tell anyone.");
        largeLabels.Add("Secret, shall I tell you? Grand Master of Jedi Order am I. Important this job is.");
        largeLabels.Add("Please give this birthay present to my friend, will you?. Haven't seen him in while. Lives on the other end of the city. Ok?");

        if (distance < 12)
        {
            return shortLabels.GetRandomItem();
        }
        
        if (distance < 25)
        {
            return mediumLabels.GetRandomItem();
        }

        return largeLabels.GetRandomItem();
    }
}
