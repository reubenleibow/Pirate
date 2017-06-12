using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Base_Character_Script : MonoBehaviour
{
	private GameObject coreCharacter;

	float DifficaultyModifier = 1;

	int Base_Health = 100;
	bool Base_Ranged = true;
	bool Base_Melee = true;
	bool Base_HorseBack = false;
	float Base_Speed = 10;
	int Base_MaxRange = 4;
	string Base_Faction;
	public int Base_Kills = 0;
	List<Items_Script> Inventory = new List<Items_Script>(GameDatabase.MaxInventorySize);

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

		if (Enemy != null && ForceStop == false)
		{
			DistanceEnemy = Vector3.Distance(this.Core_Character.transform.position, Enemy.Core_Character.transform.position);
			NavMesh.SetDestination(Enemy.transform.position);
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
			Attack();

			//when character must remain still
			x = x || (CurrentWeaponChasis == WeaponChasis.Ranged || Dead);
		}


		ForceStop = x;

		//stop all movement if force stop is true
		if (ForceStop)
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
}
