using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		this.GetComponent<Base_Character_Script>().AddItem("Broken Arrow", 10);
		this.GetComponent<Base_Character_Script>().AddItem("Broken Arrow", 10);

	}

	// Update is called once per frame
	void Update () {
	
	}
}
