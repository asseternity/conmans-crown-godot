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
	private Label HPLabel;

	public override void _Ready()
	{
		HPLabel = GetNode<Label>("HPLabel");
		Health = MaxHealth;
		HPLabel.Text = Health.ToString() + "/" + MaxHealth.ToString();
		AddToGroup("Enemy");
	}

	public override void _PhysicsProcess(double delta)
	{
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
		HPLabel.Text = Health.ToString() + "/" + MaxHealth.ToString();
		GD.Print($"[Enemy] Took {amount} dmg. HP now {Health}/{MaxHealth}");

		if (Health <= 0)
		{
			Die();
			return;
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
