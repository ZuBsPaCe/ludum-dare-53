using Godot;

public partial class QuestMarker : Node3D
{
    private Quest _quest;


	public Quest Quest
	{
		get { return _quest; }
	}

	public bool IsStart { get; private set; }

    public override void _Ready()
	{
		Area3D area = GetNode<Area3D>("Area3D");
		area.BodyEntered += (body) => Area_BodyEntered(body);
	}

	public void Setup(Quest quest, bool isStart)
	{
        _quest = quest;
		IsStart = isStart;
	}

    private void Area_BodyEntered(Node3D body)
    {
		if (body.IsInGroup(Group.Player))
		{
			EventHub.EmitQuestMarkerEntered(this, IsStart);
		}
    }
}
