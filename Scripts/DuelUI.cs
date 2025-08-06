using System;
using Godot;

public partial class DuelUI : Control
{
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
		_logLabel = GetNode<Label>("PanelContainer/VBoxContainer/CombatLogLabel");
		_powerLabel = GetNode<Label>("PanelContainer/VBoxContainer/HBoxContainer/PowerLabel");
		_powerSlider = GetNode<HSlider>("PanelContainer/VBoxContainer/HBoxContainer/PowerSlider");
		_attackButton = GetNode<Button>("PanelContainer/VBoxContainer/AttackButton");
		_playerNameLabel = GetNode<Label>(
			"PanelContainer/VBoxContainer/Header/PlayerSide/PlayerName"
		);
		_enemyNameLabel = GetNode<Label>("PanelContainer/VBoxContainer/Header/EnemySide/EnemyName");
		_playerHP = GetNode<ProgressBar>("PanelContainer/VBoxContainer/Header/PlayerSide/PlayerHP");
		_enemyHP = GetNode<ProgressBar>("PanelContainer/VBoxContainer/Header/EnemySide/EnemyHP");
		_playerSprite = GetNode<TextureRect>(
			"PanelContainer/VBoxContainer/Header/PlayerSide/PlayerSprite"
		);
		_enemySprite = GetNode<TextureRect>(
			"PanelContainer/VBoxContainer/Header/EnemySide/EnemySprite"
		);

		// HSlider settings
		_powerSlider.Step = 0.1;
		_powerSlider.MinValue = 0.0;

		// subscribe to events
		_attackButton.Pressed += OnAttackButtonPressed;
		_powerSlider.ValueChanged += OnPowerSliderChanged;

		// Hide ui
		Hide();
	}

	public void StartDuel(Duel duel)
	{
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
		UpdateMaxPower();
		UpdateHealthBars();
		_powerSlider.Value = Math.Min(1.0, _powerSlider.MaxValue); // Default start value
		Show();
	}

	public void OnPowerSliderChanged(double value)
	{
		_powerLabel.Text = $"Power: {value:F1}";
	}

	private void OnAttackButtonPressed()
	{
		_attackButton.Disabled = true;
		double powerUsed = Math.Round(_powerSlider.Value, 1); // Round to 1 decimal place
		_engine.DuelRound(powerUsed);
		UpdateLog();
		UpdateMaxPower();
		UpdateHealthBars();

		if (_engine.GS.CurrentElement is not Duel)
		{
			Hide();
			RouteAfterDuel();
		}
		else
		{
			_attackButton.Disabled = false; // Re-enable only if duel continues
		}
	}

	public void UpdateLog()
	{
		_logLabel.Text = string.Join("\n", _engine.GS.FullLog);
	}

	public void UpdateMaxPower()
	{
		double newMax = _engine.GS.PlayerObject.Power;
		_powerSlider.MaxValue = newMax;
		_powerSlider.Value = Math.Min(_powerSlider.Value, newMax);
		OnPowerSliderChanged(_powerSlider.Value);
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
	}
}
