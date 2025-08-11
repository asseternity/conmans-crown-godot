using Godot;

public partial class MainMenu : Control
{
	private Button _playButton;
	private Button _optionsButton;
	private Button _quitButton;

	public override void _Ready()
	{
		_playButton = GetNode<Button>("CenterContainer/VBoxContainer/PlayButton");
		_optionsButton = GetNode<Button>("CenterContainer/VBoxContainer/OptionsButton");
		_quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");

		_playButton.Pressed += OnPlayPressed;
		_optionsButton.Pressed += OnOptionsPressed;
		_quitButton.Pressed += OnQuitPressed;

		_playButton.GrabFocus(); // keyboard/gamepad ready
	}

	private async void OnPlayPressed()
	{
		// Get the autoload instance
		var fade = (FadeOverlay)GetNode("/root/FadeOverlay");

		// Fade out, change scene, fade in
		await fade.FadeOut();
		GetTree().ChangeSceneToFile("res://Scenes/main_container.tscn");
		await fade.FadeIn();
	}

	private void OnOptionsPressed()
	{
		// Navigate to an options scene.
	}

	private void OnQuitPressed()
	{
		if (OS.HasFeature("web"))
			return; // Can't quit a browser build
		GetTree().Quit();
	}
}
