using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerTruck : VehicleBody3D
{
	[Export] private float _maxForwardForce = 100;
	[Export] private float _maxBackwardForce = 75;

	[Export] private float _slowBrakeForce = 75;
	[Export] private float _fastBrakeForce = 5;

	[Export] private float _friction = 4;

	[Export] private float _starForce = 300;
	[Export] private float _starBackwardForce = 225;
	[Export] private float _starFriction = 16;

	[Export] private float _starSlowBrakeForce = 75;
	[Export] private float _starFastBrakeForce = 25;

	[Export] private float _speedUpgradeForce = 25;
	[Export] private float _frictionUpgrade = 5;

	[Export] private float _slowSteeringDeg = 45;
	[Export] private float _fastSteeringDeg = 10;
	[Export] private float _fastSteeringVelocity = 30;

	[Export] private float _steerSmoothing = 5f;

	private AudioStreamPlayer3D _motorSound;

	private float _fuelTime;

	private CpuParticles3D _exhaustParticles;

	private bool _starred = false;
	private Area3D _starArea;

    private List<VehicleWheel3D> _wheels;
	private VehicleWheel3D _wheelBackLeft;
	private VehicleWheel3D _wheelBackRight;

	private bool _updateFriction;

	private bool _drivingBack;
	private float _backPressTime;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_motorSound = GetNode<AudioStreamPlayer3D>("MotorSound");
		_exhaustParticles = GetNode<CpuParticles3D>("%ExhaustParticles");

		BodyEntered += (body) => VehicleBody_BodyEntered(body);

        _starArea = GetNode<Area3D>("StarArea");
        _starArea.BodyEntered += StarArea_BodyEntered;

		_wheels = GetChildren().Where(child => child is VehicleWheel3D).Cast<VehicleWheel3D>().ToList();
		_wheelBackLeft = GetNode<VehicleWheel3D>("WheelBackLeft");
		_wheelBackRight = GetNode<VehicleWheel3D>("WheelBackRight");

		GameEventHub.Instance.GripBought += GameEventHub_GripBought;
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

    private void GameEventHub_GripBought()
    {
		_updateFriction = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		Vector3 forwardVec = Vector3.Forward.Rotated(Vector3.Up, GlobalRotation.Y);
		Vector3 vel = LinearVelocity;
		vel.Y = 0;

		float forwardSpeed = vel.Project(forwardVec).Length();


		if (Input.IsJoyButtonPressed(0, JoyButton.A))
		{
			State.UsingJoypad = true;
		}
		
		float engineForce = 0;
		float brakeForce = 0;

		if (!State.OverlayActive)
		{
			if (State.Fuel > 0)
			{
				if (Input.IsActionPressed("Forward")) 
				{
					_drivingBack = false;

					if (!_starred)
					{
                        engineForce += _maxForwardForce;

						if (State.SpeedUpgrade1)
						{
                            engineForce += _speedUpgradeForce;
						}

						if (State.SpeedUpgrade2)
						{
                            engineForce += _speedUpgradeForce;
						}
					}
					else
					{
                        engineForce += _starForce;
					}

					_fuelTime += (float)delta;
				}

				if (Input.IsActionJustPressed("Backward") && forwardSpeed <= 0.2f)
				{
					_drivingBack = true;
				}
                else if (Input.IsActionPressed("Backward"))
				{
					if (!_drivingBack && forwardSpeed <= 0.2f)
					{
						_backPressTime += (float)delta;

						if (_backPressTime > 0.25f)
						{
							_drivingBack = true;
						}
					}
                }
				else
				{
					_drivingBack = false;
					_backPressTime = 0;
				}

				if (Input.IsActionPressed("Backward"))
				{
					if (!_drivingBack)
					{
						if (!_starred)
						{
							brakeForce += Mathf.Lerp(_slowBrakeForce, _fastBrakeForce, Mathf.Clamp(30f / LinearVelocity.Length(), 0, 1));
						}
						else
						{
							brakeForce += Mathf.Lerp(_starSlowBrakeForce, _starFastBrakeForce, Mathf.Clamp(30f / LinearVelocity.Length(), 0, 1));
						}
					}
					else if (_drivingBack)
					{
						if (!_starred)
						{
							engineForce -= _maxBackwardForce;

							if (State.SpeedUpgrade1)
							{
								engineForce -= _speedUpgradeForce;
							}

							if (State.SpeedUpgrade2)
							{
								engineForce -= _speedUpgradeForce;
							}
						}
						else
						{
							engineForce -= _starBackwardForce;
						}
					}
				}
			}
		}
		else
		{
			brakeForce = 10;
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

				_wheels.ForEach(wheel => wheel.WheelFrictionSlip = _starFriction);
			}

			State.StarTime = Mathf.Max(State.StarTime - (float)delta, 0);
		}
		else
		{
			if (_starred)
			{
				_starred = false;

				SetCollisionLayerValue(2, true);
				SetCollisionMaskValue(3, true);

				_starArea.SetCollisionMaskValue(3, false);

                _updateFriction = true;

                AxisLockAngularX = false;
                AxisLockAngularZ = false;
            }

			if (_updateFriction) 
			{
				_updateFriction = false;

				float friction = _friction;

                if (State.GripUpgrade1)
                {
                    friction += _frictionUpgrade;
                }

                if (State.GripUpgrade2)
                {
                    friction += _frictionUpgrade;
                }

                _wheels.ForEach(wheel => wheel.WheelFrictionSlip = friction);

			}
		}

		float velocity = LinearVelocity.Length();

        float steering = 0;
		float steeringDeg = Mathf.Lerp(_slowSteeringDeg, _fastSteeringDeg, Math.Clamp(velocity / _fastSteeringVelocity, 0, 1));

		if (Input.IsActionPressed("Left"))
		{
			steering += Mathf.DegToRad(steeringDeg) * Input.GetActionStrength("Left");
		}
		
		if (Input.IsActionPressed("Right"))
		{
			steering -= Mathf.DegToRad(steeringDeg) * Input.GetActionStrength("Right");
		}

		steering = Mathf.MoveToward(Steering, steering, (float) (_steerSmoothing * delta));

		EngineForce = engineForce;
		Steering = steering;

		_wheelBackLeft.Brake = brakeForce;
		_wheelBackRight.Brake = brakeForce;


		if (_fuelTime > 2.07f)
		{
			_fuelTime -= 2.07f;

            State.Fuel -= 1;
		}

		if (State.Fuel == 0 && _exhaustParticles.Emitting)
		{
			_exhaustParticles.Emitting = false;
			_motorSound.Stop();
		}
		else if (State.Fuel > 0 && !_exhaustParticles.Emitting)
		{
			_exhaustParticles.Emitting = true;
			_motorSound.Play();
		}


        _motorSound.PitchScale = Mathf.Lerp(1f, 3f, Mathf.Clamp(velocity / 30f, 0f, 1f));

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
