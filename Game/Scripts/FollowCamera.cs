using Godot;
using System;

public partial class FollowCamera : Camera3D
{
	[Export] private NodePath _targetPath;
	[Export] private Vector3 _offset;
	[Export] private float _smoothing = 1;


	private Node3D _target;

	public override void _Ready()
	{
		_target = GetNode<Node3D>(_targetPath);
	}

    public override void _PhysicsProcess(double delta)
    {
		var targetGlobalTransform = _target.GlobalTransform.TranslatedLocal(_offset);
		GlobalTransform = GlobalTransform.InterpolateWith(targetGlobalTransform, (float)(_smoothing * delta));

		LookAt(_target.GlobalTransform.Origin, Vector3.Up);
    }
}
