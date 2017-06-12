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
	public static readonly float BOARD_WIDTH = 0.493f;

	[HideInInspector]
	public List<CharacterDoll> dolls;
	[HideInInspector]
	public List<CharacterDoll> playerDoll;
	[HideInInspector]
	public List<CharacterDoll> enemyDoll;
	[HideInInspector]
	public CharacterDoll selectedDoll;
	[HideInInspector]
	public float timer = 40.0f;
	[HideInInspector]
	public bool placeMode = true;
	[HideInInspector]
	public bool canMove = true;
	[HideInInspector]
	public bool myTurn = true;

	public GameObject characterBoard;



	void Awake()
	{
		_instance = this;
	}

	void Start()
	{
		Input.multiTouchEnabled = false;

		dolls = new List<CharacterDoll>();
		playerDoll = new List<CharacterDoll>();
		enemyDoll = new List<CharacterDoll>();
		UIManager.Instance.Initialize();
		StartPlaceMode();
		UIManager.Instance.ShowCharacter();
	}

	void Update()
	{
		UIManager.Instance.Tick();

		// 서버에서 처리할 필요가 있음.
		if (timer > 0f)
			timer -= Time.deltaTime;

		if (selectedDoll != null)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			if (Input.touchCount == 0)
				DeselectDoll();
			else
			{
				Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
				float angle = Mathf.Atan2(touchPos.y - selectedDoll.center.y, touchPos.x - selectedDoll.center.x) * Mathf.Rad2Deg + 90.0f;
				Quaternion q = Quaternion.Euler(0f, 0f, angle);
				selectedDoll.transform.rotation = q;
			}
#else
			if (Input.GetMouseButton(0) == false)
				DeselectDoll();
			else
			{
				Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				float angle = Mathf.Atan2(touchPos.y - selectedDoll.center.y, touchPos.x - selectedDoll.center.x) * Mathf.Rad2Deg + 90.0f;
				Quaternion q = Quaternion.Euler(0f, 0f, angle);
				selectedDoll.transform.rotation = q;
			}
#endif
		}
	}

	public void SelectDoll(CharacterDoll _doll)
	{
		selectedDoll = _doll;
	}

	public void DeselectDoll()
	{
		selectedDoll = null;
	}

	public bool IsSelectingDoll()
	{
		return selectedDoll != null;
	}

	public Vector2 GetGridPosition(int _row, int _column)
	{
		int column = Mathf.Clamp(_column, 1, 9) - 5;
		int row = Mathf.Clamp(_row, 1, 9) - 5;
		float x = BOARD_WIDTH * column;
		float y = BOARD_WIDTH * row;
		return new Vector2(x, y);
	}

	private void StartPlaceMode()
	{
		placeMode = true;
		for (int i = 0; i < 5; ++i)
		{
			Character cha = PlayerData.Instance.characters[i];
            int id = cha.id;
			Faction faction = new Faction(true, i);
			CharacterDoll doll = CreateCharacterDoll(id);
			doll.transform.localPosition = GetGridPosition(2, 2 * i + 1);
			doll.faction = faction;
			doll.Initialize(cha);
			dolls.Add(doll);
			playerDoll.Add(doll);
		}
	}

	private void EndPlaceMode()
	{
		placeMode = false;

	}

	private CharacterDoll CreateCharacterDoll(int _id)
	{
		GameObject prefab = CharacterDB.Instance.GetCharacterDollPrefab(_id);
		GameObject dupe = Instantiate(prefab, characterBoard.transform);
		CharacterDoll doll = dupe.GetComponent<CharacterDoll>();
		doll.transform.localPosition = GetGridPosition(1, 1);
		doll.faction = new Faction(false, 0);
		return doll;
	}
}
