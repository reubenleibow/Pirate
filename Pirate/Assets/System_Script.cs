using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class System_Script : MonoBehaviour {

	public static System_Script SystemScriptCode;
	public  GameObject ArrowPrefab;
	public Text Text_UI;

	public Sprite Arrow_Sprite;


	// Use this for initialization
	void Start ()
	{
		ArrowPrefab = Resources.Load("Arrow_Missile") as GameObject;
		Arrow_Sprite = Resources.Load("Arrow_Sprite") as Sprite;
	}
	// Update is called once per frame
	void Update ()
	{
	}


}
