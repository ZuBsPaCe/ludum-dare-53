using Godot;
using System;

public partial class ShopControl : MarginContainer
{
    private Label _grip1Label;
    private Label _grip1Cost;
    private Button _grip1Button;

    private Label _grip2Label;
    private Label _grip2Cost;
    private Button _grip2Button;

    private Label _speed1Label;
    private Label _speed1Cost;
    private Button _speed1Button;

    private Label _speed2Label;
    private Label _speed2Cost;
    private Button _speed2Button;

    private Label _winLabel;
    private Label _winCost;
    private Button _winButton;

    private Label _shopLabel;

    private bool _boughtSomething;

    public override void _Ready()
    {
        _grip1Label = GetNode<Label>("%Grip1Label");
        _grip1Cost = GetNode<Label>("%Grip1Cost");
        _grip1Button = GetNode<Button>("%Grip1Button");

        _grip2Label = GetNode<Label>("%Grip2Label");
        _grip2Cost = GetNode<Label>("%Grip2Cost");
        _grip2Button = GetNode<Button>("%Grip2Button");

        _speed1Label = GetNode<Label>("%Speed1Label");
        _speed1Cost = GetNode<Label>("%Speed1Cost");
        _speed1Button = GetNode<Button>("%Speed1Button");

        _speed2Label = GetNode<Label>("%Speed2Label");
        _speed2Cost = GetNode<Label>("%Speed2Cost");
        _speed2Button = GetNode<Button>("%Speed2Button");

        _winLabel = GetNode<Label>("%WinLabel");
        _winCost = GetNode<Label>("%WinCost");
        _winButton = GetNode<Button>("%WinButton");

        _shopLabel = GetNode<Label>("%ShopLabel");

        _winButton.Pressed += WinButtonPressed;
        _grip1Button.Pressed += Grip1ButtonPressed;
        _grip2Button.Pressed += Grip2ButtonPressed;
        _speed1Button.Pressed += Speed1ButtonPressed;
        _speed2Button.Pressed += Speed2ButtonPressed;

        UpdateContent();

        if (!HasItems())
        {
            _shopLabel.Text = $"Sorry, we're closed. Have nice trip!";
        }
    }

    private void UpdateContent()
    {
        _grip1Label.Visible = !State.GripUpgrade1;
        _grip1Cost.Visible = !State.GripUpgrade1;
        _grip1Button.Visible = !State.GripUpgrade1;

        _grip1Button.Disabled = State.Money < 30;


        _grip2Label.Visible = !State.GripUpgrade2;
        _grip2Cost.Visible = !State.GripUpgrade2;
        _grip2Button.Visible = !State.GripUpgrade2;

        _grip2Button.Disabled = State.Money < 60;


        _speed1Label.Visible = !State.SpeedUpgrade1;
        _speed1Cost.Visible = !State.SpeedUpgrade1;
        _speed1Button.Visible = !State.SpeedUpgrade1;

        _speed1Button.Disabled = State.Money < 80;


        _speed2Label.Visible = !State.SpeedUpgrade2;
        _speed2Cost.Visible = !State.SpeedUpgrade2;
        _speed2Button.Visible = !State.SpeedUpgrade2;

        _speed2Button.Disabled = State.Money < 120;


        _winLabel.Visible = !State.ShopWinBought;
        _winCost.Visible = !State.ShopWinBought;
        _winButton.Visible = !State.ShopWinBought;

        _winButton.Disabled = State.Money < 999;
    }

    private bool HasItems()
    {
        return !State.GripUpgrade1 || !State.GripUpgrade2 || !State.SpeedUpgrade1 || !State.SpeedUpgrade2 || !State.ShopWinBought;
    }

    private void WinButtonPressed()
    {
        State.Money -= 999;
        State.ShopWinBought = true;

        Sounds.PlaySound(SoundType.Won);

        _shopLabel.Text = $"You've won!\nAnd thanks for playing :)\n\nTime taken: {Mathf.RoundToInt(State.LevelTime.Elapsed.TotalMinutes)} minutes";

        GameEventHub.EmitShopBoughtWin();

        UpdateContent();
    }

    private void Grip1ButtonPressed()
    {
        State.Money -= 30;
        State.GripUpgrade1 = true;

        GameEventHub.EmitGripBought();

        Sounds.PlaySound(SoundType.Money);

        if (HasItems())
        {
            _shopLabel.Text = $"Thank you. Need something else?";
        }
        else
        {
            _shopLabel.Text = $"Thank you and good bye!";
        }

        UpdateContent();
    }

    private void Grip2ButtonPressed()
    {
        State.Money -= 60;
        State.GripUpgrade2 = true;

        GameEventHub.EmitGripBought();

        Sounds.PlaySound(SoundType.Money);


        if (HasItems())
        {
            _shopLabel.Text = $"Nice! Need something else?";
        }
        else
        {
            _shopLabel.Text = $"Thank you and good bye!";
        }


        UpdateContent();
    }

    private void Speed1ButtonPressed()
    {
        State.Money -= 80;
        State.SpeedUpgrade1 = true;

        Sounds.PlaySound(SoundType.Money);


        if (HasItems())
        {
            _shopLabel.Text = $"Perfect! Need something else?";
        }
        else
        {
            _shopLabel.Text = $"Thank you and good bye!";
        }


        UpdateContent();
    }

    private void Speed2ButtonPressed()
    {
        State.Money -= 120;
        State.SpeedUpgrade2 = true;

        Sounds.PlaySound(SoundType.Money);


        if (HasItems())
        {
            _shopLabel.Text = $"Drive safely! Need something else?";
        }
        else
        {
            _shopLabel.Text = $"Thank you and good bye!";
        }


        UpdateContent();
    }
}