using System;
using System.Collections.Generic;
using Godot;

public partial class GameState
{
	// the currently running Duel (null when not in a duel)
	public Duel? CurrentDuel { get; set; }
	public string PostDuelTimelinePath { get; set; }

	// To persist where Dialogic was, store timeline path + event index:
	public string? CurrentTimelinePath { get; set; }
	public int CurrentEventIndex { get; set; } = 0;

	public Combatant PlayerObject { get; set; }
	public List<string> Flags { get; set; }
	public List<string> RoundLog { get; set; }
	public List<Item> Inventory { get; set; }
	public List<Quest> ActiveQuests { get; set; }
	public int CurrentDay { get; set; }
	public List<string> Seasons = new List<string> { "Spring", "Summer", "Fall", "Winter", };
	public int CurrentSeasonIndex { get; set; }
	public bool ActivityDoneToday { get; set; }

	public GameState(Combatant player)
	{
		CurrentDuel = null;
		PostDuelTimelinePath = null;
		CurrentTimelinePath = null;
		CurrentEventIndex = 0;
		PlayerObject = player;
		Flags = new List<string>();
		RoundLog = new List<string>();
		CurrentDay = 1;
		CurrentSeasonIndex = 1;
		ActivityDoneToday = false;
	}
}
