using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public int MaxHealth = 10;
	public int Health;

	[Export]
	public int Brawn = 1;

	[Export]
	public float Speed = 60f;

	[Export]
	public float AggroRange = 120f;

	private float _invulTimer = 0f;
	private Vector2 _knockback = Vector2.Zero;
	private Panel HPAvailable;
	private Panel HPUnavailable;
	private AudioStreamPlayer _hurtPlayer;
	private AudioStreamPlayer _diePlayer;
	public bool _isDying = false;

	public override void _Ready()
	{
		_hurtPlayer = GetNode<AudioStreamPlayer>("DamagePlayer");
		_diePlayer = GetNode<AudioStreamPlayer>("DiePlayer");
		Health = MaxHealth;
		AddToGroup("Enemy");
		HPAvailable = GetNode<Panel>("Control/HPAvailable");
		HPUnavailable = GetNode<Panel>("Control/HPUnavailable");
		Vector2 newSize = new Vector2(MaxHealth, HPUnavailable.Size.Y);
		HPUnavailable.Size = newSize;
		HPAvailable.Size = newSize;

		// Recenter HP bars relative to Control
		var controlNode = GetNode<Control>("Control");
		Vector2 controlCenter = controlNode.Size / 2f;
		HPUnavailable.Position = controlCenter - (HPUnavailable.Size / 2f);
		HPAvailable.Position = HPUnavailable.Position; // align left edges
	}

	public override void _PhysicsProcess(double delta)
	{
		// disable if dying
		if (_isDying)
			return;

		// Invulnerability and knockback decay
		if (_invulTimer > 0f)
			_invulTimer -= (float)delta;

		if (_knockback.Length() > 1f)
		{
			Velocity = _knockback;
			MoveAndSlide();
			_knockback *= 0.9f; // smooth decay toward zero
			return;
		}

		// Simple follow player when within range
		var players = GetTree().GetNodesInGroup("Player");
		if (players.Count > 0)
		{
			var playerNode = players[0] as Node2D;
			if (playerNode != null)
			{
				float dist = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
				if (dist < AggroRange)
				{
					Vector2 dir = (playerNode.GlobalPosition - GlobalPosition).Normalized();
					Velocity = dir * Speed;
					MoveAndSlide();
					return;
				}
			}
		}

		// Idle
		Velocity = Vector2.Zero;
	}

	public void TakeDamage(int amount)
	{
		if (_invulTimer > 0f)
			return;

		_invulTimer = 0.2f;
		Health -= amount;
		float newWidth = ((float)Health / MaxHealth) * HPUnavailable.Size.X;
		Vector2 newSize = HPAvailable.Size;
		newSize.X = newWidth;
		HPAvailable.Size = newSize;

		// Recenter HP bars relative to Control
		var controlNode = GetNode<Control>("Control");
		Vector2 controlCenter = controlNode.Size / 2f;
		HPUnavailable.Position = controlCenter - (HPUnavailable.Size / 2f);
		HPAvailable.Position = HPUnavailable.Position; // align left edges

		if (Health <= 0)
		{
			_isDying = true;
			// Hide
			Hide();
			// disable collision
			var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (collisionShape != null)
				collisionShape.CallDeferred("set_disabled", true);
			Die();
			return;
		}
		else
		{
			_hurtPlayer.Play();
		}

		// Optional feedback: flash tween or play sound (require nodes)
		var animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (animPlayer != null && animPlayer.HasAnimation("hit"))
			animPlayer.Play("hit");
	}

	public void ApplyKnockback(Vector2 kb)
	{
		_knockback = kb;
	}

	private async void Die()
	{
		// Play death sound
		_diePlayer.Play();
		// wait asynchronously without blocking physics
		double soundLength = _diePlayer.Stream?.GetLength() ?? 0f;
		await ToSignal(GetTree().CreateTimer(soundLength), SceneTreeTimer.SignalName.Timeout);
		// Now free the node
		QueueFree();
	}
}
