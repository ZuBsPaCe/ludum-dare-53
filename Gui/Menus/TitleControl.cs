using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class TitleControl : MarginContainer
{
	[Export] Texture2D _cloneTex;


	private Sprite2D _title1;
	private Vector2 _startPos;

	private Vector2 _targetPos;
	private Vector2 _velocity;

	private const float _maxSpeed = 2000;
	private const float _accelleration = 100;
	private const float _offset = 300;

	private const int _cloneCount = 15;
	private const int _timeShift = 5;
	private const int _totalTimeShift = (_cloneCount + 1) * _timeShift;
	private List<Sprite2D> _clones = new();

	private List<Vector2> _history = new();
	private List<Color> _colorHistory = new();
	private List<float> _rotHistory = new();

	private List<float> _alpha = new();

	private float _hue;

	private float _rotChange = 1;


	public override void _Ready()
	{
		_title1 = GetNode<Sprite2D>("%Title1");
		_startPos = _title1.Position;

		for (int i = 0; i < _cloneCount; ++i)
		{
			Sprite2D dupe = (Sprite2D)_title1.Duplicate();
			dupe.Texture = _cloneTex;
			dupe.Visible = false;
			dupe.ZIndex = 50 - i * 3;

			_title1.GetParent().AddChild(dupe);
			_clones.Add(dupe);

			_alpha.Add(0);
		}

        _title1.ZIndex = 100;

		_hue = (float)GD.Randf();
        //_title1.Modulate = Color.FromHsv(_hue, 0.8f, 1f);


		//_velocity = new Vector2(GD.Randf() * _maxSpeed - 0.5f * _maxSpeed, GD.Randf() * _maxSpeed - 0.5f * _maxSpeed);

		UpdateTarget();
    }

	public override void _Process(double delta)
	{
        _hue = (float) Mathf.PosMod(_hue + delta * 0.1, 1.0);
		Color nextCol = Color.FromHsv(_hue, 0.8f, 1f);

        _history.Insert(0, _title1.Position);
		_colorHistory.Insert(0, nextCol);

		_rotHistory.Insert(0, _title1.Rotation);

		if (_history.Count > _totalTimeShift) 
		{
			_history.RemoveAt(_history.Count - 1);
			_colorHistory.RemoveAt(_colorHistory.Count - 1);
            _rotHistory.RemoveAt(_rotHistory.Count - 1);
		}

		for (int i = 0; i < _cloneCount; ++i)
		{
			int index = (i + 1) * _timeShift;
			if (index < _history.Count)
			{
				var dupe = _clones[i];

				if (!dupe.Visible)
				{
					dupe.Visible = true;

					//var tween = dupe.CreateTween();
					//float shift = 0.2f;
					//tween.TweenProperty(dupe, "modulate", new Color(1 - (i+1) * shift, 1 - (i + 1) * shift, 1 - (i + 1) * shift), 1.0f).From(Colors.Transparent);
				}

				float alpha = _alpha[i];
				if (alpha < 1)
				{
					alpha += Mathf.Min((float)delta, 1);
					_alpha[i] = alpha;
				}

				dupe.Position = _history[index];
				dupe.Rotation = _rotHistory[index];

				var color = _colorHistory[index];

				float shift = 0.01f;
				color.R = Mathf.Clamp(color.R - (i + 1) * shift, 0, 1);
				color.G = Mathf.Clamp(color.G - (i + 1) * shift, 0, 1);
				color.B = Mathf.Clamp(color.B - (i + 1) * shift, 0, 1);
				color.A = alpha;
				dupe.Modulate = color;

			}
		}

		Vector2 targetVelocity = (_targetPos - _title1.Position).LimitLength(_maxSpeed);
		_velocity = _velocity.MoveToward(targetVelocity, (float)delta * _accelleration);

		_title1.Position += _velocity * (float)delta;

		if (_rotChange > 0)
		{
			_title1.Rotation += (float)delta * 0.1f;

			if (_title1.Rotation > Mathf.Pi * 0.05)
			{
				_rotChange *= -1;
			}
		}
		else
		{
			_title1.Rotation -=  (float)delta * 0.1f;

			if (_title1.Rotation < -Mathf.Pi * 0.05)
			{
				_rotChange *= -1;
			}
		}


		if (_title1.Position.DistanceTo(_targetPos) < 50)
		{
			UpdateTarget();
		}


		//_hue = (float) Mathf.PosMod(_hue + delta * 0.1, 1.0);
		//_title1.Modulate = Color.FromHsv(_hue, 0.8f, 1f);
	}

	private void UpdateTarget()
	{
        _targetPos = _startPos + new Vector2(GD.Randf() * _offset - 0.5f * _offset, GD.Randf() * _offset - 0.5f * _offset);

		GD.Print("Update");
    }
}
