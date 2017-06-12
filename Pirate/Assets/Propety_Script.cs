using UnityEngine;
using System.Collections.Generic;

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
	Bow,
	Gun,
	Food,
	Null
}

// database items
public class DatabaseInventoryItem
{
	public DatabaseInventoryItem(Chasis chasis, string name, int quantity, int damage)
	{
		Name = name;
		MaxQuantity = quantity;
		Damage = damage;
		Chasis = chasis;
	}

	public DatabaseInventoryItem(Chasis chasis, string name, int damage)
	{
		Name = name;
		MaxQuantity = 1;
		Damage = damage;
		Chasis = chasis;
	}

	public readonly string Name;
	public readonly int MaxQuantity;
	public readonly int Damage;
	public readonly Chasis Chasis;

	public bool Stackable
	{
		get { return MaxQuantity > 1; }
	}
}


// the database
public static class GameDatabase
{
	public const int MaxInventorySize = 4;

	public static readonly Dictionary<string, DatabaseInventoryItem> InventoryItems = new Dictionary<string, DatabaseInventoryItem>();

	static GameDatabase()
	{
		AddItem(new DatabaseInventoryItem(Chasis.Ammo, "Broken Arrow", quantity: 10, damage: 4));
		AddItem(new DatabaseInventoryItem(Chasis.Ammo, "Arrow", quantity: 12, damage: 7));
		AddItem(new DatabaseInventoryItem(Chasis.Ammo, "Enchanted Arrow", damage: 14));
	}

	private static void AddItem(DatabaseInventoryItem item)
	{
		InventoryItems[item.Name] = item;
	}
}
