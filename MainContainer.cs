using System;
using Godot;

public partial class MainContainer : Node
{
	public override void _Ready()
	{
		var engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		var gs = JsonLoader.LoadGame("res://JSON/story.json");
		engine.InitGame(gs);

		if (gs.CurrentElement is StoryLine story)
		{
			GD.Print($"Game loaded. Current: {story.Id}: {story.Text}");
		}
		else if (gs.CurrentElement is Duel duel)
		{
			GD.Print($"Game loaded. Current Duel: {duel.Id}");
		}
		else
		{
			GD.Print("Game loaded. Current element of unknown type.");
		}
	}
}
