using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestSprite : MonoBehaviour
{

	public Texture2D test;
	private Sprite sprite;

	// Use this for initialization
	void Start()
	{
		test = Resources.Load("Arrow_Sprite") as Texture2D;
		sprite = Sprite.Create(test, new Rect(0, 0, test.width, test.height), Vector2.zero);
	}

	// Update is called once per frame
	void Update()
	{
		var t = this.GetComponent<Image>();
		t.sprite = sprite;

		//this.GetComponent<Image>().sou = test;

	}
}
