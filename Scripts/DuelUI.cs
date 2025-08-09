using System;
using System.Collections.Generic;
using Godot;

public partial class DuelUI : Control
{
	private ScrollContainer _logScroll;
	private Label _logLabel;
	private Label _powerLabel;
	private HSlider _powerSlider;
	private Panel _powerAvailable;
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
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer2/ScrollContainer"
		);
		_logLabel = GetNode<Label>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer2/ScrollContainer/CombatLogLabel"
		);
		_powerLabel = GetNode<Label>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer3/HBoxContainer/PowerLabel"
		);
		_powerSlider = GetNode<HSlider>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer3/HBoxContainer/PanelContainer/PowerSlider"
		);
		_powerAvailable = GetNode<Panel>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer3/HBoxContainer/PanelContainer/PowerAvailable"
		);
		_attackButton = GetNode<Button>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer4/AttackButton"
		);
		_playerNameLabel = GetNode<Label>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/PlayerSide/PlayerName"
		);
		_enemyNameLabel = GetNode<Label>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/EnemySide/EnemyName"
		);
		_playerHP = GetNode<ProgressBar>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/PlayerSide/PlayerHP"
		);
		_enemyHP = GetNode<ProgressBar>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/EnemySide/EnemyHP"
		);
		_playerSprite = GetNode<TextureRect>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/PlayerSide/PlayerSprite"
		);
		_enemySprite = GetNode<TextureRect>(
			"PanelContainer/MarginContainer/VBoxContainer/MarginContainer/Header/EnemySide/EnemySprite"
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
		// clear gamestate log (Fixed C# initializer)
		string startingLog =
			$"You brandish your weapon. {duel.Enemy.Name} stands ready. It's a duel!";
		_engine.GS.FullLog = new List<string> { startingLog };

		// assign data
		_currentDuel = duel;
		_engine.GS.CurrentDuel = duel; // ensure GameState points to duel

		_playerNameLabel.Text = _engine.GS.PlayerObject.Name;
		_enemyNameLabel.Text = duel.Enemy.Name;

		_playerHP.MaxValue = _engine.GS.PlayerObject.MaxHealth;
		_enemyHP.MaxValue = duel.Enemy.MaxHealth;

		_playerSprite.Texture = GD.Load<Texture2D>(_engine.GS.PlayerObject.DuelSpritePath);
		_enemySprite.Texture = GD.Load<Texture2D>(duel.Enemy.DuelSpritePath);

		UpdateLog();
		UpdateHealthBars();
		_powerSlider.MaxValue = _engine.GS.PlayerObject.MaxPower;
		_powerSlider.Value = Math.Min(1.0, _powerSlider.MaxValue);
		UpdatePowerAvailable();
		Show();
	}

	public void OnPowerSliderChanged(double value)
	{
		// Disable "Attack" unless selected power is valid
		double currentPower = _engine.GS.PlayerObject.Power;
		bool valid = value <= currentPower;
		_attackButton.Disabled = !valid;

		// Clamp value to currentPower if above it
		if (value > currentPower)
		{
			_powerSlider.Value = currentPower; // snap back
			value = currentPower;
		}

		// UI hint
		_powerLabel.Text = $"Power: {value:F1} {(valid ? "" : "(Not enough!)")}";
	}

	private void OnAttackButtonPressed()
	{
		_attackButton.Disabled = true;
		double powerUsed = Math.Round(_powerSlider.Value, 1);
		_engine.DuelRound(powerUsed);

		UpdateLog();
		UpdateHealthBars();

		double currentPower = _engine.GS.PlayerObject.Power;
		bool valid = _powerSlider.Value <= currentPower;
		_attackButton.Disabled = !valid;
		_powerLabel.Text = $"Power: {_powerSlider.Value:F1} {(valid ? "" : "(Not enough!)")}";

		// if the battle is over, rebind buttons and disable the slider
		if (_engine.GS.CurrentDuel == null) // duel ended -> Engine cleared it
		{
			_powerSlider.Editable = false;
			_attackButton.Pressed -= OnAttackButtonPressed;
			_attackButton.Pressed += RouteAfterDuel;
		}

		_powerSlider.MaxValue = _engine.GS.PlayerObject.MaxPower;
		_powerSlider.Value = Math.Min(1.0, _powerSlider.MaxValue);
		UpdatePowerAvailable();

		_attackButton.Disabled = false;
	}

	public async void UpdateLog()
	{
		_logLabel.Text = string.Join("\n", _engine.GS.FullLog);

		// Wait for three frames so the UI can update first
		await ToSignal(GetTree(), "process_frame");
		await ToSignal(GetTree(), "process_frame");
		await ToSignal(GetTree(), "process_frame");
		ScrollLogToBottom();
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

	private void UpdatePowerAvailable()
	{
		double currentPower = _engine.GS.PlayerObject.Power;
		double maxPower = _engine.GS.PlayerObject.MaxPower;

		// Panel width in pixels
		float maxWidthPixels = 650f; // Max height for full power bar
		float newWidth = (float)(currentPower / maxPower) * maxWidthPixels;

		// Apply new minimum size
		var size = _powerAvailable.CustomMinimumSize;
		size.X = newWidth;
		_powerAvailable.CustomMinimumSize = size;
	}

	private void RouteAfterDuel()
	{
		Hide();
		var main = GetTree().Root.GetNode<MainContainer>("MainScene");
		_attackButton.Pressed -= RouteAfterDuel;

		var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
		if (dialogic != null)
			dialogic.Call("start", _engine.GS.PostDuelTimelinePath);
	}
}
