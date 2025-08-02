using System;
using Godot;

public class Combatant
{
	public string Name { get; set; }
	public int MaxPower { get; set; }
	public int Power { get; set; }
	public int MaxHealth { get; set; }
	public int Health { get; set; }
	public int Subterfuge { get; set; }
	public int Charisma { get; set; }

	public Combatant(
		string name,
		int power,
		int maxPower,
		int health,
		int maxHealth,
		int subterfuge,
		int charisma
	)
	{
		Name = name;
		Power = power;
		MaxPower = maxPower;
		Health = health;
		MaxHealth = maxHealth;
		Subterfuge = subterfuge;
		Charisma = charisma;
	}

	public static Combatant FromString(string s)
	{
		var parts = s.Split(',');
		return new Combatant(
			parts[0].Trim(),
			int.Parse(parts[1].Trim()),
			int.Parse(parts[2].Trim()),
			int.Parse(parts[3].Trim()),
			int.Parse(parts[4].Trim()),
			int.Parse(parts[5].Trim()),
			int.Parse(parts[6].Trim())
		);
	}

	public int SpendPower(int amount)
	{
		int spend = Math.Max(0, Math.Min(amount, Power));
		Power -= spend;
		return spend;
	}

	public void RestorePower(int amount)
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
