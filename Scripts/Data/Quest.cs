using System;
using System.Collections.Generic;
using Godot;

public class Quest
{
    public int ID { get; set; }
    public string Name { get; set; }
    List<QuestStage> QuestStages = new List<QuestStage>();
    public QuestStage Stage { get; set; }
    public bool FullyFinished { get; set; }

    public Quest(int id, string name, List<QuestStage> questStages)
    {
        ID = id;
        Name = name;
        QuestStages = questStages;
        Stage = questStages[0];
        FullyFinished = false;
    }
}
