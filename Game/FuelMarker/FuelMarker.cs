using Godot;

public partial class FuelMarker : Node3D
{
    public override void _Ready()
    {
        Area3D area = GetNode<Area3D>("Area3D");
        area.BodyEntered += (body) => Area_BodyEntered(body);
        area.BodyExited += (body) => Area_BodyExited(body);
    }

    private void Area_BodyEntered(Node3D body)
    {
		if (body.IsInGroup(Group.Player))
		{
            GameEventHub.EmitFuelMarkerEntered();
		}
    }

    private void Area_BodyExited(Node3D body)
    {
		if (body.IsInGroup(Group.Player))
		{
            GameEventHub.EmitFuelMarkerExited();
		}
    }
}
