using System;
using Godot;

public partial class StatsUI : Control
{
	private Button _closeButton;
	private AudioStreamPlayer _clickPlayer;

	// Labels
	private Label _nameLabel;
	private Label _healthLabel;
	private Label _powerLabel;
	private Label _subterfugeLabel;
	private Label _brawnLabel;
	private Label _charismaLabel;

	// Bars
	private ColorRect _currentHealth;
	private ColorRect _maxHealth;
	private ColorRect _currentPower;
	private ColorRect _maxPower;
	private ColorRect _currentBrawn;
	private ColorRect _maxBrawn;
	private ColorRect _currentCharisma;
	private ColorRect _maxCharisma;
	private ColorRect _currentSubterfuge;
	private ColorRect _maxSubterfuge;

	// Personality bars
	private ColorRect _honestBar;
	private ColorRect _manipulativeBar;
	private ColorRect _accommodatingBar;
	private ColorRect _domineeringBar;
	private ColorRect _humanistBar;
	private ColorRect _deistBar;

	public override void _Ready()
	{
		_closeButton = GetNode<Button>("Panel/CloseButton");
		_closeButton.Pressed += OnCloseButtonClicked;
		_clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");

		// Grab labels
		_nameLabel = GetNode<Label>("Panel/VBoxContainer/NameLabel");
		_healthLabel = GetNode<Label>("Panel/VBoxContainer/MaxHealth/HealthLabel");
		_powerLabel = GetNode<Label>("Panel/VBoxContainer/MaxPower/PowerLabel");
		_subterfugeLabel = GetNode<Label>("Panel/VBoxContainer/MaxSubterfuge/SubterfugeLabel");
		_brawnLabel = GetNode<Label>("Panel/VBoxContainer/MaxBrawn/BrawnLabel");
		_charismaLabel = GetNode<Label>("Panel/VBoxContainer/MaxCharisma/CharismaLabel");

		// Grab bars
		_maxHealth = GetNode<ColorRect>("Panel/VBoxContainer/MaxHealth");
		_currentHealth = GetNode<ColorRect>("Panel/VBoxContainer/MaxHealth/CurrentHealth");
		_maxPower = GetNode<ColorRect>("Panel/VBoxContainer/MaxPower");
		_currentPower = GetNode<ColorRect>("Panel/VBoxContainer/MaxPower/CurrentPower");
		_maxBrawn = GetNode<ColorRect>("Panel/VBoxContainer/MaxBrawn");
		_currentBrawn = GetNode<ColorRect>("Panel/VBoxContainer/MaxBrawn/CurrentBrawn");
		_maxCharisma = GetNode<ColorRect>("Panel/VBoxContainer/MaxCharisma");
		_currentCharisma = GetNode<ColorRect>("Panel/VBoxContainer/MaxCharisma/CurrentCharisma");
		_maxSubterfuge = GetNode<ColorRect>("Panel/VBoxContainer/MaxSubterfuge");
		_currentSubterfuge = GetNode<ColorRect>(
			"Panel/VBoxContainer/MaxSubterfuge/CurrentSubterfuge"
		);

		// Personality bars
		_honestBar = GetNode<ColorRect>("Panel/VBoxContainer/ManipulativeBar/HonestBar");
		_manipulativeBar = GetNode<ColorRect>("Panel/VBoxContainer/ManipulativeBar");
		_accommodatingBar = GetNode<ColorRect>(
			"Panel/VBoxContainer/DomineeringBar/AccomodatingBar"
		);
		_domineeringBar = GetNode<ColorRect>("Panel/VBoxContainer/DomineeringBar");
		_humanistBar = GetNode<ColorRect>("Panel/VBoxContainer/DeistBar/HumanistBar");
		_deistBar = GetNode<ColorRect>("Panel/VBoxContainer/DeistBar");

		Hide();
	}

	public void OnShowStats()
	{
		var engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		Combatant player = engine.GS.PlayerObject;

		// Set labels
		_nameLabel.Text = player.Name;
		_healthLabel.Text = $"Health {player.Health}/{player.MaxHealth}";
		_powerLabel.Text = $"Power: {player.Power}/{player.MaxPower}";
		_subterfugeLabel.Text = $"Subterfuge: {player.Subterfuge}";
		_brawnLabel.Text = $"Brawn: {player.Brawn}";
		_charismaLabel.Text = $"Charisma: {player.Charisma}";

		// Adjust inner bar widths relative to outer bars
		SetBarWidth(_currentHealth, _maxHealth, player.Health, player.MaxHealth);
		SetBarWidth(_currentPower, _maxPower, player.Power, player.MaxPower);
		SetBarWidth(_currentBrawn, _maxBrawn, player.Brawn ?? 0, 20);
		SetBarWidth(_currentCharisma, _maxCharisma, player.Charisma, 20);
		SetBarWidth(_currentSubterfuge, _maxSubterfuge, player.Subterfuge, 20);

		// Personality / traits bars (Honest vs Manipulative, Accommodating vs Domineering, Humanist vs Deist)
		if (player.Honest_Manipulative.HasValue)
		{
			SetBarWidth(_honestBar, _manipulativeBar, player.Honest_Manipulative.Value, 100);
		}
		if (player.Accommodating_Domineering.HasValue)
		{
			SetBarWidth(
				_accommodatingBar,
				_domineeringBar,
				player.Accommodating_Domineering.Value,
				100
			);
		}
		if (player.Humanist_Deist.HasValue)
		{
			SetBarWidth(_humanistBar, _deistBar, player.Humanist_Deist.Value, 100);
		}

		Show();
	}

	private void SetBarWidth(ColorRect inner, ColorRect outer, double value, double maxValue)
	{
		double ratio = Math.Clamp(value / maxValue, 0, 1);
		inner.CustomMinimumSize = new Vector2((float)(outer.Size.X * ratio), inner.Size.Y);
	}

	private void OnCloseButtonClicked()
	{
		_clickPlayer.Play();
		Hide();
	}
}
