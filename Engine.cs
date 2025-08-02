using System;
using Godot;

public partial class Engine : Node
{
	public GameState GS;

	public void InitGame(GameState gs)
	{
		GS = gs;
	}

	public void ChooseOption(Element next)
	{
		GS.CurrentElement = next;
	}

	public void DuelRound(int playerAction)
	{
		if (GS.CurrentElement is not Duel duel)
			return;

		int enemyAction = duel.ChooseEnemyAction();
		GS.FullLog.Add(duel.DamagePhase(GS.PlayerObject, playerAction, enemyAction));
		GS.FullLog.Add($"You spent {playerAction}, enemy spent {enemyAction}.");

		if (!GS.PlayerObject.IsAlive())
		{
			GS.CurrentElement = duel.LoseStory;
			GS.FullLog.Add("You lost the fight.");
			return;
		}
		else if (!duel.Enemy.IsAlive())
		{
			GS.CurrentElement = duel.WinStory;
			GS.FullLog.Add("You won the fight.");
			return;
		}

		GS.FullLog.Add(duel.TacticHint(duel.Enemy));
		duel.RestoreAfter(GS.PlayerObject, playerAction, enemyAction);
	}
}
