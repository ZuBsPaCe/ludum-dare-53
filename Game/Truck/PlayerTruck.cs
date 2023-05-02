using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerTruck : VehicleBody3D
{
	[Export] private float _maxForwardForce = 100;
	[Export] private float _maxBackwardForce = 75;

	[Export] private float _slowSteeringDeg = 45;
	[Export] private float _fastSteeringDeg = 15;
	[Export] private float _fastSteeringVelocity = 30;

	[Export] private float _steerSmoothing = 5f;

	private AudioStreamPlayer3D _motorSound;

	private float _fuelTime;

	private CpuParticles3D _exhaustParticles;

	private bool _starred = false;
	private Area3D _starArea;

    private List<VehicleWheel3D> _wheels;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_motorSound = GetNode<AudioStreamPlayer3D>("MotorSound");
		_exhaustParticles = GetNode<CpuParticles3D>("%ExhaustParticles");

		BodyEntered += (body) => VehicleBody_BodyEntered(body);

        _starArea = GetNode<Area3D>("StarArea");
        _starArea.BodyEntered += StarArea_BodyEntered;

		_wheels = GetChildren().Where(child => child is VehicleWheel3D).Cast<VehicleWheel3D>().ToList();
    }

    private void VehicleBody_BodyEntered(Node body)
    {
		if (body.IsInGroup("Car"))
		{
            Sounds.PlaySound(SoundType.Crash);

			ApplyStarToCar(body);
		}
		else
		{
			Sounds.PlaySound(SoundType.ObjectHit);
		}
    }

	private void StarArea_BodyEntered(Node body)
	{
		if (body.IsInGroup("Car"))
		{
			Sounds.PlaySound(SoundType.Crash);
			ApplyStarToCar(body);
		}
    }

	private bool ApplyStarToCar(Node body)
	{
        if (_starred)
        {
            if (body.IsInGroup("Car"))
            {
                Car car = (Car)body;

                var dir = car.GlobalPosition - GlobalPosition;
                dir.Y = 0;
                dir = dir.Normalized();
                dir.Y += 0.4f + GD.Randf() * 0.2f;

                car.ApplyImpulse(dir.Normalized() * (1000f + GD.Randf() * 200f));
				return true;
            }
        }

		return false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		float engineForce = 0;

		if (!State.OverlayActive)
		{
			if (State.Fuel > 0)
			{
				if (Input.IsActionPressed("Forward"))
				{
					engineForce += _maxForwardForce;

					_fuelTime += (float)delta;
				}

				if (Input.IsActionPressed("Backward"))
				{
					engineForce -= _maxBackwardForce;

					_fuelTime += (float)delta;
				}
			}
		}

		if (State.StarTime > 0)
		{
			if (!_starred)
			{
				_starred = true;

				// Disabling collision with cars (we're on layer 2, cars on layer 3). We detect them with an area instead.
				// This way, we don't lose momentum, when we run into them.

				SetCollisionLayerValue(2, false);
				SetCollisionMaskValue(3, false);

                _starArea.SetCollisionMaskValue(3, true);

                // Point up vector straight up. Align forward and side vector.
                // And then axis lock them.

                var transform = Transform;
				var basis = transform.Basis;

				var up = Vector3.Up;
				var side = basis.X;

				var forward = side.Cross(Vector3.Up);
				side = up.Cross(forward);

				basis.Z = forward;
				basis.X = side;
				basis.Y = up;

				transform.Basis = basis;
				Transform = transform;


				AxisLockAngularX = true;
				AxisLockAngularZ = true;

				_wheels.ForEach(wheel => wheel.WheelFrictionSlip = 16);
			}

			State.StarTime = Mathf.Max(State.StarTime - (float)delta, 0);
            engineForce *= 3;
		}
		else
		{
			if (_starred)
			{
				_starred = false;

				SetCollisionLayerValue(2, true);
				SetCollisionMaskValue(3, true);

                _starArea.SetCollisionMaskValue(3, false);

				_wheels.ForEach(wheel => wheel.WheelFrictionSlip = 4);

				AxisLockAngularX = false;
				AxisLockAngularZ = false;
			}
		}

		float velocity = LinearVelocity.Length();

        float steering = 0;
		float steeringDeg = Mathf.Lerp(_slowSteeringDeg, _fastSteeringDeg, Math.Clamp(velocity / _fastSteeringVelocity, 0, 1));

		if (Input.IsActionPressed("Left"))
		{
			steering += Mathf.DegToRad(steeringDeg);
		}
		
		if (Input.IsActionPressed("Right"))
		{
			steering -= Mathf.DegToRad(steeringDeg);
		}

		steering = Mathf.MoveToward(Steering, steering, (float) (_steerSmoothing * delta));

		EngineForce = engineForce;
		Steering = steering;


		_motorSound.PitchScale = Mathf.Lerp(1f, 3f, Mathf.Clamp(velocity / 30f, 0f, 1f));


		if (_fuelTime > 2.07f)
		{
			_fuelTime -= 2.07f;

            State.Fuel -= 1;
		}

		if (State.Fuel == 0 && _exhaustParticles.Emitting)
		{
			_exhaustParticles.Emitting = false;
		}
		else if (State.Fuel > 0 && !_exhaustParticles.Emitting)
		{
			_exhaustParticles.Emitting = true;
		}

		//var rot = Transform;
		//rot = rot.RotatedLocal(Vector3.Right, (float)GetPhysicsProcessDeltaTime() * Mathf.Tau * 0.25f);
		//rot = rot.RotatedLocal(Vector3.Forward, (float)GetPhysicsProcessDeltaTime() * Mathf.Tau * 0.2f);
		//rot = rot.RotatedLocal(Vector3.Up, (float)GetPhysicsProcessDeltaTime() * Mathf.Tau * 0.15f);
		//      Transform = rot;

		//GravityScale = 0;
		//GlobalPosition = new Vector3(GlobalPosition.X, 12, GlobalPosition.Z);

		//if (State.StarTime > 0)
		//{
		

		//	State.StarTime = 0;
		//}
	}

  //  public override void _IntegrateForces(PhysicsDirectBodyState3D state)
  //  {
		//return;
		////if (State.StarTime > 0)
		//{
		//	var transform = Transform;
		//	var basis = transform.Basis;

		//	var sideVec = basis.X;
		//	sideVec.Y = 0;
		//	sideVec = sideVec.Normalized();

		//	var forwardVec = basis.Z;
		//	forwardVec.Y = 0;
		//	forwardVec = forwardVec.Normalized();

		//	var upVec = forwardVec.Cross(sideVec);

		//	GD.Print(upVec);

		//	transform.Basis.X = sideVec;
		//	transform.Basis.Y = upVec;
		//	transform.Basis.Z = forwardVec;

		//	Transform = transform;
		//}
  //  }
}
