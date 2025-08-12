using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float MovementSpeed = 200f;

	private AnimationPlayer _anim;
	private string _facing = "down";

	private enum Axis
	{
		None,
		Horizontal,
		Vertical
	}

	private Axis _lastAxis = Axis.Vertical; // tie-breaker when both pressed

	public override void _Ready()
	{
		_anim = GetNode<AnimationPlayer>("AnimationPlayer");
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
		// Can't move if dialogic or duelUI is open
		var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
		bool dialogActive =
			dialogic != null && dialogic.Get("current_timeline").VariantType != Variant.Type.Nil;
		var duelUI = GetNode<DuelUI>("/root/MainScene/UIContainer/DuelUI");

		// Movement (no diagonals)
		var dir = GetBlockedInput();
		Velocity = dir * MovementSpeed;

		if (!dialogActive && !duelUI.Visible)
			MoveAndSlide();

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
}
