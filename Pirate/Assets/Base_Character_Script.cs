using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	public int TeamSide;
	public bool targeted = false;
	public Base_Character_Script Enemy;
	public string CharacterState;
	public float DistanceEnemy;
	public Base_Character_Script LastTouch = null;
	public bool Dead = false;

	public List<Base_Character_Script> PendingSideList;
	public List<Base_Character_Script> TeamSideList;
	public List<Base_Character_Script> Enemies;


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
		Debug.Log("Base start");
		MainBattle_Script.System.AllCharacters.Add(this);
		if(TeamSide == 1)
		{
			// assign team 1
			//MainBattle_Script.System.Team1.Add(Core_Character);
			//when battle starts, find nearest enemy
			//MainBattle_Script.System.PendingTeam1.Add(Core_Character);

			// Make shortcut to list,thus also allowing for teams 2>
			PendingSideList = MainBattle_Script.System.PendingTeam1;
			TeamSideList = MainBattle_Script.System.Team1;
			Enemies = MainBattle_Script.System.Team2;
		}
		else
		{
			// assign team 2
			//MainBattle_Script.System.Team2.Add(Core_Character);
			//when battle starts, find nearest enemy
			//MainBattle_Script.System.PendingTeam2.Add(Core_Character);

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

		Debug.Log("BaseUpdate");
		bool InRange = false;

		//work out dist from enemy

		if (Enemy != null)
		{
			DistanceEnemy = Vector3.Distance(this.Core_Character.transform.position, Enemy.Core_Character.transform.position);
			NavMesh.SetDestination(Enemy.transform.position);
		}
		else
		{
			PendingSideList.Add(this);
		}

		//is enemy in range?
		InRange = DistanceEnemy <= Base_MaxRange;
		//enemy in range
		if (InRange)
		{
			Attack();
		}
		//Have no Enemy
		//if(Enemy == null && Enemies.Count >0)
		//{
		//	InRange = false;
		//	PendingSideList.Add(this.gameObject);
		//}

	}


	void Attack()
	{

	}

	void KillCharacter()
	{
		Dead = true;
		MainBattle_Script.System.AllDeath.Add(this);
		NavMesh.Stop();
		NavMesh.ResetPath();
	}
}
