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
					float currentWidth = GetWidth(sprite);
					sprite.Texture = texture;
					ResizeWidth(sprite, currentWidth);
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

	private float GetWidth(Sprite2D sprite2D)
	{
		// Base size in pixels from the texture
		Vector2 textureSize = sprite2D.Texture.GetSize();
		// The final width on screen (includes scale)
		return textureSize.X * sprite2D.Scale.X;
	}

	private void ResizeWidth(Sprite2D sprite2D, float targetWidth)
	{
		float aspect = sprite2D.Texture.GetSize().Y / sprite2D.Texture.GetSize().X;
		sprite2D.Scale = new Vector2(
			targetWidth / sprite2D.Texture.GetSize().X,
			(targetWidth * aspect) / sprite2D.Texture.GetSize().Y
		);
	}
}
