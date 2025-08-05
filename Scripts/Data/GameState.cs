using System;
using System.Collections.Generic;
using Godot;

public partial class GameState
{
	public Element CurrentElement { get; set; }
	public Dictionary<string, Element> AllElements { get; set; }
	public Combatant PlayerObject { get; set; }
	public List<string> Flags { get; set; }
	public List<string> FullLog { get; set; }

	public GameState(
		Element currentElement,
		Dictionary<string, Element> allElements,
		Combatant playerObject
	)
	{
		CurrentElement = currentElement;
		AllElements = allElements;
		PlayerObject = playerObject;
		Flags = new List<string>();
		FullLog = new List<string>();
	}
}
