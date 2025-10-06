using System;
using System.Collections.Generic;
using Godot;

public partial class QuarrelUI : Control
{
	private Label _logLabel;
	private Label _powerLabel;
	private HSlider _powerSlider;
	private Panel _powerAvailable;
	private Button _attackButton;
	private Label _playerNameLabel;
	private Label _enemyNameLabel;
	private ProgressBar _playerHP;
	private ProgressBar _enemyHP;
	private Label _playerHPText;
	private Label _enemyHPText;
	private TextureRect _playerSprite;
	private TextureRect _enemySprite;
	private AudioStreamPlayer _slashSound;
	private AudioStreamPlayer _clickSound;
	private Quarrel _currentQuarrel;
	private Engine _engine;

	public override void _Ready()
	{
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		_logLabel = GetNode<Label>("LogPanel/CombatLogLabel");
		_powerLabel = GetNode<Label>("ActionPanel/PowerLabel");
		_powerSlider = GetNode<HSlider>("ActionPanel/PowerSlider");
		_powerAvailable = GetNode<Panel>("ActionPanel/PowerSlider/PowerAvailable");
		_attackButton = GetNode<Button>("ActionPanel/AttackButton");
		_playerNameLabel = GetNode<Label>("PlayerPanel/PlayerSide/PlayerName");
		_enemyNameLabel = GetNode<Label>("EnemyPanel/EnemySide/EnemyName");
		_playerHP = GetNode<ProgressBar>("PlayerPanel/PlayerSide/PlayerHP");
		_enemyHP = GetNode<ProgressBar>("EnemyPanel/EnemySide/EnemyHP");
		_playerHPText = GetNode<Label>("PlayerPanel/PlayerSide/PlayerHP/PlayerHPText");
		_enemyHPText = GetNode<Label>("EnemyPanel/EnemySide/EnemyHP/EnemyHPText");
		_playerSprite = GetNode<TextureRect>("PlayerPanel/PlayerSide/PlayerSprite");
		_enemySprite = GetNode<TextureRect>("EnemyPanel/EnemySide/EnemySprite");
		_slashSound = GetNode<AudioStreamPlayer>("SlashSound");
		_clickSound = GetNode<AudioStreamPlayer>("ClickSound");

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

	public void StartQuarrel(Quarrel quarrel)
	{
		var popup = GetNode<PopupUI>("/root/MainScene/UIContainer/PopupUI");
		if (!popup.HasShown("quarrel_intro_1"))
		{
			List<string> tutorial_1 = new List<string>
			{
				"You’ve entered a quarrel — a tense dance of choices. Every move balances bold strikes against patient recovery. Win by outlasting your foe’s strength and wits.",
				"Your Power fuels attacks. Use the slider to choose how much to spend. Spend lightly (<3) and you’ll recover some Power next turn. Go all-in, and you risk running dry.",
				"Hit Attack when you’ve chosen your Power. High numbers swing hard, but leave you exhausted. Small strikes are safer — and help Power return."
			};
			popup.ShowTutorial("quarrel_intro_1", tutorial_1);
		}

		// clear gamestate log (Fixed C# initializer)
		string startingLog =
			$"You brandish your weapon. {quarrel.Enemy.Name} stands ready. It's a quarrel!";
		_engine.GS.RoundLog = new List<string> { startingLog };

		// assign data
		_currentQuarrel = quarrel;
		_engine.GS.CurrentQuarrel = quarrel; // ensure GameState points to quarrel

		_playerNameLabel.Text = _engine.GS.PlayerObject.Name;
		_enemyNameLabel.Text = quarrel.Enemy.Name;

		_playerHP.MaxValue = _engine.GS.PlayerObject.MaxHealth;
		_enemyHP.MaxValue = quarrel.Enemy.MaxHealth;

		_playerSprite.Texture = GD.Load<Texture2D>(_engine.GS.PlayerObject.QuarrelSpritePath);
		_enemySprite.Texture = GD.Load<Texture2D>(quarrel.Enemy.QuarrelSpritePath);

		_powerAvailable.Visible = true;
		_powerSlider.Visible = true;
		_powerLabel.Visible = true;

		UpdateLog();
		UpdateHealthBars();
		_powerSlider.MaxValue = _engine.GS.PlayerObject.MaxPower;
		_powerSlider.Value = Math.Min(1.0, _powerSlider.MaxValue);
		UpdatePowerAvailable();
		Show();
	}

	public void OnPowerSliderChanged(double value)
	{
		_clickSound.Play();
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
		ShakeUI();
		_attackButton.Disabled = true;
		double powerUsed = Math.Round(_powerSlider.Value, 1);
		_engine.QuarrelRound(powerUsed);

		UpdateLog();
		UpdateHealthBars();

		double currentPower = _engine.GS.PlayerObject.Power;
		bool valid = _powerSlider.Value <= currentPower;
		_attackButton.Disabled = !valid;
		_powerLabel.Text = $"Power: {_powerSlider.Value:F1} {(valid ? "" : "(Not enough!)")}";

		// if the battle is over, rebind buttons and disable the slider
		if (_engine.GS.CurrentQuarrel == null) // quarrel ended -> Engine cleared it
		{
			_powerAvailable.Visible = false;
			_powerSlider.Visible = false;
			_powerLabel.Visible = false;
			_attackButton.Pressed -= OnAttackButtonPressed;
			_attackButton.Pressed += RouteAfterQuarrel;
		}

		_powerSlider.MaxValue = _engine.GS.PlayerObject.MaxPower;
		_powerSlider.Value = Math.Min(1.0, _powerSlider.MaxValue);
		UpdatePowerAvailable();

		_attackButton.Disabled = false;

		var popup = GetNode<PopupUI>("/root/MainScene/UIContainer/PopupUI");
		if (!popup.HasShown("quarrel_intro_2"))
		{
			List<string> tutorial_2 = new List<string>
			{
				"Damage isn’t about who hit harder — it’s about who spent less. The loser’s Power spent decides the wound.",
				"Your HP bar tracks your life. If it hits zero, the quarrel is lost. Keep an eye on both your own bar and your foe’s!",
				"The Combat Log tells the story of each clash. Watch it to learn your enemy’s habits and adjust your tactics.",
				"Alright, enough chatter. You’ve got the tools — now go mess him up!"
			};
			popup.ShowTutorial("quarrel_intro_2", tutorial_2);
		}
	}

	public async void UpdateLog()
	{
		_logLabel.Text = string.Join("\n", _engine.GS.RoundLog);

		// Wait for three frames so the UI can update first
		await ToSignal(GetTree(), "process_frame");
		await ToSignal(GetTree(), "process_frame");
		await ToSignal(GetTree(), "process_frame");
	}

	public void UpdateHealthBars()
	{
		_playerHP.Value = _engine.GS.PlayerObject.Health;
		_enemyHP.Value = _currentQuarrel.Enemy.Health;

		_playerHPText.Text =
			$"{_engine.GS.PlayerObject.Health.ToString()}/{_engine.GS.PlayerObject.MaxHealth.ToString()}";
		_enemyHPText.Text =
			$"{_currentQuarrel.Enemy.Health.ToString()}/{_currentQuarrel.Enemy.MaxHealth.ToString()}";
	}

	private void UpdatePowerAvailable()
	{
		double currentPower = _engine.GS.PlayerObject.Power;
		double maxPower = _engine.GS.PlayerObject.MaxPower;

		// Panel width in pixels
		float maxWidthPixels = 633f; // Max width for full power bar
		float newWidth = (float)(currentPower / maxPower) * maxWidthPixels;

		// Apply new minimum size
		var size = _powerAvailable.CustomMinimumSize;
		size.X = newWidth;
		_powerAvailable.CustomMinimumSize = size;
	}

	private void RouteAfterQuarrel()
	{
		Hide();
		var main = GetTree().Root.GetNode<MainContainer>("MainScene");
		_attackButton.Pressed -= RouteAfterQuarrel;

		var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
		if (dialogic != null)
			dialogic.Call("start", _engine.GS.PostQuarrelTimelinePath);
	}

	private void ShakeUI()
	{
		Tween tween = GetTree().CreateTween();
		_clickSound.Play();
		Vector2 originalUIPosition = Position;
		tween.TweenProperty(this, "position:y", originalUIPosition.Y - 10, 0.05);
		tween.TweenProperty(this, "position:y", originalUIPosition.Y + 10, 0.1);
		tween.TweenProperty(this, "position:y", originalUIPosition.Y, 0.05);
		_slashSound.Play();
	}
}
