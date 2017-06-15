using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Base_Character_Script : MonoBehaviour
{
	private GameObject coreCharacter;

	float DifficaultyModifier = 1;

	int Base_Health = 100;
	DatabaseInventoryItem Base_Ranged;
	DatabaseInventoryItem Base_Melee;
	bool Base_HorseBack = false;
	float Base_Speed = 10;
	int Base_MaxRange = 4;
	string Base_Faction;
	public int Base_Kills = 0;
	List<Items_Script> Inventory = new List<Items_Script>(GameDatabase.MaxInventorySize);

	// load the properties
	characterProperties Base_Propeties = new characterProperties();

	public int TeamSide;
	public bool targeted = false;
	public Base_Character_Script Enemy;
	public string CharacterState;
	public float DistanceEnemy;
	public Base_Character_Script LastTouch = null;
	public bool Dead = false;
	public WeaponChasis CurrentWeaponChasis = WeaponChasis.Nothing;

	public List<Base_Character_Script> PendingSideList;
	public List<Base_Character_Script> TeamSideList;
	public List<Base_Character_Script> Enemies;
	bool ForceStop = false;

	//InventoryStuff
	int Ammo = 0;


	public GameObject Core_Character
	{
		get { return coreCharacter ?? this.gameObject; }
		set { coreCharacter = value; }
	}

	private NavMeshAgent NavMesh
	{
		get { return Core_Character.GetComponent<NavMeshAgent>(); }
	}

	// Use this for initialization
	void Start()
	{
		//Test***********************
		AddItem("Bow", 1);
		AddItem("Sword", 1);
		MainBattle_Script.System.AllCharacters.Add(this);
		if (TeamSide == 1)
		{
			// Make shortcut to list,thus also allowing for teams 2>
			PendingSideList = MainBattle_Script.System.PendingTeam1;
			TeamSideList = MainBattle_Script.System.Team1;
			Enemies = MainBattle_Script.System.Team2;
		}
		else
		{
			// Make shortcut to list,thus also allowing for teams 2>
			PendingSideList = MainBattle_Script.System.PendingTeam2;
			TeamSideList = MainBattle_Script.System.Team2;
			Enemies = MainBattle_Script.System.Team1;
		}

		TeamSideList.Add(this);
		PendingSideList.Add(this);

		foreach(var cItem in Inventory)
		{
			var explainedItem = GameDatabase.InventoryItems[cItem.Name];

			if(explainedItem.WChasis == WeaponChasis.Ranged)
			{
				Base_Ranged = explainedItem;
			}

			if (explainedItem.WChasis == WeaponChasis.SingleHandMelee || explainedItem.WChasis == WeaponChasis.DoubleHandedWeapon)
			{
				Base_Melee = explainedItem;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Dead)
		{
			return;
		}

		bool InRange = false;

		//work out dist from enemy

		if (Enemy != null)
		{
			DistanceEnemy = Vector3.Distance(this.Core_Character.transform.position, Enemy.Core_Character.transform.position);
			//test***************************
			if(ForceStop == false && CurrentWeaponChasis != WeaponChasis.Ranged)
			{
				NavMesh.SetDestination(Enemy.transform.position);
			}
		}
		else
		{
			PendingSideList.Add(this);
		}

		bool x = false;

		//is enemy in range?
		InRange = DistanceEnemy <= Base_MaxRange;
		//enemy in range
		if (InRange)
		{
			//Attack();

			//when character must remain still
			x = x || (CurrentWeaponChasis == WeaponChasis.Ranged || Dead);
		}
		if(DistanceEnemy < Base_Propeties.MinBowRange && CurrentWeaponChasis == WeaponChasis.Ranged)
		{
			if(Base_Melee != null)
			{
				CurrentWeaponChasis = Base_Melee.WChasis;
			}
		}
		//aDD aMMO CHECK HERE********************************
		if(Base_Ranged != null && DistanceEnemy > Base_Propeties.MinStartRanged)
		{
			CurrentWeaponChasis = WeaponChasis.Ranged;
			CheackForAmmo();
		}

		ForceStop = x;

		//stop all movement if force stop is true**************************
		if (ForceStop || CurrentWeaponChasis == WeaponChasis.Ranged)
		{
			NavMesh.Stop();
			NavMesh.ResetPath();
		}
	}


	void Attack()
	{
		//var leftOver = AddItem("Broken Arrow", 20);
		//ReturnToLeftSide("Broken Arrow", leftOver);
	}

	void KillCharacter()
	{
		Dead = true;
		MainBattle_Script.System.AllDeath.Add(this);
		NavMesh.Stop();
		NavMesh.ResetPath();
	}

	int AddItem(string name, int quantity)
	{
		int leftOverQuantity = quantity;
		var maxQty = GameDatabase.InventoryItems[name].MaxQuantity;

		var freeSpaces = Inventory
			.Where(i => i.Name == name)
			.Where(i => i.Quantity < maxQty)
			.ToArray();

		// if 0 therefore used all else X left over(only if stackable)
		foreach (Items_Script currentItem in freeSpaces)
		{
			var freeSpace = maxQty - (currentItem.Quantity + leftOverQuantity);

			leftOverQuantity = freeSpace > 0 ? 0 : (freeSpace * -1);

			if (leftOverQuantity == 0)
			{
				currentItem.Quantity += quantity;
			}
			else
			{
				currentItem.Quantity = maxQty;
			}

		}

		// fill up to capacity but still left over(not stackable/got left overs)
		for (int i = GameDatabase.MaxInventorySize - Inventory.Count; i > 0 && leftOverQuantity > 0; i--)
		{
			//AddItem(currentItem.Chasis, currentItem.Name, currentItem.)
			Inventory.Add(new Items_Script
			{
				Name = name,
				Quantity = (leftOverQuantity > maxQty) ? maxQty : leftOverQuantity
			});
			leftOverQuantity -= maxQty;
		}

		return leftOverQuantity > 0 ? leftOverQuantity : 0;
	}

	void CheackForAmmo()
	{
		var ammo = 0;

		//Check ammo for current weapon
		foreach (var cItem in Inventory)
		{
			var DataItem = GameDatabase.InventoryItems[cItem.Name];

			if (DataItem.Chasis == Base_Ranged.Chasis)
			{
				ammo += cItem.Quantity;
			}
		}

		if(ammo == 0)
		{
			//no ammo for current weapon;
			FindRangedWeapon();
		}

	}

	void FindRangedWeapon()
	{
		var ammo = 0;

		foreach (var cItem in Inventory)
		{
			if (ammo == 0)
			{
				// Item of weapon
				var DataItem = GameDatabase.InventoryItems[cItem.Name];

				//picked ranged weapon
				if (DataItem.Name != Base_Ranged.Name && DataItem.WChasis == WeaponChasis.Ranged)
				{
					//search for ammo for that item(DataItem = the picked item)
					foreach (var ammoItem in Inventory)
					{
						var DataAmmo = GameDatabase.InventoryItems[ammoItem.Name];
						//If of the same class(both arrow group) and if is ammo...
						if (DataAmmo.Chasis == DataItem.Chasis && DataAmmo.WChasis == WeaponChasis.Ammo)
						{
							ammo += ammoItem.Quantity;
						}
					}
					// if that item has ammo then make that the base weapon
					if (ammo > 0)
					{
						Base_Ranged = DataItem;
					}
				}
			}
		}
	}
}

