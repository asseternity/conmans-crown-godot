using System;
using System.Threading.Tasks;
using Godot;

public partial class MapScreen : Control
{
	// define and find the buttons
	// define and find the fade in scene
	// attach events to buttons in _Ready()
	// make a MoveToScene(string scenePath) that will fade to that scene

	[Export]
	public string HomeScenePath { get; set; } = "res://Scenes/level_apartment.tscn";

	[Export]
	public string OfficeScenePath { get; set; } = "res://Scenes/level_test.tscn";

	[Export]
	public string BeachScenePath { get; set; } = "res://Scenes/level_test_2.tscn";

	private Engine _engine;
	private Button _homeButton;
	private Button _officeButton;
	private Button _beachButton;
	private AudioStreamPlayer _clickPlayer;

	public override void _Ready()
	{
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		_homeButton = GetNode<Button>("TextureRect/HomeButtonShadow/HomeButton");
		_officeButton = GetNode<Button>("TextureRect/OfficeButtonShadow/OfficeButton");
		_beachButton = GetNode<Button>("TextureRect/BeachButtonShadow/BeachButton");
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");

		_homeButton.Pressed += () => LoadLevel(HomeScenePath);
		_officeButton.Pressed += () => LoadLevel(OfficeScenePath);
		_beachButton.Pressed += () => LoadLevel(BeachScenePath);
	}

	private void PlayClickSound()
	{
		if (_clickPlayer != null)
			_clickPlayer.Play();
	}

	private async void LoadLevel(string levelPath)
	{
		if (IsInstanceValid(_clickPlayer))
			_clickPlayer.Play();

		await _engine.LoadLevelInMain(levelPath);
	}
}
