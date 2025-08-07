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

		var dialogueUI = GetNode<DialogueUI>("/root/MainScene/UIContainer/DialogueUI");
		var duelUI = GetNode<DuelUI>("/root/MainScene/UIContainer/DuelUI");

		if (!dialogueUI.Visible & !duelUI.Visible)
		{
			MoveAndSlide();
		}
	}
}
