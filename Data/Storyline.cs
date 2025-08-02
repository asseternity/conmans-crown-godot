using System;
using System.Collections.Generic;
using Godot;

public class StoryLine : Element
{
    public string Id { get; set; }
    public string Text { get; set; }
    public List<DialogueOption> Options { get; set; }

    public StoryLine(string id, string text, List<DialogueOption> options)
    {
        Id = id;
        Text = text;
        Options = options;
    }
}
