using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Base_Character_Script : MonoBehaviour
{

	public bool playerControl = false;
	public bool GotAnimations = true;
	public bool Inpending = true;
	
	private GameObject coreCharacter;

	float DifficaultyModifier = 1;

	public float Base_Health = 100;
	public DatabaseInventoryItem Base_Ranged = null;
	public DatabaseInventoryItem Base_Melee;

	bool Base_HorseBack = false;
	float Base_Speed = 10;
	public int Base_MaxRange = 0;
	string Base_Faction;
	public int Base_Kills = 0;
	List<Items_Script> Inventory = new List<Items_Script>(GameDatabase.MaxInventorySize);

	public Horse_Script MountedHorse;

	// load the properties
	characterProperties Base_Propeties = new characterProperties();

	public int TeamSide;
	public bool targeted = false;
	public Base_Character_Script Enemy;
	public string CharacterState;
	public float DistanceEnemy;
	public Base_Character_Script LastTouch = null;
	public bool Dead = false;
	public WeaponChasis CurrentWeaponChasis = WeaponChasis.Nothing;

	private List<Base_Character_Script> PendingSideList;
	private List<Base_Character_Script> TeamSideList;
	private List<Base_Character_Script> Enemies;
	bool ForceStop = false;

	//fast ammo = ammo for that slot selected(Eg: just the one budle)
	public int fastAmmo = 0;
	//all all ammo is for weapon(Eg: all arrows)
	public int AllAmmo = 0;

	public Items_Script CurrentMissile = new Items_Script();

	//InventoryStuff
	public bool GotUsablePrimaryWeapon = false;
	public float Timer = 0.5f;
	//public int TestRange;
	bool Scan = true;
	public int CurrentInventoryIndex = 0;


	public Items_Script CurrentWeapon = new Items_Script();

	//controls variables
	public float scroll;
	public bool Fireable;

	public RangedWeaponsSates RangedWeaponsSates = RangedWeaponsSates.Nothing;

	public GameObject Core_Character
	{
		get { return coreCharacter ?? this.gameObject; }
		set { coreCharacter = value; }
	}

	//private NavMeshAgent NavMesh
	//{
	//	get { return Core_Character.GetComponent<NavMeshAgent>(); }
	//}

	private NavMeshAgent NavMesh;


	// Use this for initialization
	void Start()
	{
		//Check to see if it has all the animations

		//Test***********************
		AddItem("Bow", 1);
		//AddItem("Sword", 1);


		// set up battlefield
		MainBattle_Script.System.AllCharacters.Add(this);
		if (TeamSide == 1)
		{
			// Make shortcut to list,thus also allowing for teams 2>
			PendingSideList = MainBattle_Script.System.PendingTeam1;
			TeamSideList = MainBattle_Script.System.Team1;
			Enemies = MainBattle_Script.System.Team2;
		}
		else
		{
			// Make shortcut to list,thus also allowing for teams 2>
			PendingSideList = MainBattle_Script.System.PendingTeam2;
			TeamSideList = MainBattle_Script.System.Team2;
			Enemies = MainBattle_Script.System.Team1;
		}

		TeamSideList.Add(this);
		PendingSideList.Add(this);
		
		foreach(var cItem in Inventory)
		{
			var explainedItem = GameDatabase.InventoryItems[cItem.Name];
			var MeleeWeapon = false;

			if (explainedItem.WChasis == WeaponChasis.SingleHandMelee || explainedItem.WChasis == WeaponChasis.DoubleHandedWeapon)
			{
				Base_Melee = explainedItem;
				CurrentWeaponChasis = Base_Melee.WChasis;
				Base_MaxRange = Base_Melee.Range;
				MeleeWeapon = true;
			}

			if(MeleeWeapon == false)
			{
				Base_Melee = GameDatabase.InventoryItems["HandToHand"];
				MeleeWeapon = true;
			}

			if (explainedItem.WChasis == WeaponChasis.Ranged)
			{
				Base_Ranged = explainedItem;
				Base_MaxRange = Base_Ranged.Range;
			}
		}

		CheckForAnyWeapon();
	}

	// Update is called once per frame
	void Update()
	{
		//ANIMATIONS
		
		//Is moving
		if (GotAnimations == true)
		{
			if ((this.GetComponent<NavMeshAgent>().velocity.x != 0 || this.GetComponent<NavMeshAgent>().velocity.z != 0))
			{
				//this.GetComponent<Animation>().Play("Walking");
				//this.GetComponent<Animation>()["Walking"].speed = 2;
			}
		}

		if(Base_Health <= 0)
		{
			Dead = true;
		}
		

		if (!playerControl)
		{
			HandleAiControl();
			this.GetComponent<NavMeshAgent>().enabled = true;
		}
		else
		{
			HandlePlayerControl();
			this.GetComponent<NavMeshAgent>().enabled = false;
		}

		// all affected=
		if (MountedHorse != null)
		{
			this.GetComponent<NavMeshAgent>().enabled = false;
			this.GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	void HandleAiControl()
	{
		Timer -= Time.deltaTime;

		if (Dead)
		{
			this.gameObject.SetActive(false);
			return;
		}

		bool InRange = false;

		if (MountedHorse != null)
		{
			this.transform.position = MountedHorse.transform.position;
			NavMesh = MountedHorse.GetComponent<NavMeshAgent>();
			//this.GetComponent<NavMeshAgent>().enabled = false;
		}
		else
		{
			//this.GetComponent<NavMeshAgent>().enabled = true
			NavMesh = this.GetComponent<NavMeshAgent>();

		}

		//work out dist from enemy
		if (Enemy != null)
		{
			DistanceEnemy = Vector3.Distance(this.Core_Character.transform.position, Enemy.Core_Character.transform.position);
			//test***************************
			if (ForceStop == false && CurrentWeaponChasis != WeaponChasis.Ranged)
			{
				MoveTo(Enemy.transform.position);
			}

			if(Enemy.GetComponent<Base_Character_Script>().Dead == true && Inpending == false)
			{
				Enemy = null;
				PendingSideList.Add(this);
				Inpending = true;
			}

			InRange = DistanceEnemy <= Base_MaxRange;

		}
		else if(Inpending == false)
		{
			PendingSideList.Add(this);
			Inpending = true;
		}

		bool x = false;

		//is enemy in range?

		//enemy in range
		if (InRange && Enemy != null)
		{
			fastAmmo = CurrentMissile.Quantity;
			var IsMelee = CurrentWeaponChasis == WeaponChasis.HandToHand || CurrentWeaponChasis == WeaponChasis.DoubleHandedWeapon || CurrentWeaponChasis == WeaponChasis.SingleHandMelee;

			// if (Eg: bow) is being used, 
			if (CurrentWeaponChasis == WeaponChasis.Ranged && fastAmmo > 0)
			{
				//****************
				this.transform.LookAt(Enemy.transform.position);
				//shoot every second
				if (Timer <= 0)
				{
					Scan = true;
					FireMissile(CurrentMissile);
				}
			}

			// when no ammo is left, or all is used up.
			if (fastAmmo == 0 && Scan == true && CurrentWeaponChasis == WeaponChasis.Ranged)
			{
				CheackForAmmo();
				Scan = false;
			}

			if(IsMelee)
			{
				Engage(CurrentWeaponChasis,Base_Melee.Damage);
			}

			//when character must remain still
			x = x || (CurrentWeaponChasis == WeaponChasis.Ranged || Dead);
		}

		// switch to melee
		if (DistanceEnemy < Base_Propeties.MinBowRange && CurrentWeaponChasis == WeaponChasis.Ranged && Base_Melee != null && Enemy != null)
		{
			CurrentWeaponChasis = Base_Melee.WChasis;
			Base_MaxRange = Base_Melee.Range;
			CurrentWeapon.Name = Base_Melee.Name;
		}

		// switch to ranged
		if (Base_Ranged != null && DistanceEnemy > Base_Propeties.MinStartRanged && Enemy != null)
		{
			CurrentWeaponChasis = Base_Ranged.WChasis;
			CurrentWeapon.Name = Base_Ranged.Name;
			Base_MaxRange = Base_Ranged.Range;
			//CheackForAmmo();
		}

		ForceStop = x;

		//stop all movement if force stop is true**************************
		if (ForceStop || CurrentWeaponChasis == WeaponChasis.Ranged)
		{
			NavMesh.Stop();
			NavMesh.ResetPath();
		}
		//*************************************
		if (Timer <= 0)
		{
			Timer = 0.5f;
		}
	}

	//private void OnCollisionEnter(Collision collision)
	//{
	//	if (collision.gameObject.tag == "Missile")
	//	{
	//		var missile = collision.gameObject;
	//		var missileScript = missile.GetComponent<Missile_Script>();
	//
	//		if (missileScript.TeamNumber != TeamSide)
	//		{
	//			Base_Health -= missile.GetComponent<Missile_Script>().damgage;
	//			Destroy(collision.gameObject);
	//		}
	//
	//	}
	//
	//}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Missile")
		{
			var missile = other.gameObject;
			var missileScript = missile.GetComponent<Missile_Script>();

			if(missileScript.TeamNumber != TeamSide)
			{
				Base_Health -= missile.GetComponent<Missile_Script>().damgage;
				Destroy(missile);
			}
		}
	}

	//for player control only
	void HandlePlayerControl()
	{
		var text = GameObject.Find("Text");

		text.GetComponent<Text>().text = AllAmmo.ToString();

		//propeties that are not set and give error are set here....
		if (NavMesh != null)
		{
			NavMesh.enabled = false;
		}

		var dt = Time.deltaTime;
		var Cam = this.GetComponent<GodProperties>().MainCam;

		Cam.transform.position = this.transform.position;
		Cam.transform.Translate(Vector3.up * 1.5f);

		if (Input.GetKey("w"))
		{
			this.transform.Translate(Vector3.forward * 2*dt);
		}

		if (Input.GetKey("d"))
		{
			this.transform.Translate(Vector3.right * 2*dt);
		}

		if (Input.GetKey("a"))
		{
			this.transform.Translate(Vector3.right * -2*dt);
		}

		if (Input.GetKey("s"))
		{
			this.transform.Translate(Vector3.forward * -2*dt);
		}

		if(Input.GetKey("s") || Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("d"))
		{
			this.GetComponent<Animation>().Play("Walking");
		}

		if(Input.GetKeyDown(KeyCode.Space))
		{
			this.GetComponent<Rigidbody>().velocity = new Vector3(0,4,0);
		}

		if (Input.GetMouseButtonDown(0) && CurrentWeaponChasis != WeaponChasis.Ranged)
		{
			Engage(CurrentWeaponChasis, Base_Melee.Damage);
		}

		//Load up an arrow
		if(Input.GetMouseButton(0) && CurrentWeaponChasis == WeaponChasis.Ranged && AllAmmo > 0 && RangedWeaponsSates == RangedWeaponsSates.Nothing)
		{
			PlayAnimation("PullOutArrow", 1);
			RangedWeaponsSates = RangedWeaponsSates.PullOutMissile;
		}
		// pack arrow away
		if (Input.GetMouseButtonDown(1) && (RangedWeaponsSates == RangedWeaponsSates.Aiming || RangedWeaponsSates == RangedWeaponsSates.PullOutMissile))
		{
			PlayAnimation("PullOutArrow", -1);
			RangedWeaponsSates = RangedWeaponsSates.PutArrowAway;
		}
		//fire arrow
		if(!Input.GetMouseButton(0) && CurrentWeaponChasis == WeaponChasis.Ranged && RangedWeaponsSates == RangedWeaponsSates.Aiming)
		{
			Engage(CurrentWeaponChasis, Base_Melee.Damage);
			RangedWeaponsSates = RangedWeaponsSates.Release;
			PlayAnimation("PullOutArrow", -1);
		}
		//firing before complete
		if (Input.GetMouseButton(0) && CurrentWeaponChasis == WeaponChasis.Ranged && AllAmmo > 0 && RangedWeaponsSates == RangedWeaponsSates.Release)
		{
			PlayAnimation("ShootAndReload", 0.1f);
			RangedWeaponsSates = RangedWeaponsSates.PullOutMissile;
		}

		if (fastAmmo <= 0 && AllAmmo > 0)
		{
			CheackForAmmo();
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.lockState = CursorLockMode.None;
		var h = Input.GetAxis("Mouse X");
		var v = Input.GetAxis("Mouse Y");


		Cam.transform.Rotate(-v, 0, 0);
		this.transform.Rotate(0, h*2, 0);
		Cam.transform.parent = this.transform;

		//display ammo

		//Scrolling control
		if (Input.GetAxisRaw("Mouse ScrollWheel") != 0)//&& GotUsablePrimaryWeapon == true
		{
			scroll += Input.GetAxis("Mouse ScrollWheel");
		}

		if (scroll >= 1)
		{
			ScrolledUp();
			scroll = 0;
		}

		if (scroll <= -1)
		{
			ScrolledDown();
			scroll = 0;
		}
	}

	//All attack (player/Ai)
	void Engage (WeaponChasis chasis,int damage)
	{
		if(chasis == WeaponChasis.Ranged && AllAmmo > 0)
		{
			FireMissile(CurrentMissile);
			PlayAnimation("PullOutArrow", 1);
		}

		if (chasis == WeaponChasis.HandToHand || chasis == WeaponChasis.DoubleHandedWeapon || chasis == WeaponChasis.SingleHandMelee)
		{
			PlayAnimation("Attack", 1);
		}

		if (playerControl == false)
		{
			Enemy.GetComponent<Base_Character_Script>().Base_Health -= damage * Time.deltaTime;
		}
	}


	void KillCharacter()
	{
		Dead = true;
		MainBattle_Script.System.AllDeath.Add(this);
		NavMesh.Stop();
		NavMesh.ResetPath();
	}

	public int AddItem(string name, int quantity)
	{
		int leftOverQuantity = quantity;
		var maxQty = GameDatabase.InventoryItems[name].MaxQuantity;

		var freeSpaces = Inventory
			.Where(i => i.Name == name)
			.Where(i => i.Quantity < maxQty)
			.ToArray();

		// if 0 therefore used all else X left over(only if stackable)
		foreach (Items_Script currentItem in freeSpaces)
		{
			var freeSpace = maxQty - (currentItem.Quantity + leftOverQuantity);

			leftOverQuantity = freeSpace > 0 ? 0 : (freeSpace * -1);

			if (leftOverQuantity == 0)
			{
				currentItem.Quantity += quantity;
			}
			else
			{
				currentItem.Quantity = maxQty;
			}

		}

		// fill up to capacity but still left over(not stackable/got left overs)
		for (int i = GameDatabase.MaxInventorySize - Inventory.Count; i > 0 && leftOverQuantity > 0; i--)
		{
			//AddItem(currentItem.Chasis, currentItem.Name, currentItem.)
			Inventory.Add(new Items_Script
			{
				Name = name,
				Quantity = (leftOverQuantity > maxQty) ? maxQty : leftOverQuantity
			});
			leftOverQuantity -= maxQty;
		}

		CheckForAnyWeapon();
		return leftOverQuantity > 0 ? leftOverQuantity : 0;
	}

	void CheackForAmmo()
	{
		var ammo = 0;
		//player control ammo
		AllAmmo = 0;


		//Check ammo for current weapon
		foreach (var cItem in Inventory.Where(q => q.Quantity > 0).ToArray())
		{

			var DataItem = GameDatabase.InventoryItems[cItem.Name];

			// if same class and is ammo
			if (DataItem.Chasis == Base_Ranged.Chasis && DataItem.WChasis == WeaponChasis.Ammo)
			{
				//add to ammo
				AllAmmo += cItem.Quantity;

				// if the chosen ammo matches the current missile, the set ammo to the right amount;
				if(cItem.Name == CurrentMissile.Name && fastAmmo > 0 && (cItem.Quantity + fastAmmo) < DataItem.MaxQuantity)
				{
					cItem.Quantity = fastAmmo;
					fastAmmo = 0;
				}

				ammo += cItem.Quantity;
				// if there is ammo in a pack then set current missile and fast ammo;
				if(fastAmmo == 0 && ammo > 0)
				{
					CurrentMissile = cItem;
					fastAmmo = cItem.Quantity;
				}
			}
		}

		if(ammo == 0 && playerControl == false)
		{
			//no ammo for current weapon;
			FindRangedWeapon();
		}

	}

	void FindRangedWeapon()
	{
		var ammo = 0;

		foreach (var cItem in Inventory.ToArray())
		{
			if (ammo == 0)
			{
				// Item of weapon
				var DataItem = GameDatabase.InventoryItems[cItem.Name];

				//picked ranged weapon
				if (DataItem.WChasis == WeaponChasis.Ranged)
				{
					//search for ammo for that item(DataItem = the picked item)
					foreach (var ammoItem in Inventory.ToArray())
					{
						var DataAmmo = GameDatabase.InventoryItems[ammoItem.Name];

						//If of the same class(both arrow group) and if is ammo...
						if (DataAmmo.Chasis == DataItem.Chasis && DataAmmo.WChasis == WeaponChasis.Ammo)
						{
							if(CurrentMissile != null)
							{
								if (ammoItem.Name == CurrentMissile.Name && fastAmmo > 0)
								{
									ammoItem.Quantity = fastAmmo;
									fastAmmo = 0;
								}
							}

							ammo += ammoItem.Quantity;

							// if there is ammo in a pack then set current missile and fast ammo;
							if (fastAmmo == 0 && ammo > 0)
							{
								CurrentMissile = ammoItem;
								fastAmmo = ammoItem.Quantity;
							}
						}
					}
					// if that item has ammo then make that the base weapon
					if (ammo > 0)
					{
						Base_Ranged = DataItem;
					}
				}
			}
		}
		//if no weapon is found then switch to melee
		if (ammo == 0 && Base_Melee != null)
		{
			Debug.Log("No other weapons");
			Base_Ranged = null;

			FindMeleeWeapon();
		}
	}

	void FindMeleeWeapon()
	{
		foreach(var i in Inventory)
		{
			var explainedItem = GameDatabase.InventoryItems[i.Name];
			var MeleeFound = false;
			var HandToHandWeapon = GameDatabase.InventoryItems["HandToHand"];

			if (explainedItem.WChasis == WeaponChasis.DoubleHandedWeapon || explainedItem.WChasis == WeaponChasis.SingleHandMelee)
			{
				//a weapon is available
				CurrentWeaponChasis = explainedItem.WChasis;
				Base_Melee = explainedItem;
				Base_MaxRange = explainedItem.Range;
				MeleeFound = true;
			}

			if(MeleeFound == false)
			{
				CurrentWeaponChasis = HandToHandWeapon.WChasis;
				Base_Melee = HandToHandWeapon;
				Base_MaxRange = HandToHandWeapon.Range;
				MeleeFound = true;
			}
		}
	}

	void MoveTo(Vector3 position)
	{
		if(MountedHorse != null)
		{
			MountedHorse.GetComponent<NavMeshAgent>().SetDestination(position);
		}
		else
		{
			this.GetComponent<NavMeshAgent>().SetDestination(position);
		}
	}

	void FireMissile(Items_Script missleFired)
	{
		var dataItem = GameDatabase.InventoryItems[missleFired.Name];
		var arrow = Resources.Load("Arrow_Missile") as GameObject;
		//Fire a missile
		var newMissile = Instantiate(arrow, this.transform.position, Quaternion.Euler(this.transform.eulerAngles)) as GameObject;

		CurrentMissile.Quantity--;
		newMissile.GetComponent<Missile_Script>().TeamNumber = TeamSide;

		if(playerControl == true)
		{
			AllAmmo--;
			fastAmmo--;
		}

		if(playerControl == true)
		{
			newMissile.transform.rotation = Quaternion.Euler(this.GetComponent<GodProperties>().MainCam.transform.eulerAngles);
		}
	}

	// only does checking
	void CheckForAnyWeapon()
	{
		var GotMelee = false;
		var GotRanged = false;
		//var MeleeWeapon = new Items_Script();
		//check for any usable weapon, If ranged then check to see if it has ammo, else not usable
		if(Inventory.Count > 0)
		{
			foreach (var cItem in Inventory.ToArray())
			{
				var ExplainedItem = GameDatabase.InventoryItems[cItem.Name];

				if (ExplainedItem.WChasis == WeaponChasis.DoubleHandedWeapon || ExplainedItem.WChasis == WeaponChasis.SingleHandMelee)
				{
					//A melle Item is avaliable
					//MeleeWeapon = cItem;
					GotMelee = true;
				}

				//check for ranged weapons
				if (ExplainedItem.WChasis == WeaponChasis.Ranged)
				{
					foreach (var cAmmo in Inventory.ToArray())
					{
						var ExplainedAmmo = GameDatabase.InventoryItems[cAmmo.Name];
						Debug.Log("The Check: " + ExplainedAmmo.Name + ", Chasis: " + ExplainedAmmo.Chasis + " ,WChasis: " + ExplainedAmmo.WChasis);
						//If the current checked item has ammo then there is a possible use of a ranged weapon
						if (ExplainedAmmo.Chasis == ExplainedItem.Chasis && ExplainedAmmo.WChasis == WeaponChasis.Ammo)
						{
							GotRanged = true;
						}
					}
				}
			}
		}

		//GotMelee == true, therefore possible ranged.
		//GotRanged == true, therefore there is a possible ranged weapon to use.

		if (GotMelee == false && GotRanged == false)
		{
			GotUsablePrimaryWeapon = false;
		}

		if (GotMelee == true || GotRanged == true)
		{
			GotUsablePrimaryWeapon = true;
		}
	}

	//for player control only
	void ScrolledUp()
	{
		CurrentInventoryIndex ++;

		if (CurrentInventoryIndex > Inventory.Count)
		{
			//keep index within limits
			CurrentInventoryIndex = Inventory.Count;
		}

		if (CurrentInventoryIndex < Inventory.Count && Inventory.Count >0)
		{
			//find what that item is
			var ExplainedItem = GameDatabase.InventoryItems[Inventory[CurrentInventoryIndex].Name];

			//skip over non-weapon items by recalling the function
			if (ExplainedItem.WChasis == WeaponChasis.Ammo || ExplainedItem.WChasis == WeaponChasis.Nothing)
			{
				//not a weapon
				ScrolledUp();
			}
			else
			{
				//If there is a weapon in this inventory then set current weapon to this item
				CurrentWeapon.Name = ExplainedItem.Name;
				CurrentWeaponChasis = ExplainedItem.WChasis;
			}
		}
		else
		{
			CurrentWeaponChasis = WeaponChasis.HandToHand;
			CurrentWeapon.Name = "HandToHand";
		}

		CurrentWeaponChange();
	}

	//for player control only
	void ScrolledDown()
	{
		CurrentInventoryIndex--;

		if (CurrentInventoryIndex < -1)
		{
			CurrentInventoryIndex = -1;
		}

		if(CurrentInventoryIndex < Inventory.Count && Inventory.Count > 0 && CurrentInventoryIndex >= 0)
		{
			var ExplainedItem = GameDatabase.InventoryItems[Inventory[CurrentInventoryIndex].Name];

			//skip over non-weapon items by recalling the function
			if (ExplainedItem.WChasis == WeaponChasis.Ammo || ExplainedItem.WChasis == WeaponChasis.Nothing)
			{
				ScrolledDown();
			}
			else
			{
				//If there is a weapon in this inventory then set current weapon to this item
				CurrentWeapon.Name = ExplainedItem.Name;
				CurrentWeaponChasis = ExplainedItem.WChasis;
			}
		}
		else
		{
			CurrentWeaponChasis = WeaponChasis.HandToHand;
			CurrentWeapon.Name = "HandToHand";
		}

		CurrentWeaponChange();
	}

	//for player control only
	void CurrentWeaponChange()
	{
		var hud = GameObject.Find("Hud");
		var text = GameObject.Find("Text");
		var CurrentItem = GameDatabase.InventoryItems[CurrentWeapon.Name];

		if (playerControl == true)
		{
			if (CurrentItem.Chasis == Chasis.Arrow)
			{
				var image = Resources.Load("Arrow_Sprite") as Texture2D;
				hud.GetComponent<Image>().sprite = Sprite.Create(image, new Rect(0,0, image.width, image.height), Vector2.zero);

				text.GetComponent<Text>().enabled = true;
			}

			if (CurrentItem.Chasis == Chasis.SingleHandWeapon)
			{
				var image = Resources.Load("Sword_Sprite") as Texture2D;
				hud.GetComponent<Image>().sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.zero);

				text.GetComponent<Text>().enabled = false;
			}

			if(CurrentItem.WChasis == WeaponChasis.HandToHand)
			{
				var image = Resources.Load("Fists") as Texture2D;
				hud.GetComponent<Image>().sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector3.zero);

				text.GetComponent<Text>().enabled = false;
			}
		}

		//if the selected item is ranged then look for ammo
		if (CurrentItem.WChasis == WeaponChasis.Ranged)
		{
			CheackForAmmo();
		}
		else
		{
			AllAmmo = 0;
		}
	}

	void PlayAnimation(string animation, float speed)
	{
		var PlayClip = this.GetComponent<Animation>();

		PlayClip.Play(animation);
		this.GetComponent<Animation>()[animation].speed = speed;

		if(speed == -1)
		{
			this.GetComponent<Animation>()[animation].time = this.GetComponent<Animation>()[animation].length-0.1f;
		}
	}

	void AnimationOver()
	{
		Fireable = true;

		if(RangedWeaponsSates == RangedWeaponsSates.PullOutMissile)
		{
			RangedWeaponsSates = RangedWeaponsSates.Aiming;
		}

		if (RangedWeaponsSates == RangedWeaponsSates.PutArrowAway)
		{
			RangedWeaponsSates = RangedWeaponsSates.Nothing;
		}

		if (RangedWeaponsSates == RangedWeaponsSates.Release)
		{
			RangedWeaponsSates = RangedWeaponsSates.Nothing;
		}
	}
}

