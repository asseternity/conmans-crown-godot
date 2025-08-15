using System;
using Godot;

public partial class GameUi : Control
{
	private AudioStreamPlayer _clickPlayer;
	private Button _mapButton;

	public override void _Ready()
	{
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
		_mapButton = GetNode<Button>("MapButton");
		_mapButton.Pressed += OnPlayPressed;
	}

	private void PlayClickSound()
	{
		if (_clickPlayer != null)
			_clickPlayer.Play();
	}

	private void OnPlayPressed()
	{
		PlayClickSound();
		GetTree().ChangeSceneToFile("res://Scenes/map_container.tscn");
	}
}
