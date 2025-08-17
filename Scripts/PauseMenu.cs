using System;
using Godot;

public partial class PauseMenu : Control
{
	private Button _resumeButton;
	private Button _optionsButton;
	private Button _quitButton;
	private bool pauseMenuShown = false;
	private AudioStreamPlayer _clickPlayer;

	public override void _Ready()
	{
		_resumeButton = GetNode<Button>("Panel/VBoxContainer/ResumeButton");
		_optionsButton = GetNode<Button>("Panel/VBoxContainer/SettingsButton");
		_quitButton = GetNode<Button>("Panel/VBoxContainer/QuitButton");
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");

		_resumeButton.Pressed += TogglePauseMenu;
		_optionsButton.Pressed += OnOptionsPressed;
		_quitButton.Pressed += OnQuitPressed;

		Hide();
	}

	private void PlayClickSound()
	{
		if (_clickPlayer != null)
			_clickPlayer.Play();
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Pause"))
		{
			TogglePauseMenu();
		}
	}

	public void TogglePauseMenu()
	{
		var gameUI = GetNode<GameUI>("/root/MainScene/UIContainer/GameUI");

		if (pauseMenuShown)
		{
			gameUI.Show();
			PlayClickSound();
			Hide();
			pauseMenuShown = false;
		}
		else
		{
			gameUI.Hide();
			PlayClickSound();
			Show();
			pauseMenuShown = true;
		}
	}

	private void OnOptionsPressed()
	{
		PlayClickSound();
		// Navigate to options scene
	}

	private void OnQuitPressed()
	{
		PlayClickSound();
		if (OS.HasFeature("web"))
			return;
		GetTree().Quit();
	}

	// needs to hide game UI on pause
	// needs to block movement on pause
}
