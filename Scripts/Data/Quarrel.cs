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
    public double PlayerPowerRestorationBonus = 0;
    public double EnemyPowerRestorationBonus = 0;
    public int PlayerCharismaPenalty = 0;
    public int EnemyCharismaPenalty = 0;

    // 1. Attack Archetypes
    // Type	    Stat Focus	        Theme	                                Strengths	                                    Weaknesses	                                        Extra Effect
    // Persuade	Charisma	        Soothing rhetoric, calm logic	        Beats Insult (diffuses anger)	                Weak to Lie (manipulation outsmarts sincerity)	    On a win, restore +1 Power next turn (confidence boost)
    // Lie	    Subterfuge	        Deception, trickery, false narrative	Beats Persuade (truth-seekers get tangled)	    Weak to Perform (audience sees through you)	        On a win, 25% chance enemy loses 1 Power (mental confusion)
    // Perform	Charisma + Power	Dramatic flair, crowd control	        Beats Lie (spotlight breaks deception)	        Weak to Insult (heckled off stage)	                On a win, small “momentum” bonus: +0.5 Power restoration every turn
    // Insult	Subterfuge + Power	Verbal aggression, raw taunt	        Beats Perform (breaks composure)	            Weak to Persuade (cool-headed rebuttal)	            On a win, 25% chance to reduce enemy Charisma by 1 for this quarrel

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
            return "Both hold their tongues, the silence louder than any argument.";
        }
        else if (pSpent == 0)
        {
            WinnersApproach = Enemy.CurrentApproach;
            int damage = CalculateDamage(0, Enemy);
            player.TakeDamage(damage);
            return $"{Enemy.Name} strikes with {Enemy.CurrentApproach}, while {player.Name} stays silent. The retort lands clean — {Enemy.CurrentApproach} overwhelms passivity.";
        }
        else if (eSpent == 0)
        {
            WinnersApproach = player.CurrentApproach;
            int damage = CalculateDamage(0, player);
            Enemy.TakeDamage(damage);
            return $"{player.Name} launches a {player.CurrentApproach} while {Enemy.Name} falters. Words cut sharper than hesitation.";
        }
        else
        {
            double playerPowerModifier = CompareApproaches(player, Enemy);
            string advantageText = AdvantageText(player.CurrentApproach, Enemy.CurrentApproach);

            if (pSpent * playerPowerModifier > eSpent)
            {
                WinnersApproach = player.CurrentApproach;
                int damage = CalculateDamage(eSpent, player);
                Enemy.TakeDamage(damage);
                return $"{player.Name} uses {player.CurrentApproach}, {advantageText} {Enemy.Name}'s {Enemy.CurrentApproach}. The crowd turns — {damage} resolve lost!";
            }
            else if (eSpent > pSpent * playerPowerModifier)
            {
                WinnersApproach = Enemy.CurrentApproach;
                int damage = CalculateDamage(pSpent, Enemy);
                player.TakeDamage(damage);
                return $"{Enemy.Name} answers with {Enemy.CurrentApproach}, {advantageText} {player.Name}'s {player.CurrentApproach}. Confidence cracks — {damage} resolve lost!";
            }
            else
            {
                player.TakeDamage(1);
                Enemy.TakeDamage(1);
                return $"{player.Name}'s {player.CurrentApproach} and {Enemy.Name}'s {Enemy.CurrentApproach} clash in perfect symmetry. Both arguments sting, neither concedes.";
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

    public Combatant.Approach ChooseEnemyApproach(double playerAction)
    {
        double randomNumber = NextDoubleInRange(rng, 0, 101);
        if (Enemy.Charisma > Enemy.Subterfuge)
        {
            if (randomNumber < 75)
            {
                // 75% chance to use perform or persuade (persuade if low on power, otherwise perform)
                if (Enemy.Power < 5)
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
        else if (Enemy.Charisma < Enemy.Subterfuge)
        {
            if (randomNumber < 75)
            {
                // 75% chance to use insult or lie (lie if player just used little power, insult otherwise)
                if (playerAction < 2)
                    return Combatant.Approach.Lie;
                else
                    return Combatant.Approach.Insult;
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
                if (playerAction < 2)
                    return Combatant.Approach.Lie;
                else
                    return Combatant.Approach.Insult;
            }
            else
            {
                // 50% chance to use perform or persuade (persuade if low on power, otherwise perform)
                if (Enemy.Power < 5)
                    return Combatant.Approach.Persuade;
                else
                    return Combatant.Approach.Perform;
            }
        }
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

    public int CalculateDamage(double loserSpent, Combatant roundWinner)
    {
        double statToUse;
        if (
            WinnersApproach == Combatant.Approach.Persuade
            || WinnersApproach == Combatant.Approach.Perform
        )
        {
            statToUse = roundWinner.Charisma;
        }
        else
        {
            statToUse = roundWinner.Subterfuge;
        }
        double baseDamage;
        if (loserSpent == 0)
            baseDamage = 1;
        else if (loserSpent <= 5)
            baseDamage = 2;
        else
            baseDamage = 4;

        // scaling: +50% per every even stat point (no baseline)
        int evenCount = (int)Math.Floor(statToUse / 2.0); // how many even thresholds reached
        double multiplier = 1 + (0.5 * evenCount); // +50% each
        double scaledDamage = baseDamage * multiplier;

        return (int)Math.Ceiling(scaledDamage);
    }

    public string ApplyEffects(Combatant roundWinner, Combatant roundLoser)
    {
        // needs to return a log quote as to what happened for QuarrelRound to add to th elogs
        switch (WinnersApproach)
        {
            case Combatant.Approach.Persuade:
                // On a win, restore +1 Power next turn (confidence boost)
                double missingPower = roundWinner.MaxPower - roundWinner.Power;
                if (missingPower > 0)
                    roundWinner.RestorePower(1);
                return $"{roundWinner.Name} has a confidence boost from the persuasion, restoring 1 power!";
            case Combatant.Approach.Lie:
                // On a win, 25% chance enemy loses 1 Power (mental confusion)
                double randomNumber = NextDoubleInRange(rng, 0, 101);
                if (randomNumber < 25)
                {
                    if (roundLoser.Power > 1)
                        roundLoser.Power = roundLoser.Power - 1;
                    else
                        roundLoser.Power = 0;
                    return $"{roundWinner.Name}'s words sting deep, causing {roundLoser.Name}'s mind to falter, losing 1 power!";
                }
                else
                {
                    return $"{roundLoser.Name} shrugs off {roundWinner.Name}'s words.";
                }
            case Combatant.Approach.Perform:
                // On a win, small “momentum” bonus: +0.5 Power restoration every turn
                if (roundWinner == Enemy)
                    EnemyPowerRestorationBonus = EnemyPowerRestorationBonus + 0.5;
                else
                    PlayerPowerRestorationBonus = PlayerPowerRestorationBonus + 0.5;
                return $"{roundWinner.Name}'s performance boosts their momentum, increasing their total power restoration bonus to {(roundWinner == Enemy ? EnemyPowerRestorationBonus : PlayerPowerRestorationBonus)}";
            case Combatant.Approach.Insult:
                // On a win, 25% chance to reduce enemy Charisma by 1 for this quarrel
                double randomNumber2 = NextDoubleInRange(rng, 0, 101);
                if (randomNumber2 < 25)
                {
                    if (roundLoser == Enemy)
                    {
                        if (Enemy.Charisma > 0)
                            Enemy.Charisma = Enemy.Charisma - 1;
                        EnemyCharismaPenalty = EnemyCharismaPenalty + 1;
                    }
                    else
                    {
                        if (roundLoser.Charisma > 0)
                            roundLoser.Charisma = roundLoser.Charisma - 1;
                        PlayerCharismaPenalty = PlayerCharismaPenalty + 1;
                    }
                    return $"The insult sticks! {roundLoser.Name}'s confidence falters, decreasing their charisma by 1 for this quarrel!";
                }
                else
                {
                    return $"{roundLoser.Name} shrugs off the insult!";
                }
        }
        return "";
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
        if (PlayerPowerRestorationBonus > 0)
            player.RestorePower(PlayerPowerRestorationBonus);
        if (EnemyPowerRestorationBonus > 0)
            Enemy.RestorePower(EnemyPowerRestorationBonus);
    }

    private string AdvantageText(
        Combatant.Approach playerApproach,
        Combatant.Approach enemyApproach
    )
    {
        if (
            playerApproach == Combatant.Approach.Persuade
            && enemyApproach == Combatant.Approach.Insult
        )
            return "calm reason defuses";
        if (
            playerApproach == Combatant.Approach.Lie
            && enemyApproach == Combatant.Approach.Persuade
        )
            return "cunning twists";
        if (playerApproach == Combatant.Approach.Perform && enemyApproach == Combatant.Approach.Lie)
            return "showmanship exposes";
        if (
            playerApproach == Combatant.Approach.Insult
            && enemyApproach == Combatant.Approach.Perform
        )
            return "mockery shatters";
        if (
            enemyApproach == Combatant.Approach.Persuade
            && playerApproach == Combatant.Approach.Insult
        )
            return "calm reason defuses";
        if (
            enemyApproach == Combatant.Approach.Lie
            && playerApproach == Combatant.Approach.Persuade
        )
            return "cunning twists";
        if (enemyApproach == Combatant.Approach.Perform && playerApproach == Combatant.Approach.Lie)
            return "showmanship exposes";
        if (
            enemyApproach == Combatant.Approach.Insult
            && playerApproach == Combatant.Approach.Perform
        )
            return "mockery shatters";
        return "outmaneuvers";
    }
}
