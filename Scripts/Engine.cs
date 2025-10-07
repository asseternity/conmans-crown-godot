using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class Engine : Node
{
    public GameState GS;

    [Export]
    public string MainContainerScenePath { get; set; } = "res://Scenes/main_container.tscn";

    [Export]
    public NodePath LevelHolderPath { get; set; } = "LevelHolder";

    public void InitGame(GameState gs)
    {
        GS = gs;
    }

    public void SetPlayer(Combatant player)
    {
        if (GS == null)
            GS = new GameState(player);
        else
            GS.PlayerObject = player;
    }

    public void QuarrelRound(double playerAction)
    {
        GS.RoundLog = new List<string>();

        if (GS.CurrentQuarrel is not Quarrel quarrel)
            return;

        double enemyAction = quarrel.ChooseEnemyPower();
        GS.RoundLog.Add(quarrel.DamagePhase(GS.PlayerObject, playerAction, enemyAction));
        GS.RoundLog.Add($"You spent {playerAction}, enemy spent {enemyAction:F1}.");

        if (!GS.PlayerObject.IsAlive())
        {
            GS.RoundLog.Add("You lost the fight.");
            GS.PostQuarrelTimelinePath = GS.CurrentQuarrel.LoseTimelinePath;
            GS.CurrentQuarrel = null;
            return;
        }
        else if (!quarrel.Enemy.IsAlive())
        {
            GS.RoundLog.Add("You won the fight.");
            GS.PostQuarrelTimelinePath = GS.CurrentQuarrel.WinTimelinePath;
            GS.CurrentQuarrel = null;
            return;
        }

        GS.PostQuarrelTimelinePath = null;
        GS.RoundLog.Add(quarrel.TacticHint(quarrel.Enemy));
        quarrel.RestoreAfter(GS.PlayerObject, playerAction, enemyAction);
    }

    public async Task LoadLevelInMain(string levelScenePath)
    {
        var tree = (SceneTree)global::Godot.Engine.GetMainLoop();
        var fade = (FadeOverlay)GetNode("/root/FadeOverlay");
        if (fade != null)
            await fade.FadeOut();

        var err = tree.ChangeSceneToFile(MainContainerScenePath);
        if (err != Error.Ok)
        {
            GD.PushError($"Failed to change scene: {err}");
            if (fade != null)
                await fade.FadeIn();
            return;
        }

        // Ensure the new scene is fully in the tree
        await ToSignal(tree, SceneTree.SignalName.ProcessFrame);

        var current = tree.CurrentScene;
        if (current == null)
        {
            GD.PushError("CurrentScene is null after scene change.");
            if (fade != null)
                await fade.FadeIn();
            return;
        }

        var holder = current.GetNodeOrNull<Node>(LevelHolderPath);
        if (holder == null)
        {
            GD.PushError($"LevelHolder not found at: {LevelHolderPath}");
            if (fade != null)
                await fade.FadeIn();
            return;
        }

        // Clear old level(s)
        for (int i = holder.GetChildCount() - 1; i >= 0; i--)
        {
            holder.GetChild(i).QueueFree();
        }

        // Let the frees process
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Add new level
        var packed = GD.Load<PackedScene>(levelScenePath);
        if (packed == null)
        {
            GD.PushError($"Could not load level at: {levelScenePath}");
            if (fade != null)
                await fade.FadeIn();
            return;
        }

        holder.AddChild(packed.Instantiate());

        if (fade != null)
            await fade.FadeIn();
    }

    public void ProgressDay()
    {
        if (GS == null)
            return;

        // Advance the day
        GS.CurrentDay++;

        // Check if month should advance
        if (GS.CurrentDay > 30)
        {
            GS.CurrentDay = 1;
            GS.CurrentSeasonIndex++;

            // Wrap around at the end of the year
            if (GS.CurrentSeasonIndex >= 4)
                GS.CurrentSeasonIndex = 0;
        }

        GS.ActivityDoneToday = false;

        GD.Print($"[Engine] New Date: {GS.CurrentDay} {GS.Seasons[GS.CurrentSeasonIndex]}");
    }
}
