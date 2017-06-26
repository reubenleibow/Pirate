using UnityEngine;
using System.Collections;

public class Missile_Script : MonoBehaviour {

	public int speed = 50;
	public int damgage = 10;
	public int TeamNumber;
	public float y;
	//public Vector3 direction;



	// Use this for initialization
	void Start ()
	{
	}

	// Update is called once per frame
	void Update ()
	{
		y = this.transform.position.y;

		if(y < -10)
		{
			Destroy(this.gameObject);
		}

		this.transform.Translate(Vector3.forward * 1);
	}
}
