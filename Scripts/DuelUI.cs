using System;
using System.Collections.Generic;
using Godot;

public partial class DuelUI : Control
{
	private ScrollContainer _logScroll;
	private Label _logLabel;
	private Label _powerLabel;
	private HSlider _powerSlider;
	private Button _attackButton;
	private Label _playerNameLabel;
	private Label _enemyNameLabel;
	private ProgressBar _playerHP;
	private ProgressBar _enemyHP;
	private TextureRect _playerSprite;
	private TextureRect _enemySprite;
	private Duel _currentDuel;
	private Engine _engine;

	public override void _Ready()
	{
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		_logScroll = GetNode<ScrollContainer>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer2/ScrollContainer"
		);
		_logLabel = GetNode<Label>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer2/ScrollContainer/VBoxContainer/CombatLogLabel"
		);
		_powerLabel = GetNode<Label>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer3/HBoxContainer/PowerLabel"
		);
		_powerSlider = GetNode<HSlider>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer3/HBoxContainer/PowerSlider"
		);
		_attackButton = GetNode<Button>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer4/AttackButton"
		);
		_playerNameLabel = GetNode<Label>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/PlayerSide/PlayerName"
		);
		_enemyNameLabel = GetNode<Label>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/EnemySide/EnemyName"
		);
		_playerHP = GetNode<ProgressBar>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/PlayerSide/PlayerHP"
		);
		_enemyHP = GetNode<ProgressBar>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/EnemySide/EnemyHP"
		);
		_playerSprite = GetNode<TextureRect>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/PlayerSide/PlayerSprite"
		);
		_enemySprite = GetNode<TextureRect>(
			"CenterContainer/PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/EnemySide/EnemySprite"
		);

		// HSlider settings
		_powerSlider.Step = 0.1;
		_powerSlider.MinValue = 0.0;

		// subscribe to events
		_attackButton.Pressed += OnAttackButtonPressed;
		_powerSlider.ValueChanged += OnPowerSliderChanged;
		_powerSlider.Editable = true;

		// Hide ui
		Hide();
	}

	public void StartDuel(Duel duel)
	{
		// clear gamestate log
		string startingLog =
			$"You brandish your weapon. {duel.Enemy.Name} stands ready. It's a duel!";
		_engine.GS.FullLog = new List<string>([startingLog]);

		// assign data
		_currentDuel = duel;
		_playerNameLabel.Text = _engine.GS.PlayerObject.Name;
		_enemyNameLabel.Text = duel.Enemy.Name;

		_playerHP.MaxValue = _engine.GS.PlayerObject.MaxHealth;
		_enemyHP.MaxValue = duel.Enemy.MaxHealth;

		// assign textures
		_playerSprite.Texture = GD.Load<Texture2D>(_engine.GS.PlayerObject.DuelSpritePath);
		_enemySprite.Texture = GD.Load<Texture2D>(duel.Enemy.DuelSpritePath);

		// update UI
		UpdateLog();
		UpdateHealthBars();
		_powerSlider.MaxValue = _engine.GS.PlayerObject.MaxPower;
		_powerSlider.Value = Math.Min(1.0, _powerSlider.MaxValue); // Default start value
		Show();
	}

	public void OnPowerSliderChanged(double value)
	{
		// Disable "Attack" unless selected power is valid
		double currentPower = _engine.GS.PlayerObject.Power;
		bool valid = value <= currentPower;
		_attackButton.Disabled = !valid;

		// UI hint
		_powerLabel.Text = $"Power: {value:F1} {(valid ? "" : "(Not enough!)")}";
	}

	private void OnAttackButtonPressed()
	{
		// disable the continue button
		_attackButton.Disabled = true;

		// take the power value and run the round
		double powerUsed = Math.Round(_powerSlider.Value, 1); // Round to 1 decimal place
		_engine.DuelRound(powerUsed);

		// update UI
		UpdateLog();
		UpdateHealthBars();

		// update power slider hint
		double currentPower = _engine.GS.PlayerObject.Power;
		bool valid = _powerSlider.Value <= currentPower;
		_attackButton.Disabled = !valid;
		_powerLabel.Text = $"Power: {_powerSlider.Value:F1} {(valid ? "" : "(Not enough!)")}";

		// if the battle is over, rebind buttons and disable the slider
		if (_engine.GS.CurrentElement is not Duel)
		{
			_powerSlider.Editable = false;
			_attackButton.Pressed -= OnAttackButtonPressed;
			_attackButton.Pressed += RouteAfterDuel;
		}

		// re-enable the continue button in any case
		_attackButton.Disabled = false;
	}

	public void UpdateLog()
	{
		_logLabel.Text = string.Join("\n", _engine.GS.FullLog);

		// Delay scroll until after UI updates
		CallDeferred(nameof(ScrollLogToBottom));
	}

	private void ScrollLogToBottom()
	{
		_logScroll.ScrollVertical = (int)_logScroll.GetVScrollBar().MaxValue;
	}

	public void UpdateHealthBars()
	{
		_playerHP.Value = _engine.GS.PlayerObject.Health;
		_enemyHP.Value = _currentDuel.Enemy.Health;
	}

	private void RouteAfterDuel()
	{
		Hide();
		var main = GetTree().Root.GetNode<MainContainer>("MainScene"); // load MainContainer.cs inside the MainScene root node
		main.RouteElement(_engine.GS.CurrentElement); // this handles both story or duel
		_attackButton.Pressed -= RouteAfterDuel; // unsubscribe from route after duel
	}
}
