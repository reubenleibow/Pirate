using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Base_Character_Script : MonoBehaviour
{
	private GameObject coreCharacter;

	float DifficaultyModifier = 1;

	int Base_Health = 100;
	DatabaseInventoryItem Base_Ranged = null;
	DatabaseInventoryItem Base_Melee;
	bool Base_HorseBack = false;
	float Base_Speed = 10;
	int Base_MaxRange = 20;
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

	public int fastAmmo = 0;

	public Items_Script CurrentMissile = new Items_Script();

	//InventoryStuff

	public float Timer = 0.5f;
	public int TestRange;

	bool Scan = true;

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

		// set up battlefield
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

			if (explainedItem.WChasis == WeaponChasis.SingleHandMelee || explainedItem.WChasis == WeaponChasis.DoubleHandedWeapon)
			{
				Base_Melee = explainedItem;
				CurrentWeaponChasis = Base_Melee.WChasis;
				Base_MaxRange = Base_Melee.Range;
			}

			if (explainedItem.WChasis == WeaponChasis.Ranged)
			{
				Base_Ranged = explainedItem;
				Base_MaxRange = Base_Ranged.Range;
				//FindRangedWeapon();
			}


		}
	}

	// Update is called once per frame
	void Update()
	{
		Timer -= Time.deltaTime;
		TestRange = Base_MaxRange;

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
			fastAmmo = CurrentMissile.Quantity;
			// if (Eg: bow) is being used, 
			if (CurrentWeaponChasis == WeaponChasis.Ranged && fastAmmo > 0)
			{
				//shoot every second
				if (Timer <= 0)
				{
					CurrentMissile.Quantity --;
					Scan = true;
				//	Attack();
				}
			}

			if(fastAmmo == 0 && Base_Ranged != null && Scan == true)
			{
				CheackForAmmo();
				Scan = false;
			}

			//when character must remain still
			x = x || (CurrentWeaponChasis == WeaponChasis.Ranged || Dead);
		}

		// switch to melee
		if (DistanceEnemy < Base_Propeties.MinBowRange && CurrentWeaponChasis == WeaponChasis.Ranged && Base_Melee != null)
		{
			CurrentWeaponChasis = Base_Melee.WChasis;
			Base_MaxRange = Base_Melee.Range;
		}

		// switch to ranged
		if (Base_Ranged != null && DistanceEnemy > Base_Propeties.MinStartRanged)
		{
			CurrentWeaponChasis = Base_Ranged.WChasis;
			Base_MaxRange = Base_Ranged.Range;
			//CheackForAmmo();

		}

		ForceStop = x;

		//stop all movement if force stop is true**************************
		if (ForceStop || CurrentWeaponChasis == WeaponChasis.Ranged)
		{
			NavMesh.Stop();
			NavMesh.ResetPath();
		}
		//*************************************
		if(Timer <= 0)
		{
			Timer = 0.5f;
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

	public int AddItem(string name, int quantity)
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
		foreach (var cItem in Inventory.Where(q => q.Quantity > 0).ToArray())
		{

			var DataItem = GameDatabase.InventoryItems[cItem.Name];

			// if same class and is ammo
			if (DataItem.Chasis == Base_Ranged.Chasis && DataItem.WChasis == WeaponChasis.Ammo)
			{
				// if the chosen ammo matches the current missile, the set ammo to the right amount;
				if(cItem.Name == CurrentMissile.Name && fastAmmo > 0)
				{
					cItem.Quantity = fastAmmo;
					fastAmmo = 0;
				}

				ammo += cItem.Quantity;
				// if there is ammo in a pack then set current missile and fast ammo;
				if(fastAmmo == 0 && ammo > 0)
				{
					CurrentMissile = cItem;
					fastAmmo = cItem.Quantity;
				}
			}
		}

		if(ammo == 0)
		{
			//no ammo for current weapon;
			//FindRangedWeapon();
		}

	}

	void FindRangedWeapon()
	{
		var ammo = 0;

		foreach (var cItem in Inventory.ToArray())
		{
			if (ammo == 0)
			{
				// Item of weapon
				var DataItem = GameDatabase.InventoryItems[cItem.Name];

				//picked ranged weapon
				if (DataItem.WChasis == WeaponChasis.Ranged)
				{
					//search for ammo for that item(DataItem = the picked item)
					foreach (var ammoItem in Inventory.ToArray())
					{
						var DataAmmo = GameDatabase.InventoryItems[ammoItem.Name];

						//If of the same class(both arrow group) and if is ammo...
						if (DataAmmo.Chasis == DataItem.Chasis && DataAmmo.WChasis == WeaponChasis.Ammo)
						{
							if(CurrentMissile != null)
							{
								if (ammoItem.Name == CurrentMissile.Name && fastAmmo > 0)
								{
									ammoItem.Quantity = fastAmmo;
									fastAmmo = 0;
								}
							}

							ammo += cItem.Quantity;

							// if there is ammo in a pack then set current missile and fast ammo;
							if (fastAmmo == 0 && ammo > 0)
							{
								CurrentMissile = ammoItem;
								fastAmmo = ammoItem.Quantity;
							}
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

		if (ammo == 0 && Base_Melee != null)
		{
			Base_Ranged = null;
			CurrentWeaponChasis = Base_Melee.WChasis;
		}
	}
}

