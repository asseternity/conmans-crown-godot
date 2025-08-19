using System;
using Godot;

public partial class GameUI : Control
{
	private AudioStreamPlayer _clickPlayer;
	private Button _mapButton;
	private Button _settingsButton;

	public override void _Ready()
	{
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
		_mapButton = GetNode<Button>("MapButton");
		_settingsButton = GetNode<Button>("PauseButton");
		_mapButton.Pressed += OnMapPressed;
		_settingsButton.Pressed += OnSettingsPressed;
	}

	private void PlayClickSound()
	{
		if (_clickPlayer != null)
			_clickPlayer.Play();
	}

	private void OnMapPressed()
	{
		PlayClickSound();
		GetTree().ChangeSceneToFile("res://Scenes/map_container.tscn");
	}

	private void OnSettingsPressed()
	{
		var pauseUI = GetNode<PauseMenu>("/root/MainScene/UIContainer/PauseMenu");
		pauseUI.TogglePauseMenu();
	}
}
