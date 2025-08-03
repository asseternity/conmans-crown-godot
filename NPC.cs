using System;
using Godot;

public partial class NPC : CharacterBody2D
{
	[Export]
	public string StoryID = "";
	public Element StartElement;
	private Engine _engine;

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
	}
}
