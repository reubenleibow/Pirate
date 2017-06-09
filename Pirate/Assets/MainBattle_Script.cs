﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MainBattle_Script : MonoBehaviour
{
	public readonly List<GameObject> Team1;
	public readonly List<GameObject> Team2;
	public readonly List<GameObject> PendingTeam1;
	public readonly List<GameObject> PendingTeam2;
	public readonly List<GameObject> AllCharacters;
	public readonly List<GameObject> AllDeath;

	public static MainBattle_Script System { get; private set; }

	MainBattle_Script()
	{
		Team1 = new List<GameObject>();
		Team2 = new List<GameObject>();
		PendingTeam1 = new List<GameObject>();
		PendingTeam2 = new List<GameObject>();
		AllCharacters = new List<GameObject>();
		AllDeath = new List<GameObject>();

		MainBattle_Script.System = this;

	}
	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if(PendingTeam1.Count > 0)
		{
			var shortDist = 9999f;
			var shortDistNon = 9999f;

			//for team 1----------------------------------------

			foreach (GameObject man1 in PendingTeam1)
			{
				foreach (GameObject man2 in Team2)
				{
					GameObject target = null;
					GameObject targetNon = null;
					var dist = Vector3.Distance(man1.transform.position, man2.transform.position);

					//lock onto enemies regardless
					if (dist < shortDist)
					{
						shortDist = dist;
						target = man2;

						//lock onto enemies that are not already targeted
						if (man2.GetComponent<Base_Character_Script>().targeted == false)
						{
							shortDistNon = dist;
							targetNon = man2;
						}
					}
					//if non-targeted enemy found then set enemy V
					if (targetNon != null)
					{
						man1.GetComponent<Base_Character_Script>().Enemy = targetNon;
					}
					//if only targeted enemy found then set enemy V
					else
					{
						man1.GetComponent<Base_Character_Script>().Enemy = target;
					}

				}
				PendingTeam1.Remove(man1);
			}
		}
		//For Team 2-----------------------------------------
		if(PendingTeam2.Count > 0)
		{
			var shortDist = 9999f;
			var shortDistNon = 9999f;
			foreach (GameObject man2 in PendingTeam2)
			{
				foreach (GameObject man1 in Team1)
				{
					GameObject target = null;
					GameObject targetNon = null;
					var dist = Vector3.Distance(man2.transform.position, man1.transform.position);

					//lock onto enemies regardless
					if (dist < shortDist)
					{
						shortDist = dist;
						target = man1;

						//lock onto enemies that are not already targeted
						if (man2.GetComponent<Base_Character_Script>().targeted == false)
						{
							shortDistNon = dist;
							targetNon = man1;
						}
					}
					//if non-targeted enemy found then set enemy V
					if (targetNon != null)
					{
						man2.GetComponent<Base_Character_Script>().Enemy = targetNon;
					}
					//if only targeted enemy found then set enemy V
					else
					{
						man2.GetComponent<Base_Character_Script>().Enemy = target;
					}
				}
				PendingTeam1.Remove(man2);
			}
		}
		//if there is a death
		if(AllDeath.Count > 0)
		{
			foreach(GameObject death in AllDeath)
			{
				foreach(GameObject men in AllCharacters)
				{
					//if the man that died was last hit by a man then that man gets a kill
					if(death.GetComponent<Base_Character_Script>().LastTouch == men)
					{
						men.GetComponent<Base_Character_Script>().Base_Kills++;
					}
					//if a man has Enemy == man that just died then set null
					if(men.GetComponent<Base_Character_Script>().Enemy == death)
					{
						men.GetComponent<Base_Character_Script>().Enemy = null;
					}
				}
				AllDeath.Remove(death);
			}
		}
	}
}