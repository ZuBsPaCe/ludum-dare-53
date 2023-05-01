using Godot;
using System;

public partial class QuestMarker : Node3D
{
    private Quest _quest;
	private MeshInstance3D _meshInstance;


	public Quest Quest
	{
		get { return _quest; }
	}

	public bool IsStart { get; private set; }

    public override void _Ready()
	{
		Area3D area = GetNode<Area3D>("Area3D");
		area.BodyEntered += (body) => Area_BodyEntered(body);
		area.BodyExited += (body) => Area_BodyExited(body);

		_meshInstance = GetNode<MeshInstance3D>("Cylinder");
	}

    public void Setup(Quest quest, bool isStart, Material material)
	{
        _quest = quest;
		IsStart = isStart;

		_meshInstance.MaterialOverride = material;
	}

    private void Area_BodyEntered(Node3D body)
    {
		if (body.IsInGroup(Group.Player))
		{
            GameEventHub.EmitQuestMarkerEntered(this, IsStart);
		}
    }

    private void Area_BodyExited(Node3D body)
    {
		if (body.IsInGroup(Group.Player))
		{
            GameEventHub.EmitQuestMarkerExited();
		}
    }
}
