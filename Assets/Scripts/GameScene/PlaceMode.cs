using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceMode : MonoBehaviour
{
	private static PlaceMode _instance;
	public static PlaceMode Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("PlaceMode : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	public AudioClip placeFx;

	private GameManager g;
	private GameUIManager u;
	private int order = 0;
	private Vector2 lastAim;
	private int aimIndex = 0;
	private List<Vector2> remainPlace;



	/*
	 * PlaceMode
	 * 1. myTurn이 true일 경우 현재 차례(order)의 캐릭터를 가져온다.
	 * 2. UI에서 현재 차례의 캐릭터를 보여주고(스케일, 화살표 등) 타이머를 맞춘다.
	 * 3. 보드의 격자를 선택하면 표식을 두고 다시 클릭하거나 '선택'버튼을 누르면 배치된다.
	 * 4. 타이머가 0이 되었을 경우 랜덤(임시)으로 배치한다.
	 * 5. 현재 차례의 캐릭터가 배치되었으면 상대방에게 차례를 넘긴다.
	 */

	void Awake()
	{
		_instance = this;
	}

	public void StartPlaceMode()
	{
		g = GameManager.Instance;
		u = GameUIManager.Instance;

		g.placeMode = true;
		g.timer = 40f;
		lastAim = new Vector2();

		// Temporary??? : 나중에 방식을 더 좋은 것으로 바꿀 필요가 있다!
		remainPlace = new List<Vector2>();
		for (int c = 1; c <= 9; ++c)
			for (int r = 1; r <= 9; ++r)
				remainPlace.Add(new Vector2(c, r));

		u.ShowPlaceMode(g.myTurn, order);
	}

	public void ContinuePlaceMode()
	{
		if (g.turnCount == 9)
		{
			EndPlaceMode();
			return;
		}

		g.timer = 40f;
		g.myTurn = !g.myTurn;
		order = ++g.turnCount / 2;
		u.ShowPlaceMode(g.myTurn, order);
	}

	public void UpdatePlaceMode()
	{
		if (g.myTurn)
		{
			bool touched = false;
			Vector3 touchPos = new Vector3();
#if UNITY_ANDROID && !UNITY_EDITOR
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
			{
				touched = true;
                touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
			}
#else
			if (Input.GetMouseButtonDown(0))
			{
				touched = true;
				touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			}
#endif
			if (touched)
			{
				Board b = Board.Instance;
				Vector3 boardPos = b.transform.InverseTransformPoint(touchPos);
				Vector2 aim = b.GetNearGrid(boardPos.x, boardPos.y);
				Vector2 nearPos = b.GetNearGridPosition(boardPos.x, boardPos.y);

				if (aim == lastAim)
				{
					CharacterDoll doll = g.GetDoll(true, order);
					doll.transform.localPosition = nearPos;
					doll.gameObject.SetActive(true);
					u.placeModeAim.rectTransform.localPosition = new Vector2(-9999f, -9999f);
					remainPlace.RemoveAt(aimIndex);
					lastAim = Vector2.zero;
					SoundManager.Instance.PlayFX(placeFx);
					ContinuePlaceMode();
				}
				else
				{
					bool canPlace = false;
					for (int i = 0; i < remainPlace.Count; ++i)
					{
						if (remainPlace[i] == aim)
						{
							canPlace = true;
							aimIndex = i;
                            break;
						}
					}
					if (canPlace)
					{
						Vector3 worldPos = b.transform.TransformPoint(nearPos);
						Vector2 scrnPos = Camera.main.WorldToScreenPoint(worldPos) - new Vector3(Screen.width * 0.5f, 0f);
						scrnPos.x /= u.resolutionScale.x;
						scrnPos.y /= u.resolutionScale.y;
						u.placeModeAim.rectTransform.localPosition = scrnPos;
						lastAim = aim;
					}
				}
			}

			if (g.timer <= 0f)
			{
				RandomPlace(true, order);
				ContinuePlaceMode();
			}
		}
		else
		{
			if (g.timer <= 39f)
			{
				RandomPlace(false, order);
				ContinuePlaceMode();
			}
		}
	}

	public void EndPlaceMode()
	{
		g.placeMode = false;
		g.timer = 40f;
		g.turnCount = 0;
		g.myTurn = !g.myTurn;
		u.HidePlaceMode();
		u.SetTurnArrow(g.myTurn);
	}

	private void RandomPlace(bool _isPlayer, int _order)
	{
		int rand = Random.Range(0, remainPlace.Count);
		Vector2 v = remainPlace[rand];
		CharacterDoll doll = g.GetDoll(_isPlayer, _order);
		doll.transform.localPosition = Board.Instance.GetGridPosition((int)v.x, (int)v.y);
		if (!_isPlayer)
			doll.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		doll.gameObject.SetActive(true);
		remainPlace.RemoveAt(rand);
		SoundManager.Instance.PlayFX(placeFx);
	}

	// Temporary!!!
	public void DebugPlace()
	{
		for (; g.turnCount < 9;)
		{
			RandomPlace(g.myTurn, order);
			ContinuePlaceMode();
		}
	}
}
