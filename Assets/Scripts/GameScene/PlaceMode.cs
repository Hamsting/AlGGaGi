﻿using System.Collections;
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

	private GameManager g;
	private UIManager u;
	private int order = 0;
	private Vector2 lastAim;



	void Awake()
	{
		_instance = this;
	}

	/*
	 * PlaceMode
	 * 1. myTurn이 true일 경우 현재 차례(order)의 캐릭터를 가져온다.
	 * 2. UI에서 현재 차례의 캐릭터를 보여주고(스케일, 화살표 등) 타이머를 맞춘다.
	 * 3. 보드의 격자를 선택하면 표식을 두고 다시 클릭하거나 '선택'버튼을 누르면 배치된다.
	 * 4. 타이머가 0이 되었을 경우 랜덤(임시)으로 배치한다.
	 * 5. 현재 차례의 캐릭터가 배치되었으면 상대방에게 차례를 넘긴다.
	 */
	public void StartPlaceMode()
	{
		g = GameManager.Instance;
		u = UIManager.Instance;

		g.placeMode = true;
		g.timer = 40f;
		lastAim = new Vector2();
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
				Vector2 nearPos = b.GetNearGridPosition(boardPos.x, boardPos.y);
				Vector2 aim = b.GetNearGrid(boardPos.x, boardPos.y);

				if (aim == lastAim)
				{
					CharacterDoll doll = g.GetDoll(true, order);
					doll.transform.localPosition = nearPos;
					doll.gameObject.SetActive(true);
					u.placeModeAim.rectTransform.localPosition = new Vector2(-9999f, -9999f);
					ContinuePlaceMode();
				}
				else
				{
					Vector3 worldPos = b.transform.TransformPoint(nearPos);
					Vector2 scrnPos = Camera.main.WorldToScreenPoint(worldPos) - new Vector3(Screen.width * 0.5f, 0f);
					u.placeModeAim.rectTransform.localPosition = scrnPos;
					lastAim = aim;
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
	}

	private void RandomPlace(bool _isPlayer, int _order)
	{
		int gx = Random.Range(1, 10);
		int gy = Random.Range(1, 10);
		CharacterDoll doll = g.GetDoll(_isPlayer, _order);
		doll.transform.localPosition = Board.Instance.GetGridPosition(gx, gy);
		doll.gameObject.SetActive(true);
	}
}
