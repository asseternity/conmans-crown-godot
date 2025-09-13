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

	public override void _Ready()
	{
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");

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
					float targetWidth = 16f; // always make NPCs 32px wide
					ApplyTextureAndResize(sprite, texture, targetWidth);
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

	private void ApplyTextureAndResize(Sprite2D sprite, Texture2D texture, float targetWidth)
	{
		sprite.RegionEnabled = false;
		sprite.Hframes = 1;
		sprite.Vframes = 1;
		sprite.Frame = 0;

		sprite.Texture = texture;

		var texSize = texture?.GetSize() ?? Vector2.Zero;
		if (texSize.X <= 0)
			return;

		float scaleFactor = targetWidth / texSize.X;
		sprite.Scale = new Vector2(scaleFactor, scaleFactor);
	}
}
