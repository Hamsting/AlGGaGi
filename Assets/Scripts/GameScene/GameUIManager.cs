using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
	private static GameUIManager _instance;
	public static GameUIManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("GameUIManager : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	[HideInInspector]
	public Vector2 resolutionScale = Vector2.one;

	public Portrait[] playerPortrait;
	public Portrait[] enemyPortrait;
	public Image dollControlAim;
	public Image placeModeArrow;
	public Image placeModeAim;
	public Bar[] timerBar;
	public TextMesh timer;
	public ActSelect actSelect;
	public Animator gameMessage;
	public Animator resultWindow;
	public TextMesh resultGold;
	public RectTransform turnArrow;

	/*
	 * CancelControl
	public Animator cancelControl;
	public Image cancelControlBG;
	[HideInInspector]
	public bool cancelControlOpened = false;
	*/

	private RectTransform rtActSelect;



	void Awake()
	{
		_instance = this;
	}

	public void Initialize()
	{
		timerBar[0].Initialize();	
		timerBar[1].Initialize();
		actSelect.Initialize();
		rtActSelect = actSelect.GetComponent<RectTransform>();
		resolutionScale = new Vector2(Screen.width / 1080f, Screen.height / 1920f);
    }

	public void Tick()
	{
		float currentTime = GameManager.Instance.timer;
        timer.text = ((int)currentTime).ToString();
		if (GameManager.Instance.myTurn)
		{
			timerBar[0].Tick(currentTime / 40.0f);
			timerBar[1].Tick(1f);
		}
		else
		{
			timerBar[0].Tick(1f);
			timerBar[1].Tick(currentTime / 40.0f);
		}
	}

	public void ShowPlaceMode(bool _isPlayer, int _order)
	{
		placeModeArrow.gameObject.SetActive(true);
		placeModeAim.gameObject.SetActive(true);

		RectTransform rt = playerPortrait[_order].rectTransform;
		placeModeArrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		if (!_isPlayer)
		{
			rt = enemyPortrait[_order].rectTransform;
			placeModeArrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
			placeModeAim.gameObject.SetActive(false);
		}
		else
		{
			placeModeAim.sprite = playerPortrait[_order].doll.chaInfo.ingame;
			placeModeAim.rectTransform.pivot = placeModeAim.sprite.pivot / 500f;
        }
		Vector2 arrowPos = rt.anchoredPosition + new Vector2(0f, 400f);
		if (!_isPlayer)
			arrowPos.y = 1920f - arrowPos.y;
		placeModeArrow.rectTransform.localPosition = arrowPos;
		SetTurnArrow(_isPlayer);
	}

	public void HidePlaceMode()
	{
		placeModeArrow.gameObject.SetActive(false);
		placeModeAim.gameObject.SetActive(false);
	}

	public void ShowCharacters()
	{
		for (int i = 0; i < 5; ++i)
		{
			CharacterDoll doll = GameManager.Instance.playerDoll[i];
			Portrait p = playerPortrait[i];
            p.image.sprite = doll.chaInfo.portrait;
			p.hpBar.Initialize();
			p.doll = doll;
			doll.portrait = p;
        }
		for (int i = 0; i < 5; ++i)
		{
			CharacterDoll doll = GameManager.Instance.enemyDoll[i];
			Portrait p = enemyPortrait[i];
			p.image.sprite = doll.chaInfo.portrait;
			p.hpBar.Initialize();
			p.doll = doll;
			doll.portrait = p;
		}
	}

	public void OpenActSelect(CharacterDoll _doll)
	{
		Vector2 scrnPos = Camera.main.WorldToScreenPoint(_doll.transform.position);
		scrnPos = ToCanvasPosition(scrnPos);
		float margin = ActSelect.MARGIN_X + 100f;
		scrnPos.x = Mathf.Clamp(scrnPos.x, margin, 1080f - margin);
		rtActSelect.anchoredPosition = scrnPos;
		actSelect.gameObject.SetActive(true);
		actSelect.Open();
	}

	public void CloseActSelect()
	{
		actSelect.Close();
	}

	public void SetDollControlAimPos(Vector2 _scrnPos)
	{
		Vector2 canvasPos = ToCanvasPosition(_scrnPos);
        dollControlAim.rectTransform.anchoredPosition = canvasPos;
	}

	public void SetDollControlAimActive(bool _active)
	{
		dollControlAim.gameObject.SetActive(_active);
	}

	public Vector2 ToCanvasPosition(Vector2 _scrnPos)
	{
		Vector2 canvasPos = _scrnPos;
		canvasPos.x /= resolutionScale.x;
		canvasPos.y /= resolutionScale.y;
		return canvasPos;
	}

	public void ShowMyTurnMessage()
	{
		gameMessage.Play("MyTurn");
    }

	public void ShowResult(bool _win, int _gold)
	{
		StartCoroutine(AnimateResult(_win));
		resultGold.text = _gold.ToString();
    }

	private IEnumerator AnimateResult(bool _win)
	{
		if (_win)
			gameMessage.Play("Win");
		else
			gameMessage.Play("Lose");
		yield return new WaitForSeconds(1.0f);

		resultWindow.gameObject.SetActive(true);
		resultWindow.Play("Open");
	}

	public void SetTurnArrow(bool _myTurn)
	{
		if (_myTurn)
			turnArrow.localRotation = Quaternion.Euler(0f, 0f, 0f);
		else
			turnArrow.localRotation = Quaternion.Euler(0f, 0f, 180f);
	}

	/*
	 * CancelControl
	public void OpenCancelControl()
	{
		cancelControl.Play("Open");
		cancelControlOpened = true;
    }

	public void CloseCancelControl()
	{
		cancelControl.Play("Close");
		cancelControlOpened = false;
    }
	*/
}
