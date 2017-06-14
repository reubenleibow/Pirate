using UnityEngine;
using System.Collections.Generic;

//characters current weapon in hand
public enum WeaponChasis
{
	SingleHandMelee,
	Ranged,
	DoubleHandedWeapon,
	Nothing
}

public enum Chasis
{
	Ammo,
	SingleHandWeapon,
	DoubleHandWeapon,
	Ranged,
	Bow,
	Gun,
	Food,
	Null
}

// database items
public class DatabaseInventoryItem
{
	public DatabaseInventoryItem(Chasis chasis,WeaponChasis wChasis, string name, int quantity, int damage)
	{
		Name = name;
		MaxQuantity = quantity;
		Damage = damage;
		Chasis = chasis;
		WChasis = wChasis;

	}

	public DatabaseInventoryItem(Chasis chasis, WeaponChasis wChasis, string name, int damage)
	{
		Name = name;
		MaxQuantity = 1;
		Damage = damage;
		Chasis = chasis;
		WChasis = wChasis;

	}

	public readonly string Name;
	public readonly int MaxQuantity;
	public readonly int Damage;
	public readonly Chasis Chasis;
	public readonly WeaponChasis WChasis;

	public bool Stackable
	{
		get { return MaxQuantity > 1; }
	}
}

public class characterProperties 
{
	public static characterProperties Properties;
	public int MinBowRange = 4;
	public int MinStartRanged = 8;
}


// the database
public static class GameDatabase
{
	public const int MaxInventorySize = 4;

	public static readonly Dictionary<string, DatabaseInventoryItem> InventoryItems = new Dictionary<string, DatabaseInventoryItem>();

	static GameDatabase()
	{
		AddItem(new DatabaseInventoryItem(Chasis.Ammo,WeaponChasis.Nothing, "Broken Arrow", quantity: 10, damage: 4));
		AddItem(new DatabaseInventoryItem(Chasis.Ammo, WeaponChasis.Nothing, "Arrow", quantity: 12, damage: 7));
		AddItem(new DatabaseInventoryItem(Chasis.Ammo, WeaponChasis.Nothing, "Enchanted Arrow", damage: 14));
		AddItem(new DatabaseInventoryItem(Chasis.Null, WeaponChasis.Ranged, "Bow", damage: 14));
		AddItem(new DatabaseInventoryItem(Chasis.Null, WeaponChasis.SingleHandMelee, "Sword", damage: 14));
	}

	private static void AddItem(DatabaseInventoryItem item)
	{
		InventoryItems[item.Name] = item;
	}
}
