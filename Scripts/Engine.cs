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

	public void QuarrelRound(double playerAction, Combatant.Approach playerApproach)
	{
		// data and protections
		GS.RoundLog = new List<string>();
		if (GS.CurrentQuarrel is not Quarrel quarrel)
			return;
		int playerHealth = GS.PlayerObject.Health;
		int enemyHealth = quarrel.Enemy.Health;

		// choose enemy power and approach
		GS.PlayerObject.CurrentApproach = playerApproach;
		Combatant.Approach enemyApproach = quarrel.ChooseEnemyApproach(playerAction);
		quarrel.Enemy.CurrentApproach = enemyApproach;
		double enemyAction = quarrel.ChooseEnemyPower();

		// winner and damage calculations
		GS.RoundLog.Add($"You spent {playerAction}, enemy spent {enemyAction:F1}.");
		GS.RoundLog.Add(quarrel.DamagePhase(GS.PlayerObject, playerAction, enemyAction));

		// check if the battle is over
		if (!GS.PlayerObject.IsAlive())
		{
			GS.RoundLog.Add("You lost the fight.");
			quarrel.EnemyPowerRestorationBonus = 0;
			quarrel.PlayerPowerRestorationBonus = 0;
			GS.PlayerObject.Charisma = GS.PlayerObject.Charisma + quarrel.PlayerCharismaPenalty;
			quarrel.PlayerCharismaPenalty = 0;
			quarrel.Enemy.Charisma = quarrel.Enemy.Charisma + quarrel.EnemyCharismaPenalty;
			quarrel.EnemyCharismaPenalty = 0;
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

		// next round preparation
		if (playerHealth != GS.PlayerObject.Health && enemyHealth == quarrel.Enemy.Health)
		{
			GS.RoundLog.Add(quarrel.ApplyEffects(quarrel.Enemy, GS.PlayerObject));
		}
		else if (playerHealth == GS.PlayerObject.Health && enemyHealth != quarrel.Enemy.Health)
		{
			GS.RoundLog.Add(quarrel.ApplyEffects(GS.PlayerObject, quarrel.Enemy));
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

	public void UpdateDialogicPlayerStats()
	{
		SetDialogicVar("PlayerStats.brawn", GS.PlayerObject.Brawn ?? 0);
		SetDialogicVar("PlayerStats.charisma", GS.PlayerObject.Charisma);
		SetDialogicVar("PlayerStats.subterfuge", GS.PlayerObject.Subterfuge);
		SetDialogicVar("PlayerStats.lore", GS.PlayerObject.Lore ?? 0);
		SetDialogicVar("PlayerStats.honest-manipulative", GS.PlayerObject.Honest_Manipulative ?? 0);
		SetDialogicVar(
			"PlayerStats.accommodating-domineering",
			GS.PlayerObject.Accommodating_Domineering ?? 0
		);
		SetDialogicVar("PlayerStats.humanist-deist", GS.PlayerObject.Humanist_Deist ?? 0);
	}

	public void SetDialogicVar(string path, Variant value)
	{
		Node? _dialogic = GetTree().Root.GetNodeOrNull("Dialogic");
		if (_dialogic == null)
			return;

		var varStoreV = _dialogic.Get("VAR");
		if (varStoreV.VariantType != Variant.Type.Object)
			return;

		var varStore = varStoreV.AsGodotObject();
		if (varStore is null)
			return;

		// Dialogic 2 exposes `set_variable(name, value)`
		varStore.Call("set_variable", path, value);
	}

	// Update one stat in both PlayerObject and Dialogic
	public void ModifyBrawn(int delta)
	{
		GS.PlayerObject.Brawn = (GS.PlayerObject.Brawn ?? 0) + delta;
		SetDialogicVar("PlayerStats.brawn", GS.PlayerObject.Brawn ?? 0);
	}

	public void ModifyCharisma(int delta)
	{
		GS.PlayerObject.Charisma = GS.PlayerObject.Charisma + delta;
		SetDialogicVar("PlayerStats.charisma", GS.PlayerObject.Charisma);
	}

	public void ModifySubterfuge(int delta)
	{
		GS.PlayerObject.Subterfuge = GS.PlayerObject.Subterfuge + delta;
		SetDialogicVar("PlayerStats.subterfuge", GS.PlayerObject.Subterfuge);
	}

	public void ModifyLore(int delta)
	{
		GS.PlayerObject.Lore = GS.PlayerObject.Lore + delta;
		SetDialogicVar("PlayerStats.lore", GS.PlayerObject.Lore ?? 0);
	}

	public void ModifyHonestManipulative(int delta)
	{
		GS.PlayerObject.Honest_Manipulative = (GS.PlayerObject.Honest_Manipulative ?? 0) + delta;
		SetDialogicVar("PlayerStats.honest-manipulative", GS.PlayerObject.Honest_Manipulative ?? 0);
	}

	public void ModifyAccommodatingDomineering(int delta)
	{
		GS.PlayerObject.Accommodating_Domineering =
			(GS.PlayerObject.Accommodating_Domineering ?? 0) + delta;
		SetDialogicVar(
			"PlayerStats.accommodating-domineering",
			GS.PlayerObject.Accommodating_Domineering ?? 0
		);
	}

	public void ModifyHumanistDeist(int delta)
	{
		GS.PlayerObject.Humanist_Deist = (GS.PlayerObject.Humanist_Deist ?? 0) + delta;
		SetDialogicVar("PlayerStats.humanist-deist", GS.PlayerObject.Humanist_Deist ?? 0);
	}
}
