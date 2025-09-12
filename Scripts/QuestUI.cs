using System;
using System.Collections.Generic;
using Godot;

public partial class QuestUI : Control
{
    private Button _closeButton;
    public ScrollContainer _scrollContainer;
    public VBoxContainer _vBoxContainer;
    private AudioStreamPlayer _clickPlayer;
    private List<Quest> currentQuests = new List<Quest>();

    public override void _Ready()
    {
        _closeButton = GetNode<Button>("Panel/CloseButton");
        _closeButton.Pressed += OnCloseButtonClicked;
        _scrollContainer = GetNode<ScrollContainer>("Panel/ScrollContainer");
        _vBoxContainer = GetNode<VBoxContainer>("Panel/ScrollContainer/VBoxContainer");
        _clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
        Hide();

        // testing
        var quest1Stages = new List<QuestStage>
        {
            new QuestStage(0, "Find the magic sword"),
            new QuestStage(1, "Return to the village elder")
        };
        var quest2Stages = new List<QuestStage>
        {
            new QuestStage(0, "Collect 5 herbs"),
            new QuestStage(1, "Brew the healing potion")
        };

        var quest1 = new Quest(1, "Hero’s Journey", quest1Stages);
        var quest2 = new Quest(2, "Herbal Remedy", quest2Stages);

        currentQuests.Add(quest1);
        currentQuests.Add(quest2);
        UpdateQuestUI();
    }

    private void OnCloseButtonClicked()
    {
        _clickPlayer.Play();
        Hide();
    }

    private void UpdateQuestUI()
    {
        foreach (Node child in _vBoxContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (currentQuests.Count == 0)
        {
            // Show "no quests" message
            var label = new Label { Text = "• no active quests" };
            _vBoxContainer.AddChild(label);
            return;
        }

        foreach (var quest in currentQuests)
        {
            string displayText = $"• {quest.Name} - {quest.Stage.Description}";
            var label = new Label { Text = displayText };
            _vBoxContainer.AddChild(label);
        }
    }
}
