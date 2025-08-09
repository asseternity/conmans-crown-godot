using System;
using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float MovementSpeed = 500f;

	public void GetInput()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * MovementSpeed;
	}

	public void _physics_process(double delta)
	{
		GetInput();

		// Check dialogic global node instead of DialogueUI only:
		var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
		bool dialogActive = false;
		if (dialogic != null)
		{
			var current = dialogic.Get("current_timeline");
			dialogActive = current.VariantType != Variant.Type.Nil;
		}

		var duelUI = GetNode<DuelUI>("/root/MainScene/UIContainer/DuelUI");

		if (!dialogActive && !duelUI.Visible)
		{
			MoveAndSlide();
		}
	}
}
