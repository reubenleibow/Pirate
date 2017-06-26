using UnityEngine;
using System.Collections;

public class System_Script : MonoBehaviour {

	public static System_Script SystemScriptCode;
	public  GameObject ArrowPrefab;


	// Use this for initialization
	void Start ()
	{
		ArrowPrefab = Resources.Load("Arrow_Missile") as GameObject;
	}
	// Update is called once per frame
	void Update ()
	{
	}


}
