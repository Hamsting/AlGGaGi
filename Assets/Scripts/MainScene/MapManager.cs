using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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



	void Awake()
	{
		_instance = this;
	}

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
}
