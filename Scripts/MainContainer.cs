using System;
using Godot;

public partial class MainContainer : Node
{
	public override void _Ready()
	{
		// get engine
		var engine = GetTree().Root.GetNode<Engine>("GlobalEngine");

		// load JSON
		var gs = JsonLoader.LoadGame("res://JSON/story.json");

		// push the loaded JSON into the engine
		engine.InitGame(gs);

		// testing: immediately show data
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

		// Call InitNPC on all NPCs
		foreach (var npc in GetTree().GetNodesInGroup("NPCs"))
		{
			if (npc is NPC npcScript)
			{
				npcScript.InitNPC();
			}
		}
	}

	public void RouteElement(Element element)
	{
		var dialogueUI = GetNode<DialogueUI>("UIContainer/DialogueUI");
		var duelUI = GetNode<DuelUI>("UIContainer/DuelUI");
		if (element is StoryLine story)
		{
			if (story.Id != "ID: Missing")
			{
				duelUI.Hide();
				dialogueUI.ShowStory(story);
			}
			else
			{
				dialogueUI.Hide();
				duelUI.Hide();
			}
		}
		else if (element is Duel duel)
		{
			dialogueUI.Hide();
			duelUI.StartDuel(duel);
		}
	}
}
