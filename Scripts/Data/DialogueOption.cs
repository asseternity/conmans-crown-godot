using System;
using Godot;

public class DialogueOption
{
    public string Text { get; set; }
    public Element NextElement { get; set; }

    public DialogueOption(string text, Element nextElement)
    {
        Text = text;
        NextElement = nextElement;
    }
}
