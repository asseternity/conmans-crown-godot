using System;
using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float MovementSpeed = 200f;

	private AnimationPlayer _anim;
	private AudioStreamPlayer _footstepsPlayer;
	private AudioStreamPlayer _hurtPlayer;
	private AudioStreamPlayer _shootPlayer;
	private float _footstepCooldown = 0.3f;
	private float _footstepTimer = 0f;

	private string _facing = "down";

	private enum Axis
	{
		None,
		Horizontal,
		Vertical
	}

	private Axis _lastAxis = Axis.Vertical;

	// Projectile combat vars
	[Export]
	public PackedScene ProjectileScene = null; // optional: can preload a PackedScene for Projectile

	[Export]
	public float ProjectileSpeed = 800f;

	[Export]
	public float ProjectileMaxDistance = 1000f;

	[Export]
	public float ProjectileRadius = 2f;

	[Export]
	public float AttackCooldown = 0.25f; // quicker since it's ranged
	private float _attackTimer = 0f;

	// Hurt / invulnerability
	[Export]
	public float InvulnerableSeconds = 0.2f;
	private Vector2 _knockback = Vector2.Zero;

	[Export]
	public float HurtboxRadius = 14f; // slightly bigger than player's collision
	private bool _invulnerable = false;

	public override async void _Ready()
	{
		_anim = GetNode<AnimationPlayer>("AnimationPlayer");
		_footstepsPlayer = GetNode<AudioStreamPlayer>("FootstepsPlayer");
		_hurtPlayer = GetNode<AudioStreamPlayer>("DamagePlayer");
		_shootPlayer = GetNode<AudioStreamPlayer>("ShootPlayer");

		// small frame delay
		for (int i = 0; i < 5; i++)
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		// global engine logic
		Engine _engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		var holder = GetNodeOrNull<Node>("/root/MainScene/LevelHolder");
		if (holder == null)
			GD.PushError($"[Engine] LevelHolder not found");
		else
			_engine.GS.CurrentLevelPath = holder.GetChild(0).SceneFilePath;

		// Ensure Hurtbox exists (bigger than collision)
		var hurtbox = GetNodeOrNull<Area2D>("Hurtbox");
		if (hurtbox == null)
			CreateHurtbox();
		else
		{
			hurtbox.Monitoring = true;
			hurtbox.BodyEntered += OnHurtboxBodyEntered;
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
		var engine = GetTree().Root.GetNodeOrNull<Engine>("GlobalEngine");
		if (engine.GS.PlayerObject.Health <= 0)
		{
			Die();
		}

		if (_knockback.Length() > 1f)
		{
			Velocity = _knockback;
			MoveAndSlide();
			_knockback *= 0.9f; // smooth decay toward zero
			return;
		}

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
				_footstepsPlayer.PitchScale = 1.4f + (float)GD.Randf() * 0.5f;
				_footstepsPlayer.Play();
				_footstepTimer = _footstepCooldown;
			}
		}
		else
		{
			_footstepTimer = 0f;
			_footstepsPlayer.Stop();
		}

		// Attack input -> SHOOT projectile
		_attackTimer -= (float)delta;
		if (Input.IsActionJustPressed("attack") && _attackTimer <= 0f && !dialogActive && !menuOpen)
		{
			_attackTimer = AttackCooldown;
			ShootProjectile();
		}

		// Animations + facing
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

	private void ShootProjectile()
	{
		// optional animation
		string attackAnim = $"attack_{_facing}";
		if (_anim.HasAnimation(attackAnim))
			_anim.Play(attackAnim);

		// instantiate Projectile either from PackedScene or create programmatically
		Projectile proj = null;
		if (ProjectileScene != null)
		{
			_shootPlayer.Play();
			var inst = ProjectileScene.Instantiate();
			if (inst is Projectile p)
				proj = p;
			else
			{
				// if scene is generic Node, try to cast
				proj = inst as Projectile;
			}
		}
		else
		{
			// create new Projectile instance programmatically
			proj = new Projectile();
		}

		if (proj == null)
		{
			GD.PrintErr(
				"Failed to create projectile (ensure ProjectileScene is a Projectile or class exists)"
			);
			return;
		}

		// set projectile params
		proj.Speed = ProjectileSpeed;
		proj.MaxDistance = ProjectileMaxDistance;
		proj.Radius = ProjectileRadius;
		int dmg = GetPlayerBrawn();
		Vector2 dirVec = _facing switch
		{
			"up" => new Vector2(0, -1),
			"down" => new Vector2(0, 1),
			"left" => new Vector2(-1, 0),
			"right" => new Vector2(1, 0),
			_ => new Vector2(0, 1)
		};

		proj.Position = GlobalPosition; // spawn at player center; tweak offset if needed
		proj.Initialize(dirVec, dmg);

		// add projectile to the player's parent (so it exists in the same canvas layer)
		GetParent().AddChild(proj);
	}

	private void CreateHurtbox()
	{
		var hb = new Area2D();
		hb.Name = "Hurtbox";
		hb.Monitoring = true;
		hb.Visible = false;

		var cs = new CollisionShape2D();
		cs.Name = "CollisionShape2D";
		var circle = new CircleShape2D();
		circle.Radius = HurtboxRadius;
		cs.Shape = circle;
		hb.AddChild(cs);

		AddChild(hb);
		hb.BodyEntered += OnHurtboxBodyEntered;
	}

	private void OnHurtboxBodyEntered(Node body)
	{
		if (body == null)
			return;

		if (_invulnerable)
			return;

		// Example: enemy collision
		if (body is Enemy enemy && !enemy._isDying)
		{
			enemy.ApplyKnockback((enemy.GlobalPosition - GlobalPosition).Normalized() * 40f);
			ApplyKnockback((GlobalPosition - enemy.GlobalPosition).Normalized() * 40f);
			ReceiveHit(enemy.Brawn);
			return;
		}
	}

	public void ApplyKnockback(Vector2 kb)
	{
		_knockback = kb;
	}

	private void ReceiveHit(int damage)
	{
		_hurtPlayer.Play();
		var engine = GetTree().Root.GetNodeOrNull<Engine>("GlobalEngine");
		engine.GS.PlayerObject.Health = engine.GS.PlayerObject.Health - damage;
		StartInvulnerability(InvulnerableSeconds);
	}

	private async void StartInvulnerability(float seconds)
	{
		_invulnerable = true;

		float blinkInterval = 0.05f;
		int loops = Math.Max(1, (int)Math.Ceiling(seconds / blinkInterval));
		for (int i = 0; i < loops; i++)
		{
			// alternate modulate between red and normal
			Modulate = (i % 2 == 0) ? new Color(1f, 0.5f, 0.5f) : new Color(1f, 1f, 1f);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		Modulate = new Color(1f, 1f, 1f);
		_invulnerable = false;
	}

	private int GetPlayerBrawn()
	{
		var engine = GetTree().Root.GetNodeOrNull<Engine>("GlobalEngine");
		if (engine != null && engine.GS != null && engine.GS.PlayerObject?.Brawn != null)
			return engine.GS.PlayerObject.Brawn.Value;
		return 1;
	}

	private void Die()
	{
		// Drop loot, play death animation, then free
		var deathAnim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (deathAnim != null && deathAnim.HasAnimation("die"))
		{
			deathAnim.Play("die");
			// Schedule free after animation length
			float wait = (float)deathAnim.CurrentAnimationLength;
			CallDeferred(nameof(QueueFree));
		}
		else
		{
			QueueFree();
		}
	}
}
