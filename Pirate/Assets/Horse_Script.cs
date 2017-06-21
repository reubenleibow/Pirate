using UnityEngine;
using System.Collections;

public class Horse_Script : MonoBehaviour {

	bool Ridealbe = true;
	bool mounted = false;
	Base_Character_Script Rider;
	float Health = 100;
	int baseSpeed = 50;

	bool Dead = false;

	// Use this for initialization
	void Start ()
	{
		this.GetComponent<NavMeshAgent>().speed = baseSpeed;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Health <= 0)
		{
			Dead = true;
			this.GetComponent<NavMeshAgent>().enabled = false;
		}
	}

	void TravelTo(Vector3 destination)
	{
		this.GetComponent<NavMeshAgent>().SetDestination(destination);
	}

}
