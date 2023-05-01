using Godot;
using System;

public partial class ShopControl : MarginContainer
{
    private Label _winLabel;
    private Label _winCost;
    private Button _winButton;

    private Label _shopLabel;

    public override void _Ready()
    {
        _winLabel = GetNode<Label>("%WinLabel");
        _winCost = GetNode<Label>("%WinCost");
        _winButton = GetNode<Button>("%WinButton");

        _shopLabel = GetNode<Label>("%ShopLabel");

        _winButton.Pressed += WinButtonPressed;

        UpdateContent();
    }

    private void UpdateContent()
    {
        _winLabel.Visible = !State.ShopWinBought;
        _winCost.Visible = !State.ShopWinBought;
        _winButton.Visible = !State.ShopWinBought;

        _winButton.Disabled = State.Money < 999;
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
}