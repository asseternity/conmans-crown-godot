using System;
using Godot;

public partial class LevelApartment : Node2D
{
    public string IntroTimelinePath = "res://dialogic/Timelines/intro.dtl";
    private Node? _dialogic;

    public override void _Ready()
    {
        // Start intro dialogue
        var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
        dialogic.Call("start", IntroTimelinePath);
    }
}
