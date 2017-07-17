using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CorpsManager : MonoBehaviour
{
	private static readonly float BAR_HALFHEIGHT	= 25f;
	private static readonly float TOP_HALFHEIGHT	= 200f;
	private static readonly float BAR_LIMIT_Y		= -BAR_HALFHEIGHT + -TOP_HALFHEIGHT * 2f;
	private static CorpsManager _instance;
	public static CorpsManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("CorpsManager : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	public GameObject top;
	public GameObject bot;
	public GameObject bar;
	public Sprite barOpenedSpr;
	public Sprite barClosedSpr;
	public GameObject barArrowOpen;
	public GameObject barArrowClose;
	public GameObject chaSlotPrefab;

	private bool hold = false;
	private bool opened = false;
	private Animator botAnimator;
	private RectTransform topRt;
	private RectTransform barRt;
	private Coroutine barCoroutine;
	private float firstY;
	private Image barImage;


	void Awake()
	{
		_instance = this;
	}

	void Start()
	{
		botAnimator = bot.GetComponent<Animator>();
		topRt = top.GetComponent<RectTransform>();
		barRt = bar.GetComponent<RectTransform>();
		barImage = bar.GetComponent<Image>();
    }

	void Update()
	{
		if (hold)
		{
			// MapManager.Instance.focused = false;

#if UNITY_ANDROID && !UNITY_EDITOR
			float curY = Input.GetTouch(0).position.y;
#else
			float curY = Input.mousePosition.y;
#endif

			float diff = (curY - firstY) * 1280f / Screen.height;
			Vector2 pos = barRt.anchoredPosition;
			pos.y = Mathf.Clamp(diff - BAR_HALFHEIGHT, BAR_LIMIT_Y, -BAR_HALFHEIGHT);
			if (opened)
				pos.y = Mathf.Clamp(BAR_LIMIT_Y + (diff - BAR_HALFHEIGHT), BAR_LIMIT_Y, -BAR_HALFHEIGHT);

			if (!opened && pos.y <= BAR_LIMIT_Y)
			{
				Open();
            }
			else if (opened && pos.y >= -BAR_HALFHEIGHT)
			{
				Close();
			}

			barRt.anchoredPosition = pos;
			pos.y += BAR_HALFHEIGHT;
			topRt.anchoredPosition = pos;
		}
		if (opened)
		{
			// MapManager.Instance.focused = false;
		}
	}

	public void OnTouchDown()
	{
		hold = true;
		top.SetActive(true);

#if UNITY_ANDROID && !UNITY_EDITOR
		firstY = Input.GetTouch(0).position.y;
#else
		firstY = Input.mousePosition.y;
#endif
	}

	public void OnTouchUp()
	{
		if (!hold)
			return;

		if (opened)
			Close();
		else
			Open();
		hold = false;
	}

	private void Open()
	{
		if (barCoroutine != null)
			StopCoroutine(barCoroutine);
		barCoroutine = StartCoroutine(OpenTop());
		barImage.sprite = barOpenedSpr;
		barArrowOpen.SetActive(false);
		barArrowClose.SetActive(true);
	}

	private void Close()
	{
		if (barCoroutine != null)
			StopCoroutine(barCoroutine);
		barCoroutine = StartCoroutine(CloseTop());
		barImage.sprite = barClosedSpr;
		barArrowOpen.SetActive(true);
		barArrowClose.SetActive(false);
	}

	private IEnumerator OpenTop()
	{
		bool animate = true;
		hold = false;
		opened = true;
		top.SetActive(true);
		bot.SetActive(true);
		botAnimator.Play("Open");
		while (animate)
		{
			Vector2 pos = barRt.anchoredPosition;
			float y = pos.y + (BAR_LIMIT_Y - pos.y) * 0.1f;
			if (y <= BAR_LIMIT_Y + 1f)
			{
				y = BAR_LIMIT_Y;
				animate = false;
			}
			pos.y = y;
			barRt.anchoredPosition = pos;
			pos.y += BAR_HALFHEIGHT;
			topRt.anchoredPosition = pos;
			yield return null;
		}
		yield return null;
	}

	private IEnumerator CloseTop()
	{
		bool animate = true;
		hold = false;
		opened = false;
		botAnimator.Play("Close");
		while (animate)
		{
			Vector2 pos = barRt.anchoredPosition;
			float y = pos.y * 0.9f;
			if (y >= -BAR_HALFHEIGHT)
			{
				y = -BAR_HALFHEIGHT;
				animate = false;
			}
			pos.y = y;
			barRt.anchoredPosition = pos;
			pos.y += BAR_HALFHEIGHT;
			topRt.anchoredPosition = pos;
			yield return null;
		}
		if (!hold)
			top.SetActive(false);
		yield return null;
	}

	private void LoadCharacters()
	{
		int max = CharacterDB.Instance.characters.Count;
		for (int i = 1; i < max; ++i)
		{
			GameObject obj = Instantiate<GameObject>(chaSlotPrefab);
			CharacterSlot slot = obj.GetComponent<CharacterSlot>();

			Character c = CharacterDB.Instance.characters[i];
			slot.portrait.sprite = c.portrait;
			
		}
	}
}
