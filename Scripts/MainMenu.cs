using Godot;

public partial class MainMenu : Control
{
	private Button _playButton;
	private Button _optionsButton;
	private Button _quitButton;
	private AudioStreamPlayer _clickPlayer;

	public override void _Ready()
	{
		_playButton = GetNode<Button>("CenterContainer/VBoxContainer/PlayButton");
		_optionsButton = GetNode<Button>("CenterContainer/VBoxContainer/OptionsButton");
		_quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");

		_playButton.Pressed += OnPlayPressed;
		_optionsButton.Pressed += OnOptionsPressed;
		_quitButton.Pressed += OnQuitPressed;
	}

	private void PlayClickSound()
	{
		if (_clickPlayer != null)
			_clickPlayer.Play();
	}

	private async void OnPlayPressed()
	{
		PlayClickSound();
		var fade = (FadeOverlay)GetNode("/root/FadeOverlay");
		await fade.FadeOut();
		GetTree().ChangeSceneToFile("res://Scenes/map_container.tscn");
		await fade.FadeIn();
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
