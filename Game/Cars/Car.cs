using Godot;
using System;
using System.Collections.Generic;

public partial class Car : VehicleBody3D
{
	private bool _hasTarget;
	private Vector3 _targetPos;

	private float _targetTime;
	private const float _interval = 0.5f;

	private float _maxSteer = (float) Mathf.DegToRad(25);

	private MeshInstance3D _debugMesh;

	private Vector2I _currentCoord = new Vector2I(-1, -1);

	private Vector3 _lastCheckMovePosition;
	private float _notMovedTime;


	public float NotMovedTime
	{
		get { return _notMovedTime; }
	}
	
	public override void _Ready()
	{
		_targetTime = GD.Randf() * _interval;
		_debugMesh = GetNode<MeshInstance3D>("DebugMesh");
		_debugMesh.Visible = false;

		_lastCheckMovePosition = GlobalPosition;
    }

	public void UpdateTarget()
	{
        _targetTime = 0;
        _hasTarget = false;

        if (!TryGetCoord(out Vector2I coord))
        {
            return;
        }

        if (!State.Map.IsType(coord, TileType.Street))
        {
            return;
        }

        Direction4 currentDir = GetDirection();
        if (!TryGetPossibleNextDir(currentDir, coord, out Direction4 nextDir))
        {
            return;
        }

        Vector2I nextCoord = coord + nextDir.ToVector();

        _targetPos = new Vector3(
            nextCoord.X * State.TileSize,
            0,
            nextCoord.Y * State.TileSize);

        Vector2I offset = nextDir.Turn(Orientation.CW).ToVector();
        _targetPos.X += offset.X * State.TileSize / 4f;
        _targetPos.Z += offset.Y * State.TileSize / 4f;

        _hasTarget = true;
    }

    public override void _PhysicsProcess(double delta)
    {
		Vector3 globalPosition = GlobalPosition;
		if ((globalPosition - _lastCheckMovePosition).Length() < 10.0)
		{
			_notMovedTime += (float) delta;
		}
		else
		{
			_notMovedTime = 0;
			_lastCheckMovePosition = globalPosition;
		}


		if (State.Map == null)
		{
			return;
		}

        if (TryGetCoord(out var currentCoord))
		{
			if (currentCoord != _currentCoord)
			{
				_currentCoord = currentCoord;
				UpdateTarget();
			}
		}


        if (!_hasTarget)
		{
			return;
		}


		{
			Vector3 forwardVec = GlobalTransform.Basis.Z;
			forwardVec.Y = 0;

			//GD.Print(forwardVec);

			Vector3 targetDir = _targetPos - GlobalPosition;
			targetDir.Y = 0;


			float steerAngle = forwardVec.SignedAngleTo(targetDir, Vector3.Up);

			float maxSteer = Mathf.Clamp(steerAngle, -_maxSteer, _maxSteer);

			Steering = maxSteer;

			EngineForce = 20;


			_debugMesh.GlobalPosition = _targetPos;
        }
    }

    private bool TryGetCoord(out Vector2I coord)
	{
		int xTile = Mathf.RoundToInt(GlobalPosition.X / State.TileSize);
		int yTile = Mathf.RoundToInt(GlobalPosition.Z / State.TileSize);

		if (State.Map.IsValid(xTile, yTile))
		{
			coord = new Vector2I(xTile, yTile);
			return true;
		}

		coord = Vector2I.Zero;
		return false;
    }

	private Direction4 GetDirection()
	{
		var degrees = Mathf.PosMod(GlobalRotationDegrees.Y, 360.0);

		if (degrees < 45)
		{
			return Direction4.S;
		}

		if (degrees < 135)
		{
			return Direction4.E;
		}

		if (degrees < 225)
		{
			return Direction4.N;
		}

		if (degrees < 315)
		{
			return Direction4.W;
		}

		return Direction4.S;
	}

	private bool TryGetPossibleNextDir(Direction4 current, Vector2I coord, out Direction4 nextDir)
	{
		List<Direction4> dirs = new();

        if (State.Map.IsTypeWithOffset(coord, current.ToVector(), TileType.Street))
        {
            dirs.Add(current);
        }

        if (State.Map.IsTypeWithOffset(coord, current.Turn(Orientation.CW).ToVector(), TileType.Street))
        {
			dirs.Add(current.Turn(Orientation.CW));
        }

        if (State.Map.IsTypeWithOffset(coord, current.Turn(Orientation.CCW).ToVector(), TileType.Street))
        {
            dirs.Add(current.Turn(Orientation.CCW));
        }

		if (dirs.Count == 0)
		{
			nextDir = current;
			return false;
		}

		nextDir = dirs.GetRandomItem();
		return true;
	}
}
