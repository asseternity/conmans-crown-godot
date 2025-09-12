using System;
using Godot;

public partial class GameUI : Control
{
	private AudioStreamPlayer _clickPlayer;
	private Button _mapButton;
	private Button _settingsButton;
	private Button _inventoryButton;
	private Button _questsButton;

	public override void _Ready()
	{
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
		_mapButton = GetNode<Button>("MapButton");
		_settingsButton = GetNode<Button>("PauseButton");
		_inventoryButton = GetNode<Button>("InventoryButton");
		_questsButton = GetNode<Button>("QuestsButton");
		_mapButton.Pressed += OnMapPressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_inventoryButton.Pressed += OnInventoryPressed;
		_questsButton.Pressed += OnQuestsPressed;
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
		PlayClickSound();
		var pauseUI = GetNode<PauseMenu>("/root/MainScene/UIContainer/PauseMenu");
		pauseUI.TogglePauseMenu();
	}

	private void OnInventoryPressed()
	{
		PlayClickSound();
		var inventoryUI = GetNode<InventoryUI>("/root/MainScene/UIContainer/InventoryUI");
		inventoryUI.Show();
	}

	private void OnQuestsPressed()
	{
		PlayClickSound();
		var questUI = GetNode<QuestUI>("/root/MainScene/UIContainer/QuestUI");
		questUI.Show();
	}
}
