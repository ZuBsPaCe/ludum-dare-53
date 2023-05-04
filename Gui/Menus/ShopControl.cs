using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShopControl : MenuControlBase
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

    private Label _tank1Label;
    private Label _tank1Cost;
    private Button _tank1Button;

    private Label _fuelSaver1Label;
    private Label _fuelSaver1Cost;
    private Button _fuelSaver1Button;

    private Label _winLabel;
    private Label _winCost;
    private Button _winButton;

    private Label _shopLabel;

    private bool _boughtSomething;

    List<Button> _allButtons = new();

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

        _tank1Label = GetNode<Label>("%Tank1Label");
        _tank1Cost = GetNode<Label>("%Tank1Cost");
        _tank1Button = GetNode<Button>("%Tank1Button");

        _fuelSaver1Label = GetNode<Label>("%FuelSaver1Label");
        _fuelSaver1Cost = GetNode<Label>("%FuelSaver1Cost");
        _fuelSaver1Button = GetNode<Button>("%FuelSaver1Button");

        _winLabel = GetNode<Label>("%WinLabel");
        _winCost = GetNode<Label>("%WinCost");
        _winButton = GetNode<Button>("%WinButton");

        _shopLabel = GetNode<Label>("%ShopLabel");

        _winButton.Pressed += WinButtonPressed;
        _grip1Button.Pressed += Grip1ButtonPressed;
        _grip2Button.Pressed += Grip2ButtonPressed;
        _speed1Button.Pressed += Speed1ButtonPressed;
        _speed2Button.Pressed += Speed2ButtonPressed;
        _tank1Button.Pressed += Tank1ButtonPressed;
        _fuelSaver1Button.Pressed += FuelSaver1ButtonPressed;

        _allButtons.Add(_grip1Button);
        _allButtons.Add(_grip2Button);
        _allButtons.Add(_speed1Button);
        _allButtons.Add(_speed2Button);
        _allButtons.Add(_tank1Button);
        _allButtons.Add(_fuelSaver1Button);
        _allButtons.Add(_winButton);

        UpdateContent();

        if (!HasItems())
        {
            _shopLabel.Text = $"Sorry, we're closed. Have nice trip!";
        }
    }

    private void UpdateContent()
    {
        Button currentFocusButton = _allButtons.Find(button => button.HasFocus());

        _grip1Label.Visible = !State.GripUpgrade1;
        _grip1Cost.Visible = !State.GripUpgrade1;
        _grip1Button.Visible = !State.GripUpgrade1;


        _grip2Label.Visible = !State.GripUpgrade2;
        _grip2Cost.Visible = !State.GripUpgrade2;
        _grip2Button.Visible = !State.GripUpgrade2;


        _speed1Label.Visible = !State.SpeedUpgrade1;
        _speed1Cost.Visible = !State.SpeedUpgrade1;
        _speed1Button.Visible = !State.SpeedUpgrade1;

        _speed2Label.Visible = !State.SpeedUpgrade2;
        _speed2Cost.Visible = !State.SpeedUpgrade2;
        _speed2Button.Visible = !State.SpeedUpgrade2;

        _tank1Label.Visible = !State.TankUpgrade1;
        _tank1Cost.Visible = !State.TankUpgrade1;
        _tank1Button.Visible = !State.TankUpgrade1;

        _fuelSaver1Label.Visible = !State.FuelSaverUpgrade1;
        _fuelSaver1Cost.Visible = !State.FuelSaverUpgrade1;
        _fuelSaver1Button.Visible = !State.FuelSaverUpgrade1;

        _winLabel.Visible = !State.ShopWinBought;
        _winCost.Visible = !State.ShopWinBought;
        _winButton.Visible = !State.ShopWinBought;

        if (currentFocusButton != null && currentFocusButton.Visible == false)
        {
            List<Button> buttons = _allButtons.ToList();
            int currentIndex = buttons.IndexOf(currentFocusButton);

            // GrabFocus on next button. If there are no buttons visible, MenuBase.UpdateFocusToLeft()
            // will grab focus on the menu bar button itself.

            while (buttons.Count > 0)
            {
                if (buttons[currentIndex].Visible == false)
                {
                    buttons.RemoveAt(currentIndex);

                    if (currentIndex >= buttons.Count)
                    {
                        currentIndex -= 1;
                    }
                }
                else
                {
                    buttons[currentIndex].GrabFocus();
                    break;
                }
            }
        }
    }

    private bool HasItems()
    {
        return !State.GripUpgrade1 || !State.GripUpgrade2 || !State.SpeedUpgrade1 || !State.SpeedUpgrade2 || !State.TankUpgrade1 || !State.FuelSaverUpgrade1 || !State.ShopWinBought;
    }

    private void WinButtonPressed()
    {
        if (State.Money < int.Parse(_winCost.Text.Replace("$ ", "")))
        {
            _shopLabel.Text = "Are you dying to get away? The Florida Keys are waiting for you...";
            return;
        }

        State.Money -= int.Parse(_winCost.Text.Replace("$ ", ""));
        State.ShopWinBought = true;

        Sounds.PlaySound(SoundType.Won);

        _shopLabel.Text = $"You've won!\nAnd thanks for playing :)\n\nTime taken: {Mathf.RoundToInt(State.LevelTime.Elapsed.TotalMinutes)} minutes";

        GameEventHub.EmitShopBoughtWin();

        UpdateContent();
    }

    private void Grip1ButtonPressed()
    {
        if (State.Money < int.Parse(_grip1Cost.Text.Replace("$ ", "")))
        {
            _shopLabel.Text = "I'm not doing this for free...";
            return;
        }

        State.Money -= int.Parse(_grip1Cost.Text.Replace("$ ", ""));
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
        if (State.Money < int.Parse(_grip2Cost.Text.Replace("$ ", "")))
        {
            _shopLabel.Text = "Great tires, ain't them?";
            return;
        }

        State.Money -= int.Parse(_grip2Cost.Text.Replace("$ ", ""));
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
        if (State.Money < int.Parse(_speed1Cost.Text.Replace("$ ", "")))
        {
            _shopLabel.Text = "Yeah, your truck could need that...";
            return;
        }

        State.Money -= int.Parse(_speed1Cost.Text.Replace("$ ", ""));
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
        if (State.Money < int.Parse(_speed2Cost.Text.Replace("$ ", "")))
        {
            _shopLabel.Text = "Don't know if that's a good idea for a truck...";
            return;
        }

        State.Money -= int.Parse(_speed2Cost.Text.Replace("$ ", ""));
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

    private void Tank1ButtonPressed()
    {
        if (State.Money < int.Parse(_tank1Cost.Text.Replace("$ ", "")))
        {
            _shopLabel.Text = "Driving a lot, ain't ya?";
            return;
        }

        State.Money -= int.Parse(_tank1Cost.Text.Replace("$ ", ""));
        State.TankUpgrade1 = true;
        State.TankMaxSize = 85;
        State.Fuel = 85;

        Sounds.PlaySound(SoundType.Money);


        if (HasItems())
        {
            _shopLabel.Text = $"Fine choice! Need something else?";
        }
        else
        {
            _shopLabel.Text = $"Thank you and good bye!";
        }


        UpdateContent();
    }

    private void FuelSaver1ButtonPressed()
    {
        if (State.Money < int.Parse(_fuelSaver1Cost.Text.Replace("$ ", "")))
        {
            _shopLabel.Text = "Very good for the environment.";
            return;
        }

        State.Money -= int.Parse(_fuelSaver1Cost.Text.Replace("$ ", ""));
        State.FuelSaverUpgrade1 = true;

        Sounds.PlaySound(SoundType.Money);


        if (HasItems())
        {
            _shopLabel.Text = $"Awesome! Need something else?";
        }
        else
        {
            _shopLabel.Text = $"Thank you and good bye!";
        }


        UpdateContent();
    }
}