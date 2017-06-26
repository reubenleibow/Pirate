using UnityEngine;
using System.Collections;

public class TestAmo : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		this.transform.Translate(Vector3.forward * Time.deltaTime*5);
	}
}
