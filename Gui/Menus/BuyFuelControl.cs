using Godot;
using System;
using System.Resources;

public partial class BuyFuelControl : MarginContainer
{
    private Label _buyFuelLabel;

    public override void _Ready()
    {
        _buyFuelLabel = GetNode<Label>("%FuelLabel");
        UpdateMessage();

        GameEventHub.Instance.FuelChanged += (amount) => EventHub_FuelChanged(amount);
    }

    private void EventHub_FuelChanged(int amount)
    {
        UpdateMessage();
    }

    private void UpdateMessage()
    {
        if (State.Fuel >= 65)
        {
            _buyFuelLabel.Text = "Your truck is fully fueled. Please come back later.";
        }
        else if (State.Money > 0)
        {
            _buyFuelLabel.Text = "Thank you! Press the button again to buy more fuel!";
        }
        else if (State.Fuel > 0)
        {
            _buyFuelLabel.Text = "You are broke. Better earn some money!";
        }
        else
        {
            _buyFuelLabel.Text = "You are broke and have no fuel. Game over.";
        }
    }
}
