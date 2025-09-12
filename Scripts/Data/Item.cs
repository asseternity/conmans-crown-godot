using System;
using Godot;

public class Item
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public string Effect { get; set; }

    public Item(int id, string name, string description, string iconPath, string effect)
    {
        ID = id;
        Name = name;
        Description = description;
        IconPath = iconPath;
        Effect = effect;
    }
}
