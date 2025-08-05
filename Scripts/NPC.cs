using System;
using Godot;

public partial class NPC : CharacterBody2D
{
	[Export]
	public string StoryID = "";
	public Element StartElement;
	private Engine _engine;

	private bool _playerInRange = false;

	public void InitNPC()
	{
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		StartElement = _engine.FindElementByID(StoryID);
		if (StartElement is StoryLine s)
		{
			GD.Print($"NPC loaded with the following story: {s.Text}");
		}
		else
		{
			GD.Print($"Loaded with something else: {StartElement}");
		}

		// C# event subscription syntax
		var area = GetNode<Area2D>("InteractionArea");
		area.BodyEntered += OnBodyEntered;
		area.BodyExited += OnBodyExited;
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
		if (_playerInRange && Input.IsActionJustPressed("interact"))
		{
			ShowDialogue();
		}
	}

	private void ShowDialogue()
	{
		var dialogueUI = GetTree().Root.GetNode<DialogueUI>("MainScene/UIContainer/DialogueUI");
		if (StartElement is StoryLine s)
		{
			dialogueUI.Show();
			dialogueUI.ShowStory(s);
		}
	}
}
