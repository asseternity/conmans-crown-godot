using System;
using Godot;

public partial class NPC : CharacterBody2D
{
	[Export]
	public string TimelinePath = "";

	[Export]
	public string SpritePath = "";
	private Engine _engine;

	private bool _playerInRange = false;

	public void InitNPC()
	{
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		GD.Print($"NPC ready. Timeline: {TimelinePath}");

		// C# event subscription syntax
		var area = GetNode<Area2D>("InteractionArea");
		area.BodyEntered += OnBodyEntered;
		area.BodyExited += OnBodyExited;

		// 🔹 Set the sprite if a path was given
		if (!string.IsNullOrEmpty(SpritePath))
		{
			var texture = GD.Load<Texture2D>(SpritePath);
			if (texture != null)
			{
				// Assuming your NPC scene has a child node named "Sprite2D"
				var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
				if (sprite != null)
				{
					sprite.Texture = texture;
					GD.Print($"NPC sprite set from {SpritePath}");
				}
				else
				{
					GD.PrintErr("Sprite2D node not found on NPC!");
				}
			}
			else
			{
				GD.PrintErr($"Failed to load texture at {SpritePath}");
			}
		}
	}

	private void OnBodyEntered(Node body)
	{
		if (body.IsInGroup("Player"))
		{
			_playerInRange = true;
		}
	}

	private void OnBodyExited(Node body)
	{
		if (body.IsInGroup("Player"))
		{
			_playerInRange = false;
		}
	}

	public override void _Process(double delta)
	{
		var duelUI = GetNode<DuelUI>("/root/MainScene/UIContainer/DuelUI");
		if (duelUI != null)
		{
			if (!duelUI.Visible)
			{
				if (_playerInRange && Input.IsActionJustPressed("interact"))
				{
					ShowDialogue();
				}
			}
		}
	}

	private void ShowDialogue()
	{
		var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");

		// Check if a timeline is currently running (Dialogic exposes current_timeline)
		var current = dialogic.Get("current_timeline");
		if (current.VariantType != Variant.Type.Nil)
			return;

		if (dialogic != null && !string.IsNullOrEmpty(TimelinePath))
		{
			dialogic.Call("start", TimelinePath);
		}
	}
}
