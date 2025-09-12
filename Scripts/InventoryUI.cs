using System;
using Godot;

public partial class InventoryUI : Control
{
    private Button _closeButton;
    private AudioStreamPlayer _clickPlayer;

    public override void _Ready()
    {
        _closeButton = GetNode<Button>("Panel/Panel/CloseButton");
        _closeButton.Pressed += OnCloseButtonClicked;
        _clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
        Hide();
    }

    private void OnCloseButtonClicked()
    {
        _clickPlayer.Play();
        Hide();
    }
}
