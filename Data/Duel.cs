using System;
using Godot;

public class Duel : Element
{
    public string Id { get; set; }
    public StoryLine WinStory { get; set; }
    public StoryLine LoseStory { get; set; }
    public Combatant Enemy { get; set; }
    private static Random rng = new Random();

    public Duel(string id, StoryLine winStory, StoryLine loseStory, Combatant enemy)
    {
        Id = id;
        WinStory = winStory;
        LoseStory = loseStory;
        Enemy = enemy;
    }

    public string TacticHint(Combatant combatant)
    {
        if (combatant.Power > combatant.MaxPower * 0.7)
            return $"{combatant.Name}'s posture shifts — shoulders tense, eyes fierce. A mighty blow seems imminent!";
        else if (combatant.Power < combatant.MaxPower * 0.3)
            return $"{combatant.Name} appears winded, steps lighter and blade hesitant. A defensive move or feeble jab is likely.";
        else
            return $"{combatant.Name}'s stance wavers between caution and ambition — a measured strike may be coming.";
    }

    public int ChooseEnemyAction()
    {
        int randomNumber;
        if (Enemy.Power > Enemy.MaxPower * 0.7)
        {
            randomNumber = rng.Next(0, 101);
            return (randomNumber < 50) ? Enemy.Power : Math.Min(Enemy.Power, rng.Next(1, 4));
        }
        else if (Enemy.Power < Enemy.MaxPower * 0.3)
        {
            randomNumber = rng.Next(0, 101);
            if (randomNumber < 70)
                return 0;
            return (Enemy.Power > 0) ? Math.Min(Enemy.Power, rng.Next(1, Enemy.Power + 1)) : 0;
        }
        else if (Enemy.Health == 1)
        {
            return Enemy.Power;
        }
        else
        {
            randomNumber = rng.Next(0, 101);
            if (randomNumber < 50)
                return 0;
            else if (randomNumber < 85)
                return Math.Min(Enemy.Power, rng.Next(1, 4));
            else
                return Enemy.Power;
        }
    }

    public string DamagePhase(Combatant player, int playerSpent, int enemySpent)
    {
        int pSpent = player.SpendPower(playerSpent);
        int eSpent = Enemy.SpendPower(enemySpent);

        if (pSpent == 0 && eSpent == 0)
        {
            return "Both warriors circle one another cautiously, eyes locked — but neither dares strike. The tension grows; no blood is spilled this turn.";
        }
        else if (pSpent == 0)
        {
            player.TakeDamage(1);
            return $"{player.Name} braces for the blow, steel raised in defense — yet {Enemy.Name}'s strike slips through, leaving a shallow wound!";
        }
        else if (eSpent == 0)
        {
            Enemy.TakeDamage(1);
            return $"{Enemy.Name} retreats behind a guarded stance, but {player.Name}'s cunning feint lands true — a minor cut, but a message sent.";
        }
        else
        {
            if (pSpent > eSpent)
            {
                int damage = CalculateDamage(eSpent);
                Enemy.TakeDamage(damage);
                return $"{player.Name} unleashes a furious assault, overpowering {Enemy.Name}'s efforts. The blow lands hard — {damage} damage dealt!";
            }
            else if (eSpent > pSpent)
            {
                int damage = CalculateDamage(pSpent);
                player.TakeDamage(damage);
                return $"{Enemy.Name} finds an opening and strikes like lightning! {player.Name} reels back, taking {damage} damage.";
            }
            else
            {
                player.TakeDamage(1);
                Enemy.TakeDamage(1);
                return "Steel clashes against steel in perfect synchronicity — both combatants land glancing hits. Blood is drawn on both sides.";
            }
        }
    }

    public int CalculateDamage(int loserSpent)
    {
        if (loserSpent == 0)
            return 1;
        else if (loserSpent <= 5)
            return 2;
        else
            return 4;
    }

    public void RestoreAfter(Combatant player, int playerAction, int enemyAction)
    {
        if (playerAction < 3)
        {
            int amount = Math.Max(1, 4 - playerAction);
            int missingPower = player.MaxPower - player.Power;
            if (missingPower > 0)
                player.RestorePower(rng.Next(1, Math.Min(amount, missingPower) + 1));
        }

        if (enemyAction < 3)
        {
            int amount = Math.Max(1, 4 - enemyAction);
            int missingPower = Enemy.MaxPower - Enemy.Power;
            if (missingPower > 0)
                Enemy.RestorePower(rng.Next(1, Math.Min(amount, missingPower) + 1));
        }
    }
}
