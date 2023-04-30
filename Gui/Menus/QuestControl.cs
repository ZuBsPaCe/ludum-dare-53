using Godot;
using System;

public partial class QuestControl : MarginContainer
{
	private Label _questLabel;
	private Quest _quest;

	public override void _Ready()
	{
		Debug.Assert(_quest != null);

		_questLabel = GetNode<Label>("%QuestLabel");
		_questLabel.Text = _quest.Label;
	}

	public void Setup(Quest quest)
	{
		_quest = quest;
	}
}
