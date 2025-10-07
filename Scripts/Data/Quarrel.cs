using System;
using Godot;

public class Quarrel
{
    public string Id { get; set; }
    public string WinTimelinePath { get; set; }
    public string LoseTimelinePath { get; set; }
    public Combatant Enemy { get; set; }
    private static Random rng = new Random();
    public Combatant.Approach WinnersApproach;

    // 1. Attack Archetypes
    // Type	    Stat Focus	        Theme	                                Strengths	                                    Weaknesses	                                        Extra Effect
    // Persuade	Charisma	        Soothing rhetoric, calm logic	        Beats Insult (diffuses anger)	                Weak to Lie (manipulation outsmarts sincerity)	    If you win, restore +1 Power next turn (confidence boost)
    // Lie	    Subterfuge	        Deception, trickery, false narrative	Beats Persuade (truth-seekers get tangled)	    Weak to Perform (audience sees through you)	        On success, 25% chance enemy loses 1 Power (mental confusion)
    // Perform	Charisma + Power	Dramatic flair, crowd control	        Beats Lie (spotlight breaks deception)	        Weak to Insult (heckled off stage)	                On success, small “momentum” bonus: +0.5 Power restoration every turn
    // Insult	Subterfuge + Power	Verbal aggression, raw taunt	        Beats Perform (breaks composure)	            Weak to Persuade (cool-headed rebuttal)	            On success, 25% chance to reduce enemy Charisma by 1 for this quarrel

    // This forms a rotating cycle (4-way rock-paper-scissors):
    // Persuade > Insult > Perform > Lie > Persuade

    public Quarrel(string id, string winTimelinePath, string loseTimelinePath, Combatant enemy)
    {
        Id = id;
        WinTimelinePath = winTimelinePath;
        LoseTimelinePath = loseTimelinePath;
        Enemy = enemy;
    }

    public static double NextDoubleInRange(Random rng, double min, double max, int decimals = 1)
    {
        double raw = min + rng.NextDouble() * (max - min);
        return Math.Round(raw, decimals, MidpointRounding.AwayFromZero);
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

    public string DamagePhase(Combatant player, double playerSpent, double enemySpent)
    {
        double pSpent = player.SpendPower(playerSpent);
        double eSpent = Enemy.SpendPower(enemySpent);

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

    public double ChooseEnemyPower()
    {
        double randomNumber;
        if (Enemy.Power > Enemy.MaxPower * 0.7)
        {
            randomNumber = NextDoubleInRange(rng, 0, 101);
            return (randomNumber < 50)
                ? Enemy.Power
                : Math.Min(Enemy.Power, NextDoubleInRange(rng, 1, 4));
        }
        else if (Enemy.Power < Enemy.MaxPower * 0.3)
        {
            randomNumber = NextDoubleInRange(rng, 0, 101);
            if (randomNumber < 70)
                return 0;
            return (Enemy.Power > 0) ? Math.Min(Enemy.Power, NextDoubleInRange(rng, 1, 4)) : 0;
        }
        else if (Enemy.Health == 1)
        {
            return Enemy.Power;
        }
        else
        {
            randomNumber = NextDoubleInRange(rng, 0, 101);
            if (randomNumber < 50)
                return 0;
            else if (randomNumber < 85)
                return Math.Min(Enemy.Power, NextDoubleInRange(rng, 1, 4));
            else
                return Enemy.Power;
        }
    }

    public Combatant.Approach ChooseEnemyApproach(Combatant enemy)
    {
        // [_] do this
        double randomNumber = NextDoubleInRange(rng, 0, 101);
        if (enemy.Charisma > enemy.Subterfuge)
        {
            if (randomNumber < 75)
            {
                // 75% chance to use perform or persuade (persuade if low on power, otherwise perform)
                if (enemy.Power < 5)
                    return Combatant.Approach.Persuade;
                else
                    return Combatant.Approach.Perform;
            }
            else
            {
                // 25% chance to use insult or lie (random)
                double anotherRandomNumber = NextDoubleInRange(rng, 0, 101);
                if (anotherRandomNumber < 50)
                    return Combatant.Approach.Insult;
                else
                    return Combatant.Approach.Lie;
            }
        }
        else if (enemy.Charisma < enemy.Subterfuge)
        {
            if (randomNumber < 75)
            {
                // 75% chance to use insult or lie (lie if player just used little power, insult otherwise)
            }
            else
            {
                // 25% chance to use perform or persuade (random)
                double anotherRandomNumber = NextDoubleInRange(rng, 0, 101);
                if (anotherRandomNumber < 50)
                    return Combatant.Approach.Perform;
                else
                    return Combatant.Approach.Persuade;
            }
        }
        else
        {
            if (randomNumber < 50)
            {
                // 50% chance to use insult or lie (lie if player just used little power, insult otherwise)
            }
            else
            {
                // 50% chance to use perform or persuade (persuade if low on power, otherwise perform)
                if (enemy.Power < 5)
                    return Combatant.Approach.Persuade;
                else
                    return Combatant.Approach.Perform;
            }
        }
        return Combatant.Approach.Persuade;
    }

    public double CompareApproaches(Combatant player, Combatant enemy)
    {
        switch (player.CurrentApproach)
        {
            case Combatant.Approach.Persuade:
                if (enemy.CurrentApproach == Combatant.Approach.Persuade)
                    return 1;
                else if (enemy.CurrentApproach == Combatant.Approach.Lie)
                    return 0.8;
                else if (enemy.CurrentApproach == Combatant.Approach.Perform)
                    return 1;
                else if (enemy.CurrentApproach == Combatant.Approach.Insult)
                    return 1.2;
                else
                    return 1;
            case Combatant.Approach.Lie:
                if (enemy.CurrentApproach == Combatant.Approach.Persuade)
                    return 1.2;
                else if (enemy.CurrentApproach == Combatant.Approach.Lie)
                    return 1;
                else if (enemy.CurrentApproach == Combatant.Approach.Perform)
                    return 0.8;
                else if (enemy.CurrentApproach == Combatant.Approach.Insult)
                    return 1;
                else
                    return 1;
            case Combatant.Approach.Perform:
                if (enemy.CurrentApproach == Combatant.Approach.Persuade)
                    return 1;
                else if (enemy.CurrentApproach == Combatant.Approach.Lie)
                    return 1.2;
                else if (enemy.CurrentApproach == Combatant.Approach.Perform)
                    return 1;
                else if (enemy.CurrentApproach == Combatant.Approach.Insult)
                    return 0.8;
                else
                    return 1;
            case Combatant.Approach.Insult:
                if (enemy.CurrentApproach == Combatant.Approach.Persuade)
                    return 0.8;
                else if (enemy.CurrentApproach == Combatant.Approach.Lie)
                    return 1;
                else if (enemy.CurrentApproach == Combatant.Approach.Perform)
                    return 1.2;
                else if (enemy.CurrentApproach == Combatant.Approach.Insult)
                    return 1;
                else
                    return 1;
            default:
                return 1;
        }
    }

    public int CalculateDamage(double loserSpent)
    {
        // [_] add winners approach modifier here
        // [_] add round winner to parameters to get their charisma or subterfuge
        if (loserSpent == 0)
            return 1;
        else if (loserSpent <= 5)
            return 2;
        else
            return 4;
    }

    public void RestoreAfter(Combatant player, double playerAction, double enemyAction)
    {
        if (playerAction < 3)
        {
            double amount = Math.Max(1, 4 - playerAction);
            double missingPower = player.MaxPower - player.Power;
            if (missingPower > 0)
                player.RestorePower(NextDoubleInRange(rng, 1, Math.Min(amount, missingPower) + 1));
        }

        if (enemyAction < 3)
        {
            double amount = Math.Max(1, 4 - enemyAction);
            double missingPower = Enemy.MaxPower - Enemy.Power;
            if (missingPower > 0)
                Enemy.RestorePower(NextDoubleInRange(rng, 1, Math.Min(amount, missingPower) + 1));
        }
    }
}
