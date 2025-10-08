using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class MainContainer : Node
{
	private Engine _engine;
	private Node? _dialogic;

	public override void _Ready()
	{
		// get autoloads
		_engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
		_dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
		if (_dialogic == null)
			GD.PrintErr(
				"Dialogic autoload not found. Make sure it is registered as AutoLoad with name 'Dialogic'."
			);

		// initialize player (replace string with your actual player data)
		var player = Combatant.FromString(
			"Player,5,5,10,10,1,1,res://downloaded_assets/10chars/spritesheets/7 idle - Portrait.png"
		);
		player.Brawn = 1;
		player.Lore = 1;
		player.Honest_Manipulative = 50;
		player.Accommodating_Domineering = 50;
		player.Humanist_Deist = 50;
		player.XP = 0;
		player.Level = 1;
		_engine.SetPlayer(player);

		// Connect Dialogic events
		if (_dialogic != null)
		{
			_dialogic.Connect("signal_event", new Callable(this, nameof(OnDialogicSignal)));
			_dialogic.Connect("timeline_ended", new Callable(this, nameof(OnTimelineEnded)));
		}
	}

	// Handler for Dialogic.signal_event
	private void OnDialogicSignal(Variant arg)
	{
		GD.Print($"[Engine] Got Dialogic signal: {arg}");
		if (arg is Variant variant && variant.VariantType == Variant.Type.Dictionary)
		{
			var dict = (Godot.Collections.Dictionary)variant;
			GD.Print("Dialogic signal type = Dictionary!");
			if (dict.ContainsKey("type"))
			{
				var typeVar = dict["type"];
				if (typeVar.VariantType != Variant.Type.Nil && typeVar.ToString() == "quarrel")
				{
					var enemyString = dict.ContainsKey("enemy") ? dict["enemy"].ToString() : "";
					var winTimeline = dict.ContainsKey("win") ? dict["win"].ToString() : "";
					var loseTimeline = dict.ContainsKey("lose") ? dict["lose"].ToString() : "";

					var enemy = Combatant.FromString(enemyString);
					var quarrel = new Quarrel("quarrel_auto", winTimeline, loseTimeline, enemy);

					// Put quarrel in GameState and route to the Quarrel UI
					_engine.GS.CurrentQuarrel = quarrel;
					var quarrelUI = GetNodeOrNull<QuarrelUI>("UIContainer/QuarrelUI");
					if (quarrelUI != null)
						quarrelUI.StartQuarrel(quarrel);
					return;
				}
			}
		}
		else if (arg.VariantType == Variant.Type.String)
		{
			string str = arg.ToString();
			if (str == "playerNameSet")
			{
				GD.Print($"[Engine] Signal type: string, playerNameSet");
				Combatant newPlayerObject = _engine.GS.PlayerObject;
				newPlayerObject.Name = GetDialogicString("Dialogues.playerName");
				_engine.GS.PlayerObject = newPlayerObject;
			}
			else if (str.StartsWith("item"))
			{
				GD.Print($"[Engine] Signal type: string, item");
				string itemID = str.Substring(5);
				var inventoryUI = GetNode<InventoryUI>("/root/MainScene/UIContainer/InventoryUI");
				Item foundItem = inventoryUI.FindItemInInventory(itemID.ToInt());
				if (foundItem != null)
				{
					string godotVariable = GetDialogicString("Items." + foundItem.ID.ToString());
					GD.Print($"[Engine] Searching for Dialogic var Items.{foundItem.ID}");
					if (!string.IsNullOrEmpty(godotVariable))
					{
						// Now actually set the variable to true
						SetDialogicVar("Items." + foundItem.ID.ToString(), true);
						GD.Print($"[Engine] Set Dialogic var Items.{foundItem.ID} = true");
					}
				}
			}
			else if (str.StartsWith("quest"))
			{
				GD.Print($"[Engine] Signal type: string, quest");
				string questID = str.Substring(6);
				var questUI = GetNode<QuestUI>("/root/MainScene/UIContainer/QuestUI");
				Quest foundQuest = questUI.FindQuest(questID.ToInt());
				if (foundQuest != null)
				{
					questUI.ProgressQuest(foundQuest);
				}
			}
		}
	}

	// Called when any Dialogic timeline finishes (timeline_ended)
	private void OnTimelineEnded()
	{
		// If your variable lives in a folder, use "FolderName.SomeVariable".
		var value = GetDialogicString("Dialogues.playerGender").ToLowerInvariant();
		GD.Print(value);
	}

	private string GetDialogicString(string path)
	{
		if (_dialogic == null)
			return "";

		// Dialogic exposes its variable subsystem as the "VAR" property.
		var varStoreV = _dialogic.Get("VAR");
		if (varStoreV.VariantType != Variant.Type.Object)
			return "";

		var varStore = varStoreV.AsGodotObject();
		if (varStore is null)
			return "";

		// subsystem_Variables has get_variable(name, default, no_warning)
		Variant v = varStore.Call("get_variable", path, "");

		return v.VariantType switch
		{
			Variant.Type.String => v.AsString(),
			Variant.Type.StringName => v.AsStringName().ToString(),
			_ => v.ToString()
		};
	}

	private void SetDialogicVar(string path, Variant value)
	{
		if (_dialogic == null)
			return;

		var varStoreV = _dialogic.Get("VAR");
		if (varStoreV.VariantType != Variant.Type.Object)
			return;

		var varStore = varStoreV.AsGodotObject();
		if (varStore is null)
			return;

		// Dialogic 2 exposes `set_variable(name, value)`
		varStore.Call("set_variable", path, value);
	}

	public List<Node> FindChildrenByName(string name)
	{
		List<Node> matching = new List<Node>();
		FindChildrenByNameRecursive(this, name, matching);
		return matching;
	}

	private void FindChildrenByNameRecursive(Node parent, string name, List<Node> list)
	{
		if (parent == null)
			return;

		foreach (Node child in parent.GetChildren())
		{
			if (child == null)
				continue;

			if (child.Name == name)
				list.Add(child);

			FindChildrenByNameRecursive(child, name, list);
		}
	}
}
