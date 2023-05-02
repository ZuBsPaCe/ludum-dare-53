using Godot;
using System;

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


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_motorSound = GetNode<AudioStreamPlayer3D>("MotorSound");
		_exhaustParticles = GetNode<CpuParticles3D>("%ExhaustParticles");

		BodyEntered += (body) => AnotherBodyEntered(body);
    }

    private void AnotherBodyEntered(Node body)
    {
        Sounds.PlaySound(SoundType.Crash);
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
			State.StarTime = Mathf.Max(State.StarTime - (float)delta, 0);
			engineForce *= 3;
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
	}
}
