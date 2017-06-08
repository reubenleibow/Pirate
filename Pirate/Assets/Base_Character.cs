using UnityEngine;
using System.Collections;

public class Base_Character : MonoBehaviour
{
	float DifficaultyModifier = 1;

	int Base_Health = 100;
	bool Base_Ranged = true;
	bool Base_Melee = true;
	bool Base_HorseBack = false;
	float Base_Speed = 10;
	int Base_MaxRange = 4;

	GameObject Core_Character;



	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		var InRange = Core_Character;
	}
}
