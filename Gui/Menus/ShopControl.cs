using Godot;
using System;

public partial class ShopControl : MarginContainer
{
    private Label _shopLabel;

    public override void _Ready()
    {
        _shopLabel = GetNode<Label>("%ShopLabel");
        UpdateMessage();
    }

    private void UpdateMessage()
    {
        //if (State.Fuel >= 65)
        //{
        //    _repairLabel.Text = "Your truck is fully fueled. Please come back later.";
        //}
        //else if (State.Money > 0)
        //{
        //    _repairLabel.Text = "Thank you! Press the button again to buy more fuel!";
        //}
        //else if (State.Fuel > 0)
        //{
        //    _repairLabel.Text = "You are broke. Better earn some money!";
        //}
        //else
        //{
        //    _repairLabel.Text = "You are broke and have no fuel. Game over.";
        //}
    }
}