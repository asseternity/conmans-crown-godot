using System;
using Godot;

public class QuestStage
{
    public int ID { get; set; }
    public string Description { get; set; }
    public bool isCompleted { get; set; }

    public QuestStage(int id, string description)
    {
        ID = id;
        Description = description;
    }
}
