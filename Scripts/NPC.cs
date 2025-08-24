using System;
using Godot;

public partial class NPC : CharacterBody2D
{
    [Export]
    public string TimelinePath = "";
    private Engine _engine;

    private bool _playerInRange = false;

    public void InitNPC()
    {
        _engine = GetTree().Root.GetNode<Engine>("GlobalEngine");
        GD.Print($"NPC ready. Timeline: {TimelinePath}");

        // C# event subscription syntax
        var area = GetNode<Area2D>("InteractionArea");
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("Player"))
        {
            _playerInRange = true;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body.IsInGroup("Player"))
        {
            _playerInRange = false;
        }
    }

    public override void _Process(double delta)
    {
        var duelUI = GetNode<DuelUI>("/root/MainScene/UIContainer/DuelUI");
        if (duelUI != null)
        {
            if (!duelUI.Visible)
            {
                if (_playerInRange && Input.IsActionJustPressed("interact"))
                {
                    ShowDialogue();
                }
            }
        }
    }

    private void ShowDialogue()
    {
        var dialogic = GetTree().Root.GetNodeOrNull("Dialogic");

        // Optional: check if a timeline is currently running (Dialogic exposes current_timeline)
        var current = dialogic.Get("current_timeline");
        if (current.VariantType != Variant.Type.Nil)
            return;

        if (dialogic != null && !string.IsNullOrEmpty(TimelinePath))
        {
            dialogic.Call("start", TimelinePath);
        }
    }
}
