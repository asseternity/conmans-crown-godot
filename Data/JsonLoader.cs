using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

public static class JsonLoader
{
	public static GameState LoadGame(string path)
	{
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		var json = file.GetAsText();
		file.Close();

		var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		// Load player
		Combatant player = Combatant.FromString(root.GetProperty("player").GetString());

		// Load storylines
		var storyById = new Dictionary<string, StoryLine>();
		foreach (var storyElem in root.GetProperty("storylines").EnumerateArray())
		{
			var id = storyElem.GetProperty("id").GetString();
			var text = storyElem.GetProperty("text").GetString();
			var new_storyline_object = new StoryLine(id, text, new List<DialogueOption>());
			storyById[id] = new_storyline_object;
		}

		// Load duels
		var duelById = new Dictionary<string, Duel>();
		foreach (var duelElem in root.GetProperty("duels").EnumerateArray())
		{
			var id = duelElem.GetProperty("id").GetString();
			Combatant enemy = Combatant.FromString(duelElem.GetProperty("enemy").GetString());
			string winId = duelElem.GetProperty("win_id").GetString();
			string loseId = duelElem.GetProperty("lose_id").GetString();
			var winStory = storyById.ContainsKey(winId)
				? storyById[winId]
				: new StoryLine("ID: Missing", "Text: Missing", new List<DialogueOption>());
			var loseStory = storyById.ContainsKey(loseId)
				? storyById[loseId]
				: new StoryLine("ID: Missing", "Text: Missing", new List<DialogueOption>());
			duelById[id] = new Duel(id, winStory, loseStory, enemy);
		}

		// Merge
		var elementsById = new Dictionary<string, Element>();
		foreach (var x in storyById)
		{
			elementsById[x.Key] = x.Value;
		}
		foreach (var x in duelById)
		{
			elementsById[x.Key] = x.Value;
		}

		// Now link options
		foreach (var storyElem in root.GetProperty("storylines").EnumerateArray())
		{
			var id = storyElem.GetProperty("id").GetString();
			var optionsList = new List<DialogueOption>();
			foreach (var opt in storyElem.GetProperty("options").EnumerateArray())
			{
				string text = opt.GetProperty("text").GetString();
				string nextId = opt.GetProperty("next_id").GetString();
				var next = elementsById.ContainsKey(nextId)
					? elementsById[nextId]
					: new StoryLine("ID: Missing", "Text: Missing", new List<DialogueOption>());
				optionsList.Add(new DialogueOption(text, next));
			}
			storyById[id].Options = optionsList;
		}

		return new GameState(elementsById.Values.First(), elementsById, player);
	}
}
