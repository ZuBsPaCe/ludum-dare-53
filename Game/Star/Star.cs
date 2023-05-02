using Godot;
using System;

public partial class Star : Node3D
{
	private Area3D _area;
	private Tween _tween;
	private MeshInstance3D _mesh;


    public override void _Ready()
	{
		_area = GetNode<Area3D>("%Area3D");
        _mesh = GetNode<MeshInstance3D>("%Cylinder");
        _tween = _mesh.CreateTween();
        _tween.SetLoops();
        _tween.TweenProperty(_mesh, "rotation", new Vector3(Mathf.Tau * 0.25f, Mathf.Tau, 0), 1).From(new Vector3(Mathf.Tau * 0.25f, 0, 0));

		_area.BodyEntered += Area_BodyEntered;
	}

    private void Pickup()
    {
		_area.CollisionLayer = 0;
		_area.QueueFree();

        var tween = _mesh.CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Cubic);

        var wtf = _mesh.GetPropertyList();
        tween.TweenProperty(_mesh, "scale", _mesh.Scale + new Vector3(7, 0, 7), 0.5f);

        var material = (StandardMaterial3D)_mesh.GetSurfaceOverrideMaterial(0);
        var albedo = material.AlbedoColor;
        albedo.A = 0;
        tween.Parallel().TweenProperty(_mesh.GetSurfaceOverrideMaterial(0), "albedo_color", albedo, 0.5f);

        _tween.TweenCallback(Callable.From(() => { QueueFree(); }));
    }

    private void Area_BodyEntered(Node3D body)
    {
		if (body.IsInGroup("Player"))
		{
			Pickup();
			GameEventHub.EmitStarPickedUp();
		}
    }
}
