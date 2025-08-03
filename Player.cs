using System;
using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float MovementSpeed = 100f;

	public void GetInput()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * MovementSpeed;
	}

	public void _physics_process(double delta)
	{
		GetInput();
		MoveAndSlide();
	}
}
