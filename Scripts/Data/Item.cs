using System;
using Godot;

public class Item
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string IconPath { get; set; }

    public Item(int id, string name, string iconPath)
    {
        ID = id;
        Name = name;
        IconPath = iconPath;
    }
}
