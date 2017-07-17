using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
	private static MapManager _instance;
	public static MapManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("MapManager : Instance is null.");
				return null;
			}
			return _instance;
		}
	}
	public static readonly int MAX_DIVIDE = 5;

	public SpriteRenderer worldSpr;
	public List<Divide> divides;
	public GameObject divideBtnGroup;
	public GameObject stageBtnGroup;
	public GameObject stageSelectedGroup;
	public List<RectTransform> stageBtns;
	public LineRenderer stageLine;
	public AnimationCurve camCurve;
	public RectTransform selectedEffect;
	/// <summary>
	/// 0 : Locked, 1 : Cleared, 2 ~ : Normal
	/// </summary>
	public Sprite[] stageBtnSpr;
	public Sprite[] bossStageBtnSpr;

	private Divide selectedDivide;
	private int selectedStage = 0;
	private Camera cam;
	private Coroutine coroutine;



	void Awake()
	{
		_instance = this;
	}

	public void Initialize()
	{
		cam = Camera.main;
	}

	public void Tick()
	{

	}

	public void OnSelectDivide(int _num)
	{
		selectedDivide = divides[_num - 1];
		if (coroutine != null)
			StopCoroutine(coroutine);
		coroutine = StartCoroutine(OpenDivideAnimate(_num));
    }

	private IEnumerator OpenDivideAnimate(int _num)
	{
		Divide d = selectedDivide;
		int cleared = PlayerData.Instance.clearedStage[_num - 1];
		float timer = 0f;
		float destTime = 0.4f;

		d.gameObject.SetActive(true);
		divideBtnGroup.SetActive(false);
		for (int i = 0; i < 6; ++i)
		{
			stageBtns[i].anchoredPosition = d.stagePos[i];
			Image img = stageBtns[i].gameObject.GetComponent<Image>();
			if (i == 5)
			{
				if (cleared == 6)
					img.sprite = bossStageBtnSpr[1];
				else if (cleared == 5)
					img.sprite = bossStageBtnSpr[2];
				else
					img.sprite = bossStageBtnSpr[0];
			}
			else
			{
				if (i < cleared)
					img.sprite = stageBtnSpr[1];
				else if (i == cleared)
					img.sprite = stageBtnSpr[2 + i];
				else
					img.sprite = stageBtnSpr[0];
			}
		}

		while (true)
		{
			worldSpr.color = Color.Lerp(Color.white, Color.gray, timer / destTime);
			if (timer >= destTime)
			{
				worldSpr.color = Color.gray;
				timer -= destTime;
                break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		destTime = 0.75f;
		Vector2 originPos = cam.transform.position;
        while (true)
		{
			float t = camCurve.Evaluate(timer / destTime);
			Vector3 pos = Vector2.Lerp(originPos, d.camCenter, t);
			pos.z = -10f;
            cam.transform.position = pos;
			cam.orthographicSize = Mathf.Lerp(5f, d.camSize, t);
			if (timer >= destTime)
			{
				pos = d.camCenter;
				pos.z = -10f;
                cam.transform.position = pos;
				cam.orthographicSize = d.camSize;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		stageBtnGroup.SetActive(true);
		stageSelectedGroup.SetActive(true);
		Vector3[] linePos = new Vector3[6];
		for (int i = 0; i < 6; ++i)
			linePos[i] = d.stagePos[i];
		stageLine.SetPositions(linePos);
		stageLine.gameObject.SetActive(true);
		OnSelectStage(1);
}

	private IEnumerator CloseDivideAnimate()
	{
        float timer = 0f;
		float destTime = 0.75f;


		stageBtnGroup.SetActive(false);
		stageSelectedGroup.SetActive(false);
		stageLine.gameObject.SetActive(false);

		Vector2 originPos = cam.transform.position;
		while (true)
		{
			float t = camCurve.Evaluate(timer / destTime);
			Vector3 pos = Vector2.Lerp(originPos, Vector2.zero, t);
			pos.z = -10f;
			cam.transform.position = pos;
			cam.orthographicSize = Mathf.Lerp(selectedDivide.camSize, 5f, t);
			worldSpr.color = Color.Lerp(Color.gray, Color.white, timer / destTime);
			if (timer >= destTime)
			{
				pos = Vector2.zero;
				pos.z = -10f;
				cam.transform.position = pos;
				cam.orthographicSize = 5f;
				worldSpr.color = Color.white;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		selectedDivide.gameObject.SetActive(false);
		divideBtnGroup.SetActive(true);
		selectedDivide = null;
	}

	public void OnGoWorld()
	{
		if (coroutine != null)
			StopCoroutine(coroutine);
		coroutine = StartCoroutine(CloseDivideAnimate());
	}

	public void OnStartGame()
	{

	}

	public void OnSelectStage(int _num)
	{
		selectedEffect.anchoredPosition = selectedDivide.stagePos[_num - 1];
		if (_num == 6)
		{
			selectedEffect.anchoredPosition = selectedDivide.stagePos[_num - 1] + new Vector2(0f, 10f);
            selectedEffect.localScale = new Vector3(1.6f, 1.6f, 1f);
		}
		else
			selectedEffect.localScale = new Vector3(1f, 1f, 1f);
	}
}










/*
public bool cameraMoving = false;
public bool cameraZooming = false;
public Transform boundLT;
public Transform boundRB;
public bool focused = false;

private Vector3 firstTouchPos;
private Vector2 firstTouchScrnPos;
private Vector2 firstTouchTwoScrnPos;
private Camera cam;
private float camSize = 5f;
private float firstCamSize = 5f;
private float camRatio = 9f / 16f;

		void Start()
		{
			cam = Camera.main;
			camRatio = (float)Screen.width / Screen.height;
		}

		void Update()
		{
	#if UNITY_ANDROID && !UNITY_EDITOR
			if (Input.touchCount > 0)
			{
				Touch t = Input.GetTouch(0);
				Vector3 touchPos = cam.ScreenToWorldPoint(t.position);
				if (t.phase == TouchPhase.Began)
				{
					firstTouchPos = touchPos;
					firstTouchScrnPos = t.position;
					cameraMoving = true;
					focused = true;
				}
				if (Input.touchCount > 1)
				{
					Touch t2 = Input.GetTouch(1);
					if (t2.phase == TouchPhase.Began)
					{
						firstTouchTwoScrnPos = t2.position;
						firstCamSize = camSize;
						cameraZooming = true;
					}
					float firstDis = Vector2.Distance(firstTouchScrnPos, firstTouchTwoScrnPos);
					float curDis = Vector2.Distance(t.position, t2.position);
					float ratio = firstDis / curDis;
					camSize = Mathf.Clamp(firstCamSize * ratio, 2f, 10f);
					cam.orthographicSize = camSize;
					LimitCameraPosition();
				}
				else
					MoveCamera(touchPos);
			}
			else
			{
				cameraMoving = false;
				cameraZooming = false;
				focused = false;
			}
	#else
			if (Input.GetMouseButton(0))
			{
				Vector3 touchPos = cam.ScreenToWorldPoint(Input.mousePosition);
				if (Input.GetMouseButtonDown(0))
				{
					firstTouchPos = touchPos;
					cameraMoving = true;
					focused = true;
				}
				MoveCamera(touchPos);
			}
			else
			{
				cameraMoving = false;
				focused = false;
			}

			float wheel = Input.GetAxis("Mouse ScrollWheel");
			if (wheel != 0f)
			{
				float fix = (wheel > 0f) ? -0.5f : 0.5f;
				camSize = Mathf.Clamp(camSize + fix, 2f, 10f);
				cam.orthographicSize = camSize;
				LimitCameraPosition();
				cameraZooming = true;
			}
			else
				cameraZooming = false;
	#endif
	
}

private void MoveCamera(Vector3 _touchPos)
	{
		if (cameraZooming || !focused)
			return;

		if (Vector3.Distance(_touchPos, firstTouchPos) < 0.15f)
			return;

		Vector3 camPos = cam.transform.position;
		camPos = camPos - (_touchPos - firstTouchPos);
		camPos.x = Mathf.Clamp(camPos.x, boundLT.transform.position.x + (camSize * camRatio), boundRB.transform.position.x - (camSize * camRatio));
		camPos.y = Mathf.Clamp(camPos.y, boundRB.transform.position.y + camSize, boundLT.transform.position.y - camSize);
		camPos.z = -10f;
		cam.transform.position = camPos;
	}

	private void LimitCameraPosition()
	{
		Vector3 camPos = cam.transform.position;
		camPos.x = Mathf.Clamp(camPos.x, boundLT.transform.position.x + (camSize * camRatio), boundRB.transform.position.x - (camSize * camRatio));
		camPos.y = Mathf.Clamp(camPos.y, boundRB.transform.position.y + camSize, boundLT.transform.position.y - camSize);
		camPos.z = -10f;
		cam.transform.position = camPos;
	}
	*/
