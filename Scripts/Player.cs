using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float MovementSpeed = 200f;

	private AnimationPlayer _anim;
	private AudioStreamPlayer _footstepsPlayer;
	private float _footstepCooldown = 0.3f; // 0.3 seconds between footsteps
	private float _footstepTimer = 0f; // tracks cooldown

	private string _facing = "down";

	private enum Axis
	{
		None,
		Horizontal,
		Vertical
	}

	private Axis _lastAxis = Axis.Vertical; // tie-breaker when both pressed

	// real-time combat vars
	// Attack
	[Export]
	public float AttackCooldown = 0.5f;
	private float _attackTimer = 0f;
	private Area2D? _attackArea;

	[Export]
	public float AttackActiveSeconds = 0.12f; // how long hitbox is active

	public override async void _Ready()
	{
		_anim = GetNode<AnimationPlayer>("AnimationPlayer");
		_footstepsPlayer = GetNode<AudioStreamPlayer>("FootstepsPlayer");

		// Wait a few frames to ensure Engine + GS are fully initialized
		for (int i = 0; i < 5; i++)
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		// store level path
		Engine _engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		var holder = GetNodeOrNull<Node>("/root/MainScene/LevelHolder");
		if (holder == null)
		{
			GD.PushError($"[Engine] LevelHolder not found");
		}
		_engine.GS.CurrentLevelPath = holder.GetChild(0).SceneFilePath;
		GD.Print($"[Engine] _engine.GS.CurrentLevelPath is {_engine.GS.CurrentLevelPath}");

		// optional attack area pre-existing on the player scene
		_attackArea = GetNodeOrNull<Area2D>("AttackArea");
		if (_attackArea != null)
		{
			_attackArea.Monitoring = false;
			_attackArea.BodyEntered += OnAttackBodyEntered;
		}
	}

	// Read input, but allow only ONE axis at a time
	private Vector2 GetBlockedInput()
	{
		float x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
		float y = Input.GetActionStrength("down") - Input.GetActionStrength("up");

		if (x != 0 || y != 0)
		{
			if (Mathf.Abs(x) > Mathf.Abs(y))
			{
				y = 0;
				_lastAxis = Axis.Horizontal;
			}
			else if (Mathf.Abs(y) > Mathf.Abs(x))
			{
				x = 0;
				_lastAxis = Axis.Vertical;
			}
			else
			{
				// Equal strength (diagonal) → prefer the last axis we used
				if (_lastAxis == Axis.Horizontal)
					y = 0;
				else
					x = 0;
			}
		}
		return new Vector2(x, y);
	}

	public override void _PhysicsProcess(double delta)
	{
		// Can't move if dialogic or quarrelUI or pauseUI is open
		var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
		bool dialogActive =
			dialogic != null && dialogic.Get("current_timeline").VariantType != Variant.Type.Nil;
		var quarrelUI = GetNode<QuarrelUI>("/root/MainScene/UIContainer/QuarrelUI");
		var pauseUI = GetNode<PauseMenu>("/root/MainScene/UIContainer/PauseMenu");
		bool menuOpen = quarrelUI.Visible || pauseUI.Visible;

		// Movement (no diagonals)
		var dir = GetBlockedInput();
		Velocity = dir * MovementSpeed;

		if (!dialogActive && !menuOpen)
			MoveAndSlide();

		// Footstep timer
		if (dir != Vector2.Zero)
		{
			_footstepTimer -= (float)delta;
			if (_footstepTimer <= 0f)
			{
				// Randomize pitch slightly (1.4 - 1.9)
				_footstepsPlayer.PitchScale = 1.4f + (float)GD.Randf() * 0.5f;
				_footstepsPlayer.Play();
				_footstepTimer = _footstepCooldown;
			}
		}
		else
		{
			_footstepTimer = 0f; // reset so footsteps start immediately when moving
			_footstepsPlayer.Stop();
		}

		// Attack timer
		_attackTimer -= (float)delta;
		if (Input.IsActionJustPressed("attack") && _attackTimer <= 0f && !dialogActive && !menuOpen)
		{
			_attackTimer = AttackCooldown;
			StartAttack();
		}

		// Animations
		string targetAnim;
		if (dir != Vector2.Zero)
		{
			if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
				_facing = dir.X > 0 ? "right" : "left";
			else
				_facing = dir.Y > 0 ? "down" : "up";

			targetAnim = $"walk_{_facing}";
		}
		else
		{
			targetAnim = $"idle_{_facing}";
		}

		if (_anim.CurrentAnimation != targetAnim)
			_anim.Play(targetAnim);
	}

	private async void StartAttack()
	{
		// prefer an animation; fallback to idle-facing attack name
		string attackAnim = $"attack_{_facing}";
		if (_anim.HasAnimation(attackAnim))
			_anim.Play(attackAnim);

		// Ensure we have an Area2D attack hitbox
		if (_attackArea == null)
			CreateTemporaryAttackArea();

		if (_attackArea == null)
			return;

		// Position the hitbox in front of the player based on facing
		Vector2 offset = _facing switch
		{
			"up" => new Vector2(0, -16),
			"down" => new Vector2(0, 16),
			"left" => new Vector2(-16, 0),
			"right" => new Vector2(16, 0),
			_ => new Vector2(0, 16)
		};
		_attackArea.Position = offset;
		_attackArea.Monitoring = true;
		_attackArea.Visible = true;

		// Active for a short window
		int frames = (int)System.Math.Max(1, AttackActiveSeconds * 60); // approximate frames
		for (int i = 0; i < frames; i++)
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		_attackArea.Monitoring = false;
		_attackArea.Visible = false;
	}

	private void OnAttackBodyEntered(Node body)
	{
		if (body == null)
			return;

		// Prefer strong-typed Enemy call if present
		if (body is Enemy enemy)
		{
			int damage = GetPlayerBrawn();
			enemy.TakeDamage(damage);

			// optional: small knockback
			Vector2 kb = (enemy.GlobalPosition - GlobalPosition).Normalized() * 40f;
			enemy.ApplyKnockback(kb);
		}
		else if (body.IsInGroup("Enemy"))
		{
			// try to call TakeDamage dynamically
			int damage = GetPlayerBrawn();
			body.Call("TakeDamage", damage);
		}
	}

	private void CreateTemporaryAttackArea()
	{
		// Create a lightweight Area2D as child if not present.
		_attackArea = new Area2D();
		_attackArea.Name = "AttackArea";
		_attackArea.Monitoring = false;
		_attackArea.Visible = false;

		var shape = new CollisionShape2D();
		var rect = new RectangleShape2D();
		rect.Size = new Vector2(16, 16);
		shape.Shape = rect;
		_attackArea.AddChild(shape);

		AddChild(_attackArea);
		_attackArea.BodyEntered += OnAttackBodyEntered;
	}

	private int GetPlayerBrawn()
	{
		var engine = GetTree().Root.GetNodeOrNull<Engine>("GlobalEngine");
		if (engine != null && engine.GS != null && engine.GS.PlayerObject?.Brawn != null)
			return engine.GS.PlayerObject.Brawn.Value;
		return 1;
	}
}
