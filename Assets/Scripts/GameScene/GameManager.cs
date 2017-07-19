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
	[HideInInspector]
	public List<Skill> skills;
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
	[HideInInspector]
	public int unlockTarget = 0;

	public GameObject characterBoard;
	public GameObject skillBoard;
	public AudioClip gameBgm;
	public AudioClip hitFx;

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
		myTurn = (Random.Range(0, 2) == 1) ? true : false;

		Input.multiTouchEnabled = false;

		dolls = new List<CharacterDoll>();
		playerDoll = new List<CharacterDoll>();
		enemyDoll = new List<CharacterDoll>();
		skills = new List<Skill>();

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
		for (int i = 0; i < skills.Count; ++i)
			skills[i].Tick();
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
				enemyDoll[i].TakeDamage(enemyDoll[i].chaInfo.hp);
		// Temporary!!!
		if (Input.GetKeyDown(KeyCode.E))
			for (int i = 0; i < 5; ++i)
				playerDoll[i].TakeDamage(playerDoll[i].chaInfo.hp);
		// Temporary!!! : AI
		if (!myTurn && timer <= 38f && !gameStopped)
			AIThink();
	}

	private void AIThink()
	{
		int randIndex = Random.Range(0, 5);
		if (enemyDoll[randIndex].dead)
		{
			for (int i = 0; i < 5; ++i)
			{
				if (!enemyDoll[i].dead)
				{
					randIndex = i;
					break;
				}
			}
		}
		float randPower = Random.Range(5.5f, 10f);
		CharacterDoll ed = enemyDoll[randIndex];

		randIndex = Random.Range(0, 5);
		if (playerDoll[randIndex].dead)
		{
			for (int i = 0; i < 5; ++i)
			{
				if (!playerDoll[i].dead)
				{
					randIndex = i;
					break;
				}
			}
		}
		CharacterDoll tar = playerDoll[randIndex];

		float randAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
		Rigidbody2D rb = ed.GetComponent<Rigidbody2D>();
	
		ed.transform.rotation = CalculateLookRotation(ed, tar);

		rb.AddForce(ed.transform.up * (randPower * 1280f));
		attacker = ed;
		NextTurn();
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
			Character c = PlayerData.Instance.characters[i];
            int lv = PlayerData.Instance.chaLevel[c.id - 1];
			Character cha = c.CalculateLevel(lv);
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
			bool dark = true;
			Character cha = ec[i];
			int id = cha.id;
			if (id == unlockTarget)
				dark = false;
			Faction faction = new Faction(false, i);
			CharacterDoll doll = CreateCharacterDoll(id);
			doll.transform.localPosition = hidePos;
			doll.faction = faction;
			doll.Initialize(cha);
			dolls.Add(doll);
			if (dark)
				doll.MakeDarkSprite();
			else
				doll.MakeRedSprite();
            enemyDoll.Add(doll);
			doll.gameObject.SetActive(false);
		}
	}

	private List<Character> LoadEnemyCharacters()
	{
		List<Character> chas = new List<Character>();

		for (int i = 0; i < 5; ++i)
		{
			Character c = PlayerData.Instance.characters[i];
            int lv = (int)(PlayerData.Instance.chaLevel[c.id - 1] * 1.1f);
			int eid = PlayerData.Instance.enemys[i];
			if (eid < 0)
			{
				eid *= -1;
				unlockTarget = eid;
			}
			Character cha = CharacterDB.Instance.GetCharacter(eid).CalculateLevel(lv);
			chas.Add(cha);
		}

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

		StartCoroutine(WaitAndNextTurn());
	}

	private IEnumerator WaitAndNextTurn()
	{
		gameStopped = true;
		timer = 0f;
		yield return null;

		bool stopped = false;
		int count = dolls.Count;
		float chkTimer = 0f;

		Coroutine waitSkill = StartCoroutine(WaitSkills());
		yield return waitSkill;

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

		for (int i = 0; i < dolls.Count; ++i)
			dolls[i].OnTurnEnd();
		for (int i = 0; i < skills.Count; ++i)
			skills[i].OnTurnEnd();

		waitSkill = StartCoroutine(WaitSkills());
		yield return waitSkill;

		if (!CheckGameOver())
		{
			myTurn = !myTurn;
			++turnCount;
			timer = 40f;
			gameStopped = false;
			for (int i = 0; i < dolls.Count; ++i)
				dolls[i].receivedDamage = 0;
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
		if (_win)
		{
			int divide = PlayerData.Instance.selectedDivide - 1;
			int stage = PlayerData.Instance.selectedStage;
			if (stage == PlayerData.Instance.clearedStage[divide] + 1)
				PlayerData.Instance.clearedStage[divide] += 1;

			if (unlockTarget > 0)
			{
				if (PlayerData.Instance.chaLevel[unlockTarget - 1] == 0)
					PlayerData.Instance.chaLevel[unlockTarget - 1] = 1;
				else
					unlockTarget = 0;
			}
        }
		int g = (int)(gainGold * 0.1f);
		if (g < 1)
			g = 1;
		if (PlayerData.Instance.selectedStage == 6 && _win)
			g *= 2;
		PlayerData.Instance.gold += g;
		PlayerData.Instance.SaveData();
		u.ShowResult(_win, g);
	}

	private IEnumerator WaitSkills()
	{
		bool end = false;
		while (true)
		{
			end = true;
			for (int i = 0; i < skills.Count; ++i)
			{
				if (skills[i].active)
				{
					end = false;
					break;
				}

			}
			if (end)
				break;
			yield return true;
		}
	}

	public void GoMainScene()
	{
		SceneManager.Instance.LoadMainScene();
	}

	public static Quaternion CalculateLookRotation(Vector3 _posStart, Vector3 _posEnd)
	{
		return Quaternion.Euler(0, 0, -Mathf.Atan2(_posEnd.x - _posStart.x, _posEnd.y - _posStart.y) * Mathf.Rad2Deg);
	}

	public static Quaternion CalculateLookRotation(CharacterDoll _me, CharacterDoll _tar)
	{
		Vector3 posStart = _me.transform.position;
		Vector3 posEnd = _tar.transform.position;
		return Quaternion.Euler(0, 0, -Mathf.Atan2(posEnd.x - posStart.x, posEnd.y - posStart.y) * Mathf.Rad2Deg);
	}
}
