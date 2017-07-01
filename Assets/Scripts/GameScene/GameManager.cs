using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;
	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("GameManager : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	[HideInInspector]
	public List<CharacterDoll> dolls;
	[HideInInspector]
	public List<CharacterDoll> playerDoll;
	[HideInInspector]
	public List<CharacterDoll> enemyDoll;
	[HideInInspector]
	public CharacterDoll selectedDoll;
	//[HideInInspector]
	public float timer = 40.0f;
	[HideInInspector]
	public bool placeMode = true;
	[HideInInspector]
	public bool myTurn = true;
	[HideInInspector]
	public int turnCount = 0;
	[HideInInspector]
	public bool gameStopped = false;
	[HideInInspector]
	public CharacterDoll attacker;

	public GameObject characterBoard;

	private Board board;
	private PlaceMode place;
	private UIManager u;



	void Awake()
	{
		_instance = this;
	}

	void Start()
	{
		// Temporary!!!
		myTurn = (Random.Range(0, 2) == 1) ? true : false;

		Input.multiTouchEnabled = false;

		dolls = new List<CharacterDoll>();
		playerDoll = new List<CharacterDoll>();
		enemyDoll = new List<CharacterDoll>();

		board = Board.Instance;
		board.Initialize();

		PushAllDolls();

        DollController.Instance.Initialize();

		u = UIManager.Instance;
		u.Initialize();

		place = PlaceMode.Instance;
		place.StartPlaceMode();

		u.ShowCharacter();
	}

	void Update()
	{
		for (int i = 0; i < dolls.Count; ++i)
			dolls[i].Tick();
		if (placeMode)
			place.UpdatePlaceMode();
		else
			DollController.Instance.Tick();
		u.Tick();
		
		if (timer > 0f && !gameStopped)
			timer -= Time.deltaTime;
		if (timer <= 0f && !placeMode && !gameStopped)
			NextTurn();

		if (!myTurn && IsSelectingDoll())
			DeselectDoll();

		// Temporary!!!
		if (Input.GetKeyDown(KeyCode.T))
			NextTurn();
		// Temporary!!!
		if (!myTurn && timer <= 38f && !gameStopped)
		{
			int randIndex = Random.Range(0, 5);
			float randPower = Random.Range(0.5f, 10f);
			float randAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
			CharacterDoll ed = enemyDoll[randIndex];
			Rigidbody2D rb = ed.GetComponent<Rigidbody2D>();
			ed.transform.rotation = Quaternion.Euler(0f, 0f, randAngle);
			rb.AddForce(ed.transform.up * (randPower * 1280f));
			NextTurn();
		}
	}

	public void SelectDoll(CharacterDoll _doll)
	{
		selectedDoll = _doll;
		selectedDoll.gameObject.layer = LayerMask.NameToLayer("SelectedDoll");
		u.OpenActSelect(_doll);
	}

	public void DeselectDoll()
	{
		selectedDoll.gameObject.layer = LayerMask.NameToLayer("Doll");
		selectedDoll = null;
		DollController.Instance.attackType = 0;
    }

	public bool IsSelectingDoll()
	{
		return selectedDoll != null;
	}

	private CharacterDoll CreateCharacterDoll(int _id)
	{
		GameObject prefab = CharacterDB.Instance.GetCharacterDollPrefab(_id);
		GameObject dupe = Instantiate(prefab, characterBoard.transform);
		CharacterDoll doll = dupe.GetComponent<CharacterDoll>();
		doll.transform.localPosition = board.GetGridPosition(1, 1);
		doll.faction = new Faction(false, 0);
		return doll;
	}

	public CharacterDoll GetDoll(bool _isPlayer, int _order)
	{
		if (_isPlayer)
			return playerDoll[_order];
		else
			return enemyDoll[_order];
	}

	private void PushAllDolls()
	{
		Vector2 hidePos = new Vector2(-999f, -999f);
		for (int i = 0; i < 5; ++i)
		{
			Character cha = PlayerData.Instance.characters[i];
			int id = cha.id;
			Faction faction = new Faction(true, i);
			CharacterDoll doll = CreateCharacterDoll(id);
			doll.transform.localPosition = hidePos;
			doll.faction = faction;
			doll.Initialize(cha);
			dolls.Add(doll);
			playerDoll.Add(doll);
			doll.gameObject.SetActive(false);
		}
		List<Character> ec = LoadEnemyCharacters();
		for (int i = 0; i < 5; ++i)
		{
			Character cha = ec[i];
			int id = cha.id;
			Faction faction = new Faction(false, i);
			CharacterDoll doll = CreateCharacterDoll(id);
			doll.transform.localPosition = hidePos;
			doll.faction = faction;
			doll.Initialize(cha);
			dolls.Add(doll);
			enemyDoll.Add(doll);
			doll.gameObject.SetActive(false);
		}
	}

	private List<Character> LoadEnemyCharacters()
	{
		// Temporary!!!
		List<Character> chas = new List<Character>();
		chas.Add(CharacterDB.Instance.GetCharacter(0000));
		chas.Add(CharacterDB.Instance.GetCharacter(0000));
		chas.Add(CharacterDB.Instance.GetCharacter(0000));
		chas.Add(CharacterDB.Instance.GetCharacter(0000));
		chas.Add(CharacterDB.Instance.GetCharacter(0000));
		return chas;
	}

	public void NextTurn()
	{
		if (IsSelectingDoll())
			DeselectDoll();
		if (u.actSelect.opened)
			u.CloseActSelect();
		/*
		 * CancelControl
		if (u.cancelControlOpened)
			u.CloseCancelControl();
		*/

		StartCoroutine(CheckAllDollStop());
	}

	private IEnumerator CheckAllDollStop()
	{
		gameStopped = true;
		timer = 0f;
		yield return null;

		bool stopped = false;
		int count = dolls.Count;
        while (!stopped)
		{
			for (int i = 0; i <= count; ++i)
			{
				if (i == count)
				{
					stopped = true;
					break;
				}
				else if (!dolls[i].IsStopped())
					break;
            }
			yield return null;
		}

		myTurn = !myTurn;
		++turnCount;
		timer = 40f;
		attacker = null;
		gameStopped = false;
		yield return null;
	}
}
