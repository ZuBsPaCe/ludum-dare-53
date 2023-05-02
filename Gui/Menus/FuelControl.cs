using Godot;
using System;

public partial class FuelControl : MarginContainer
{
    private Label _fuelLabel;
    private Button _buyFuelButton;

    private bool _boughtSome;

    public override void _Ready()
    {
        _fuelLabel = GetNode<Label>("%FuelLabel");
        _buyFuelButton = GetNode<Button>("%BuyFuelButton");
        _buyFuelButton.Pressed += () => BuyFuelButtonPressed();

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
            _fuelLabel.Text = "Your truck is fully fueled. Please come back later.";
            _buyFuelButton.Disabled = true;
        }
        else if (State.Money > 0)
        {
            if (_boughtSome)
            {
                _fuelLabel.Text = "Thank you.\n\n10 fuel for 10 bucks. Ok?";
            }
            else
            {
                _fuelLabel.Text = "10 fuel for 10 bucks. Ok?";
            }
            _buyFuelButton.Disabled = false;
        }
        else if (State.Fuel > 0)
        {
            _fuelLabel.Text = "You are broke. Better earn some money!";
            _buyFuelButton.Disabled = true;
        }
        else
        {
            _fuelLabel.Text = "You are broke and have no fuel. Game over.";
            _buyFuelButton.Disabled = true;
        }
    }

    private void BuyFuelButtonPressed()
    {
        int buyFuel = Mathf.Min(State.Money, 10);

        if (State.Fuel + buyFuel > 65)
        {
            buyFuel = 65 - State.Fuel;
        }

        if (buyFuel > 0)
        {
            State.Money -= buyFuel;
            State.Fuel += buyFuel;

            _boughtSome = true;

            Sounds.PlaySound(SoundType.Money);
        }

        UpdateMessage();
    }
}
