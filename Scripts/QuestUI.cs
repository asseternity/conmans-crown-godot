using System;
using System.Collections.Generic;
using Godot;

public partial class QuestUI : Control
{
    private Button _closeButton;
    public ScrollContainer _scrollContainer;
    public VBoxContainer _vBoxContainer;
    private AudioStreamPlayer _clickPlayer;
    public List<Quest> currentQuests = new List<Quest>();
    public List<Quest> allQuests = new List<Quest>();

    public override void _Ready()
    {
        _closeButton = GetNode<Button>("Panel/CloseButton");
        _closeButton.Pressed += OnCloseButtonClicked;
        _scrollContainer = GetNode<ScrollContainer>("Panel/ScrollContainer");
        _vBoxContainer = GetNode<VBoxContainer>("Panel/ScrollContainer/VBoxContainer");
        _clickPlayer = GetNode<AudioStreamPlayer>("ClickPlayer");
        Hide();
        UpdateQuestUI();
        CreateQuests();
    }

    private void CreateQuests()
    {
        // --- quest 1 ---
        var quest1Stages = new List<QuestStage>
        {
            new QuestStage(0, "Find the magic sword"),
            new QuestStage(1, "Return to the village elder")
        };
        var quest1 = new Quest(1, "Hero’s Journey", quest1Stages);
        allQuests.Add(quest1);

        // --- quest 2 ---
        var quest2Stages = new List<QuestStage>
        {
            new QuestStage(0, "Collect 5 herbs"),
            new QuestStage(1, "Brew the healing potion")
        };
        var quest2 = new Quest(2, "Herbal Remedy", quest2Stages);
        allQuests.Add(quest2);
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
            string displayText =
                $"• {quest.Name} - {quest.QuestStages[quest.CurrentStageID].Description}";
            var label = new Label { Text = displayText };
            _vBoxContainer.AddChild(label);
        }
    }

    public void ProgressQuest(Quest quest)
    {
        // [_] hook up progress quest when picking up items

        // if the quest is already active => progress one stage or complete it if the stage is final
        for (int i = 0; i < currentQuests.Count; i++)
        {
            if (currentQuests[i].ID == quest.ID)
            {
                var gameUI = GetNode<GameUI>("/root/MainScene/UIContainer/GameUI");
                if (gameUI != null)
                {
                    gameUI.ShowQuestNotification(
                        "Task Completed:"
                            + currentQuests[i]
                                .QuestStages[currentQuests[i].CurrentStageID]
                                .Description
                    );
                }
                if (currentQuests[i].CurrentStageID == currentQuests[i].QuestStages.Count - 1)
                {
                    currentQuests[i].FullyFinished = true;
                }
                else
                {
                    currentQuests[i].CurrentStageID++;
                }
                UpdateQuestUI();
                return;
            }
        }
        // if the quest is not active yet => move it to active quests
        for (int j = 0; j < allQuests.Count; j++)
        {
            if (allQuests[j].ID == quest.ID)
            {
                var gameUI = GetNode<GameUI>("/root/MainScene/UIContainer/GameUI");
                if (gameUI != null)
                {
                    gameUI.ShowQuestNotification(
                        "Task Acquired:" + allQuests[j].QuestStages[0].Description
                    );
                }
                currentQuests.Add(allQuests[j]);
                UpdateQuestUI();
                return;
            }
        }
    }

    public Quest FindQuest(int questID)
    {
        for (int i = 0; i < allQuests.Count; i++)
        {
            if (allQuests[i].ID == questID)
            {
                return allQuests[i];
            }
        }
        return null;
    }
}
