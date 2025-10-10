using System;
using Godot;

public class Combatant
{
	public string Name { get; set; }
	public double Power { get; set; }
	public int MaxPower { get; set; }
	public int Health { get; set; }
	public int MaxHealth { get; set; }
	public int Subterfuge { get; set; }
	public int Charisma { get; set; }
	public int? Brawn { get; set; }
	public int? Lore { get; set; }
	public int? Honest_Manipulative { get; set; }
	public int? Accommodating_Domineering { get; set; }
	public int? Humanist_Deist { get; set; }
	public string QuarrelSpritePath { get; set; }

	public enum Approach
	{
		Persuade,
		Lie,
		Perform,
		Insult
	}

	public Approach CurrentApproach;

	// About "{ get; set; }":
	// Behind the scenes, it looks like this:
	// private string _quarrelSpritePath;
	// public string QuarrelSpritePath
	// {
	// get { return _quarrelSpritePath; }
	// set { _quarrelSpritePath = value; }
	// }
	// So the benefit of this over just a field is that we can edit the getter and setter later.

	public Combatant(
		string name,
		double power,
		int maxPower,
		int health,
		int maxHealth,
		int subterfuge,
		int charisma,
		string quarrelSpritePath
	)
	{
		Name = name;
		Power = power;
		MaxPower = maxPower;
		Health = health;
		MaxHealth = maxHealth;
		Subterfuge = subterfuge;
		Charisma = charisma;
		QuarrelSpritePath = quarrelSpritePath;
	}

	public static Combatant FromString(string s)
	{
		var parts = s.Split(',');
		return new Combatant(
			parts[0].Trim(),
			double.Parse(parts[1].Trim()), // power
			int.Parse(parts[2].Trim()), // maxPower
			int.Parse(parts[3].Trim()), // health
			int.Parse(parts[4].Trim()), // maxHealth
			int.Parse(parts[5].Trim()), // subterfuge
			int.Parse(parts[6].Trim()), // charisma
			parts[7].Trim() // quarrelSpritePath
		);
	}

	public double SpendPower(double amount)
	{
		double spend = Math.Max(0, Math.Min(amount, Power));
		Power -= spend;
		return spend;
	}

	public void RestorePower(double amount)
	{
		Power = Math.Min(MaxPower, Power + amount);
	}

	public int TakeDamage(int damage)
	{
		Health = Math.Max(0, Health - damage);
		return Health;
	}

	public bool IsAlive()
	{
		return Health > 0;
	}
}
