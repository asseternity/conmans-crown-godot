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

	public void ChooseOption(Element next)
	{
		GS.CurrentElement = next;
	}

	public void DuelRound(double playerAction)
	{
		if (GS.CurrentElement is not Duel duel)
			return;

		double enemyAction = duel.ChooseEnemyAction();
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

	public Element FindElementByID(string id_string)
	{
		foreach (var element in GS.AllElements)
		{
			if (element.Value is StoryLine s)
			{
				if (s.Id == id_string)
				{
					return s;
				}
			}
			else if (element.Value is Duel d)
			{
				if (d.Id == id_string)
				{
					return d;
				}
			}
		}
		return new StoryLine("ID: Missing", "Text: Missing", new List<DialogueOption>());
	}
}
