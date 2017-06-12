using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private static UIManager _instance;
	public static UIManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("UIManager : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	public Image[] playerPortrait;
	public Image[] enemyPortrait;
	public Image placeModeArrow;
	public Image placeModeAim;
	public Bar[] timerBar;
	public Text timer;
	


	void Awake()
	{
		_instance = this;
	}

	public void Initialize()
	{
		timerBar[0].Initialize();	
		timerBar[1].Initialize();
	}

	public void Tick()
	{
		float currentTime = GameManager.Instance.timer;
        timer.text = ((int)currentTime).ToString();
		if (GameManager.Instance.myTurn)
			timerBar[0].Tick(currentTime / 40.0f);
		else
			timerBar[1].Tick(currentTime / 40.0f);
	}

	public void ShowPlaceMode()
	{
		placeModeArrow.gameObject.SetActive(true);
		placeModeAim.gameObject.SetActive(true);
	}

	public void HidePlaceMode()
	{
		placeModeArrow.gameObject.SetActive(false);
		placeModeAim.gameObject.SetActive(false);
	}

	public void ShowCharacter()
	{
		for (int i = 0; i < 5; ++i)
		{
			Character cha = GameManager.Instance.playerDoll[i].chaInfo;
			playerPortrait[i].sprite = cha.portrait;
		}
	}
}
