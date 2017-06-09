using UnityEngine;
using System.Collections;

public class Base_Character_Script : MonoBehaviour
{
	float DifficaultyModifier = 1;

	int Base_Health = 100;
	bool Base_Ranged = true;
	bool Base_Melee = true;
	bool Base_HorseBack = false;
	float Base_Speed = 10;
	int Base_MaxRange = 4;
	string Base_Faction;
	public int Base_Kills = 0;
	
	int TeamSide;
	public readonly bool targeted = false;
	public GameObject Enemy;
	public readonly string CharacterState;
	public float DistanceEnemy;
	public GameObject LastTouch = null;

	GameObject Core_Character;

	// Use this for initialization
	void Start()
	{
		MainBattle_Script.System.AllCharacters.Add(Core_Character);
		if(TeamSide == 1)
		{
			MainBattle_Script.System.Team1.Add(Core_Character);
			//when battle starts, find nearest enemy
			MainBattle_Script.System.PendingTeam1.Add(Core_Character);
		}
		else
		{
			MainBattle_Script.System.Team2.Add(Core_Character);
			//when battle starts, find nearest enemy
			MainBattle_Script.System.PendingTeam2.Add(Core_Character);
		}
	}

	// Update is called once per frame
	void Update()
	{
		bool InRange = false;

		//work out dist from enemy
		if (Enemy != null)
		{
			DistanceEnemy = Vector3.Distance(Core_Character.transform.position, Enemy.transform.position);
		}

		//is enemy in range?
		if(DistanceEnemy <= Base_MaxRange)
		{
			InRange = true;
		}
		//enemy in range
		if (InRange)
		{
			Attack();
		}
	}

	void Attack()
	{

	}

	void Death()
	{
		MainBattle_Script.System.AllDeath.Add(Core_Character);
	}
}
