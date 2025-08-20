using System;
using System.Threading.Tasks;
using Godot;

public partial class PauseMenu : Control
{
	private ColorRect _dimmer;
	private Button _resumeButton;
	private Button _optionsButton;
	private Button _quitButton;
	private bool pauseMenuShown = false;
	private AudioStreamPlayer _clickPlayer;

	public override void _Ready()
	{
		_dimmer = GetNode<ColorRect>("Dimmer");
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
			_dimmer.Color = new Color(0, 0, 0, 0.0f);
			gameUI.Show();
			PlayClickSound();
			Hide();
			pauseMenuShown = false;
		}
		else
		{
			_dimmer.Color = new Color(0, 0, 0, 0.5f);
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
}
