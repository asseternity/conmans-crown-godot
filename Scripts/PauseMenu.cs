using System;
using Godot;

public partial class PauseMenu : Control
{
	private bool pauseMenuShown = false;
	private AudioStreamPlayer _clickPlayer;

	public override void _Ready()
	{
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
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
		if (pauseMenuShown)
		{
			PlayClickSound();
			Hide();
			pauseMenuShown = false;
		}
		else
		{
			PlayClickSound();
			Show();
			pauseMenuShown = true;
		}
	}

	// needs to hide game UI on pause
	// needs to block movement on pause
}
