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
	[HideInInspector]
	public CharacterDoll victim;
	[HideInInspector]
	public bool gameover = false;
	[HideInInspector]
	public int gainGold = 0;
		
	public GameObject characterBoard;
	public AudioClip gameBgm;

	private Board board;
	private PlaceMode place;
	private GameUIManager u;



	void Awake()
	{
		_instance = this;
	}

	void Start()
	{
        // Temporary!!!
        //myTurn = (Random.Range(0, 2) == 1) ? true : false;
        myTurn = PSocketManager.Instance.isStartTurn;
		Input.multiTouchEnabled = false;

		dolls = new List<CharacterDoll>();
		playerDoll = new List<CharacterDoll>();
		enemyDoll = new List<CharacterDoll>();

		board = Board.Instance;
		board.Initialize();

		PushAllDolls();

        DollController.Instance.Initialize();

		u = GameUIManager.Instance;
		u.Initialize();
		u.ShowCharacters();

		place = PlaceMode.Instance;
		place.StartPlaceMode();

		SoundManager.Instance.PlayBGM(gameBgm);
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
		if (Input.GetKeyDown(KeyCode.R))
			place.DebugPlace();
		// Temporary!!!
		if (Input.GetKeyDown(KeyCode.W))
			for (int i = 0; i < 4; ++i)
				enemyDoll[i].TakeDamage(99999);
		// Temporary!!!
		if (Input.GetKeyDown(KeyCode.E))
			for (int i = 0; i < 5; ++i)
				playerDoll[i].TakeDamage(99999);
		// Temporary!!! : AI
		if (!myTurn && timer <= 38f && !gameStopped)
		{
			int randIndex = Random.Range(0, 5);
			float randPower = Random.Range(0.5f, 10f);
			float randAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
			CharacterDoll ed = enemyDoll[randIndex];
			Rigidbody2D rb = ed.GetComponent<Rigidbody2D>();
			ed.transform.rotation = Quaternion.Euler(0f, 0f, randAngle);
			rb.AddForce(ed.transform.up * (randPower * 1280f));
			attacker = ed;
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
		chas.Add(CharacterDB.Instance.GetCharacter(0008));
		chas.Add(CharacterDB.Instance.GetCharacter(0010));
		chas.Add(CharacterDB.Instance.GetCharacter(0009));
		chas.Add(CharacterDB.Instance.GetCharacter(0006));
		chas.Add(CharacterDB.Instance.GetCharacter(0015));
		return chas;
	}

	public void NextTurn()
	{
		if (IsSelectingDoll())
			DeselectDoll();
		if (u.actSelect.opened)
			u.CloseActSelect();
		u.SetDollControlAimActive(false);
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
		float chkTimer = 0f;
        while (!stopped)
		{
			for (int i = 0; i <= count; ++i)
			{
				if (i == count)
				{
					chkTimer += Time.deltaTime;
					break;
				}
				else if (!dolls[i].IsStopped())
				{
					chkTimer = 0f;
					break;
				}
            }
			if (chkTimer >= 0.25f)
				break;
			yield return null;
		}

		if (!CheckGameOver())
		{
			myTurn = !myTurn;
			++turnCount;
			timer = 40f;
			gameStopped = false;
			if (myTurn)
				u.ShowMyTurnMessage();
			u.SetTurnArrow(myTurn);
			yield return null;
		}
	}

	private bool CheckGameOver()
	{
		bool allDead = true;
		for (int i = 0; i < 5; ++i)
		{
			if (!playerDoll[i].dead)
			{
				allDead = false;
				break;
			}
		}
		if (allDead)
		{
			GameOver(false);
			return true;
		}

		allDead = true;
		for (int i = 0; i < 5; ++i)
		{
			if (!enemyDoll[i].dead)
			{
				allDead = false;
				break;
			}
		}
		if (allDead)
		{
			GameOver(true);
			return true;
		}
		return false;
	}

	private void GameOver(bool _win)
	{
		gameover = true;
		gameStopped = true;
		PlayerData.Instance.gold += gainGold;
		u.ShowResult(_win, gainGold);
	}

	public void GoMainScene()
	{
		SceneManager.Instance.LoadMainScene();
	}
}
