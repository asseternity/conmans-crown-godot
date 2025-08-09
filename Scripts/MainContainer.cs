using System;
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
		var player = Combatant.FromString("Player,5,5,10,10,0,0,res://Images/icon.svg");
		_engine.SetPlayer(player);

		// Connect Dialogic events
		if (_dialogic != null)
		{
			_dialogic.Connect("signal_event", new Callable(this, nameof(OnDialogicSignal)));
			_dialogic.Connect("timeline_ended", new Callable(this, nameof(OnTimelineEnded)));
		}

		// Call InitNPC on all NPCs
		foreach (var npc in GetTree().GetNodesInGroup("NPCs"))
		{
			if (npc is NPC npcScript)
				npcScript.InitNPC();
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
				if (typeVar.VariantType != Variant.Type.Nil && typeVar.ToString() == "duel")
				{
					var enemyString = dict.ContainsKey("enemy") ? dict["enemy"].ToString() : "";
					var winTimeline = dict.ContainsKey("win") ? dict["win"].ToString() : "";
					var loseTimeline = dict.ContainsKey("lose") ? dict["lose"].ToString() : "";

					var enemy = Combatant.FromString(enemyString);
					var duel = new Duel("duel_auto", winTimeline, loseTimeline, enemy);

					// Put duel in GameState and route to the Duel UI
					_engine.GS.CurrentDuel = duel;
					var duelUI = GetNodeOrNull<DuelUI>("UIContainer/DuelUI");
					if (duelUI != null)
						duelUI.StartDuel(duel);
					return;
				}
			}
		}
	}

	// Called when any Dialogic timeline finishes (timeline_ended)
	private void OnTimelineEnded() { }
}
