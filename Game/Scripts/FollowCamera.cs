using Godot;
using System;

public partial class FollowCamera : Camera3D
{
	[Export] private NodePath _targetPath;
	[Export] private Vector3 _offset;
	[Export] private float _smoothing = 1;

	[Export] private float _fixedHeight = 15f;

	private Node3D _target;

	private Vector3 _currentLookAtFloor;
	private float _currentLookAtHeight;

	private bool _lookingBack;

	public override void _Ready()
	{
		_target = GetNode<Node3D>(_targetPath);
        _currentLookAtFloor = new Vector3(_target.GlobalTransform.Origin.X, 0, _target.GlobalTransform.Origin.Z) + _target.GlobalTransform.Basis.Z * 5;
		_currentLookAtHeight = _target.GlobalTransform.Origin.Y;
    }

    public override void _PhysicsProcess(double delta)
    {
		float smoothing = (float)(_smoothing * delta);
		
		Vector3 offset = _offset;
		if (State.DrivingBack)
		{
			offset.Z *= -2;
			if (!_lookingBack)
			{
				_lookingBack = true;
				smoothing = 1;
			}
		}
		else
		{
			if (_lookingBack)
			{
				_lookingBack = false;
                smoothing = 1;
			}
		}

		var targetGlobalTransform = _target.GlobalTransform.TranslatedLocal(offset);
		targetGlobalTransform.Origin.Y = _fixedHeight;

		GlobalTransform = GlobalTransform.InterpolateWith(targetGlobalTransform, smoothing);


		Vector3 lookAtFloor = new Vector3(_target.GlobalTransform.Origin.X, 0, _target.GlobalTransform.Origin.Z) + _target.GlobalTransform.Basis.Z * 5;
		_currentLookAtFloor = _currentLookAtFloor.Lerp(lookAtFloor, smoothing);

        float lookAtHeight = _target.GlobalTransform.Origin.Y;
		_currentLookAtHeight = Mathf.Lerp(_currentLookAtHeight, lookAtHeight, (float)(0.1 * delta));

		Vector3 lookAt = new Vector3(_currentLookAtFloor.X, _currentLookAtHeight, _currentLookAtFloor.Z);

        LookAt(lookAt, Vector3.Up);
    }
}
