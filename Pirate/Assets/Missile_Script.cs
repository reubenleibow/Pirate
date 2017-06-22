using UnityEngine;
using System.Collections;

public class Missile_Script : MonoBehaviour {

	public int speed = 100;
	public int damgage = 10;
	public float y;
	//public Vector3 direction;



	// Use this for initialization
	void Start ()
	{
		var front = this.transform.forward;
		//this.GetComponent<Rigidbody>().velocity = (front * speed);
		//Debug.Log("a" + front);
		//this.GetComponent<Rigidbody>().
	}

	// Update is called once per frame
	void Update ()
	{
		var v = this.GetComponent<Rigidbody>().velocity;
		//this.transform.rotation = Quaternion.LookRotation(v);

		y = this.transform.position.y;

		if(y < -10)
		{
			Destroy(this.gameObject);
		}

		this.transform.Translate(Vector3.forward * 1);
	}
}
