using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class CustomTask
{
    public bool isCompleted { get; set; }
    public string name { get; set; }
    public string description { get; set; }

    // Parameterless constructor needed for deserialization
    public CustomTask() { }

    public void CompleteTask() => isCompleted = true;

    public CustomTask(string name, string description)
    {
        this.name = name;
        this.description = description;
        isCompleted = false;
    }
}

class ToDoList
{
    public List<CustomTask> tasks;

    public ToDoList()
    {
        tasks = new List<CustomTask>();
    }

    public void AddTask(string taskName, string taskDescription)
    {
        CustomTask newTask = new CustomTask(taskName, taskDescription);
        tasks.Add(newTask);
    }

    public void ListTasks()
    {
        foreach (CustomTask task in tasks)
        {
            Console.WriteLine(
                (task.isCompleted ? "[v] " : "[_] ") + task.name + ": " + task.description
            );
        }
    }

    public void Save(string filePath)
    {
        string json = JsonSerializer.Serialize(tasks);
        File.WriteAllText(filePath, json);
    }

    public void Load(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            tasks = JsonSerializer.Deserialize<List<CustomTask>>(json) ?? new List<CustomTask>();
        }
    }
}

class Program
{
    static void Main()
    {
        string filePath = "tasks.json";
        ToDoList todo = new ToDoList();
        todo.Load(filePath);

        while (true)
        {
            Console.WriteLine("1) Add Task  2) List Tasks  3) Save & Exit");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Enter task name: ");
                string name = Console.ReadLine();

                Console.Write("Enter description: ");
                string desc = Console.ReadLine();

                todo.AddTask(name, desc);
            }
            else if (choice == "2")
            {
                todo.ListTasks();
            }
            else if (choice == "3")
            {
                todo.Save(filePath);
                break;
            }
        }
    }
}
