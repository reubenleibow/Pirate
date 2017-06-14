using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainBattle_Script : MonoBehaviour
{
	public readonly List<Base_Character_Script> Team1;
	public readonly List<Base_Character_Script> Team2;
	public readonly List<Base_Character_Script> PendingTeam1;
	public readonly List<Base_Character_Script> PendingTeam2;
	public readonly List<Base_Character_Script> AllCharacters;
	public readonly List<Base_Character_Script> AllDeath;

	public static MainBattle_Script System { get; private set; }

	MainBattle_Script()
	{
		Team1 = new List<Base_Character_Script>();
		Team2 = new List<Base_Character_Script>();
		PendingTeam1 = new List<Base_Character_Script>();
		PendingTeam2 = new List<Base_Character_Script>();
		AllCharacters = new List<Base_Character_Script>();
		AllDeath = new List<Base_Character_Script>();

		MainBattle_Script.System = this;

	}

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (PendingTeam1.Count > 0 && Team2.Count > 0)
		{
			SortPlayers(PendingTeam1, Team2);
		}
		//For Team 2
		if (PendingTeam2.Count > 0 && Team1.Count > 0)
		{
			SortPlayers(PendingTeam2, Team1);
		}
		//if there is a death
		if (AllDeath.Count > 0)
		{
			foreach (var death in AllDeath.ToArray())
			{
				foreach (var men in AllCharacters)
				{
					//if the man that died was last hit by a man then that man gets a kill
					if (death.LastTouch == men)
					{
						men.Base_Kills++;
					}
					//if a man has Enemy == man that just died then set null
					if (men.Enemy == death)
					{
						men.Enemy = null;
					}
				}
				AllDeath.Remove(death);
			}
		}
	}

	private static void SortPlayers(List<Base_Character_Script> firstTeam, List<Base_Character_Script> secondTeam)
	{
		foreach (var editingMan in firstTeam.Where(m => !m.Dead).ToArray())
		{
			var shortDist = float.MaxValue;
			var shortDistNon = float.MaxValue;

			//target is currently targeted by other people
			Base_Character_Script enemyTarget = null;
			//target that has not let been targeted
			Base_Character_Script enemyTargetFree = null;

			foreach (var secondMan in secondTeam.Where(f => !f.Dead))
			{
				var dist = Vector3.Distance(editingMan.transform.position, secondMan.transform.position);

				//lock onto enemies regardless
				if (dist <= shortDist)
				{
					shortDist = dist;
					enemyTarget = secondMan;

					//lock onto enemies that are not already targeted
					if (secondMan.targeted == false)
					{
						shortDistNon = dist;
						enemyTargetFree = secondMan;
					}
				}
			}

			var target = enemyTargetFree ?? enemyTarget;
			if (target != null)
			{
				editingMan.Enemy = target;
				//set enemy to targeted
				target.targeted = true;
			}

			firstTeam.Remove(editingMan);
		}
	}
}
