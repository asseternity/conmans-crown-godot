using System;
using System.Collections.Generic;
using Godot;

public partial class Engine : Node
{
	public GameState GS;

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

	public void DuelRound(double playerAction)
	{
		if (GS.CurrentDuel is not Duel duel)
			return;

		double enemyAction = duel.ChooseEnemyAction();
		GS.FullLog.Add(duel.DamagePhase(GS.PlayerObject, playerAction, enemyAction));
		GS.FullLog.Add($"You spent {playerAction}, enemy spent {enemyAction}.");

		if (!GS.PlayerObject.IsAlive())
		{
			GS.FullLog.Add("You lost the fight.");
			GS.PostDuelTimelinePath = GS.CurrentDuel.LoseTimelinePath;
			GS.CurrentDuel = null;
			return;
		}
		else if (!duel.Enemy.IsAlive())
		{
			GS.FullLog.Add("You won the fight.");
			GS.PostDuelTimelinePath = GS.CurrentDuel.WinTimelinePath;
			GS.CurrentDuel = null;
			return;
		}

		GS.PostDuelTimelinePath = null;
		GS.FullLog.Add(duel.TacticHint(duel.Enemy));
		duel.RestoreAfter(GS.PlayerObject, playerAction, enemyAction);
	}
}
