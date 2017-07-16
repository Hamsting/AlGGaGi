using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DollController : MonoBehaviour
{
	private static readonly int PREVIEW_INCOMING	= 0;
	private static readonly int PREVIEW_REFLECT		= 1;
	private static readonly int PREVIEW_TARGETGO	= 2;
	private static readonly int PREVIEW_CIRCLE		= 3;
	private static readonly float MAX_DISTANCE		= Board.BOARD_WIDTH * 5f;
	private static readonly float PREVIEW_Z			= -2f;
	// CancleControl
	// private static readonly Color32 CANCELBG_OFF = new Color32(255, 255, 255, 100);
	// private static readonly Color32 CANCELBG_ON = new Color32(255, 255, 255, 200);
	// private static readonly float PREVIEW_CANCEL_Y	= 370f;
	private static readonly float PREVIEW_CANCELDIS = 128f * 0.5f;
	private static DollController _instance;
	public static DollController Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("DollController : Instance is null.");
				return null;
			}
			return _instance;
		}
	}
	public static readonly float ONE_GRID_FORCE		= 1280f;
	public static readonly float ONE_GRID_VELOCITY	= 2.5f;

	public bool attacking = false;
	public int attackType = 0;

	private LineRenderer[] lineRenderer;
	private float powerScale = 0.0f;
	private Vector2 firstTouched;
	private Vector2 firstTouchedScrn;
	private bool touchStarted = false;
	private float previewMaterialSpeed = 0.01f;
	private bool cancelAct = false;
	private GameManager g;
	private GameUIManager u;



	/*
	 * DollController
	 * 1. 알이 선택되었으면 일반공격과 스킬을 고른다.
	 * 2. 골랐다면 attacking이 treu로 바뀌며 알을 조종할 수 있게 된다.
	 * 3. 알을 조종하였다면 attacking이 false로 바뀌고 턴을 상대에게 넘긴다.
	 * 4. 취소하려면 조종하는 상태에서 화면 가장 위 취소선 이내로 드래그해야한다.
	 * 5. 취소했을 경우 알의 선택을 해제한다.
	 */

	void Awake()
	{
		_instance = this;
	}

	public void Initialize()
	{
		g = GameManager.Instance;
		u = GameUIManager.Instance;

		lineRenderer = this.GetComponentsInChildren<LineRenderer>();
		lineRenderer[PREVIEW_INCOMING].gameObject.SetActive(false);
		lineRenderer[PREVIEW_REFLECT].gameObject.SetActive(false);
		lineRenderer[PREVIEW_TARGETGO].gameObject.SetActive(false);
		lineRenderer[PREVIEW_CIRCLE].gameObject.SetActive(false);
		lineRenderer[PREVIEW_CIRCLE].useWorldSpace = false;
	}
	
	public void Tick()
	{
		if (g.IsSelectingDoll() && !g.gameStopped && attackType != 0)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			if (Input.touchCount > 0)
			{
				Touch t = Input.GetTouch(0);
				if (touchStarted && t.phase == TouchPhase.Ended)
					ShotDoll();
				else
				{
					if (t.phase == TouchPhase.Began)
					{
						// CancleControl
						// u.OpenCancelControl();
						firstTouchedScrn = t.position;
						firstTouched = Camera.main.ScreenToWorldPoint(firstTouchedScrn);
						u.SetDollControlAimPos(firstTouchedScrn);
						u.SetDollControlAimActive(true);
						touchStarted = true;
						cancelAct = false;
					}
					if (touchStarted)
					{
						Vector2 touchPos = Camera.main.ScreenToWorldPoint(t.position);
						float angle = Mathf.Atan2(touchPos.y - firstTouched.y, touchPos.x - firstTouched.x) * Mathf.Rad2Deg + 90.0f;
						float dis = Mathf.Clamp(Vector2.Distance(touchPos, firstTouched), 0f, MAX_DISTANCE);
						Quaternion q = Quaternion.Euler(0f, 0f, angle);
						g.selectedDoll.transform.rotation = q;

						UpdatePreview(dis);
					}
				}
			}
			else
				DisableAllPreview();
#else
			if (Input.GetMouseButton(0))
			{
				if (Input.GetMouseButtonDown(0))
				{
					// CancleControl
					// u.OpenCancelControl();
					firstTouchedScrn = Input.mousePosition;
					firstTouched = Camera.main.ScreenToWorldPoint(firstTouchedScrn);
					u.SetDollControlAimPos(firstTouchedScrn);
					u.SetDollControlAimActive(true);
					touchStarted = true;
					cancelAct = false;
				}
				if (touchStarted)
				{
					Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					float angle = Mathf.Atan2(touchPos.y - firstTouched.y, touchPos.x - firstTouched.x) * Mathf.Rad2Deg + 90.0f;
					float dis = Mathf.Clamp(Vector2.Distance(touchPos, firstTouched), 0f, MAX_DISTANCE);
					Quaternion q = Quaternion.Euler(0f, 0f, angle);
					g.selectedDoll.transform.rotation = q;

					UpdatePreview(dis);
				}
			}
			else if (touchStarted && Input.GetMouseButtonUp(0))
				ShotDoll();
			else
				DisableAllPreview();
#endif
		}
		else
			DisableAllPreview();
		
		if (attacking)
		{
			Vector2 touchPos = Vector2.zero;
#if UNITY_ANDROID && !UNITY_EDITOR
			touchPos = Input.GetTouch(0).position;
#else
			touchPos = Input.mousePosition;
#endif
			float dis = Vector2.Distance(firstTouchedScrn, touchPos);
			if (dis <= PREVIEW_CANCELDIS * u.resolutionScale.y)
			{
				// CancleControl
				// u.cancelControlBG.color = CANCELBG_ON;
				cancelAct = true;
			}
			else
			{
				// CancleControl
				// u.cancelControlBG.color = CANCELBG_OFF;
				cancelAct = false;
			}
		}
	}

	private void UpdatePreview(float _touchDis)
	{
		CharacterDoll selectedDoll = g.selectedDoll;
		
		if (cancelAct)
		{
			lineRenderer[PREVIEW_INCOMING].gameObject.SetActive(false);
			lineRenderer[PREVIEW_REFLECT].gameObject.SetActive(false);
			lineRenderer[PREVIEW_TARGETGO].gameObject.SetActive(false);
			lineRenderer[PREVIEW_CIRCLE].gameObject.SetActive(false);
			return;
        }

		float bw = Board.BOARD_WIDTH;
		float power = _touchDis * 2f;
		powerScale = power / bw;

		Transform trans = selectedDoll.transform;
		int layerMask = -1 - (1 << LayerMask.NameToLayer("Background")) - (1 << LayerMask.NameToLayer("SelectedDoll"));
		CircleCollider2D col = selectedDoll.GetComponent<CircleCollider2D>();
		RaycastHit2D hit = Physics2D.CircleCast(trans.position, col.radius - 0.01f, trans.up, power, layerMask);

		if (hit.collider != null)
		{
			Vector3 hpos = hit.point;
			Vector3 spos = selectedDoll.transform.position;
			hpos.z = PREVIEW_Z;
			spos.z = PREVIEW_Z;

			Vector3 i = hpos - spos;                // Incoming Vector.
			Vector3 n = hit.normal;                 // Hit Vector Normalize.
			Vector3 tc = hit.normal * col.radius;   // To Center Vector.
			Vector3 c = hpos + tc;                  // Center Position.
			Vector3[] vecs1 = { spos, c };

			if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Doll"))
			{
				Vector3 tar = hit.collider.gameObject.transform.TransformPoint(hit.collider.offset);
				Vector3 res = tar + -n * 1f;
				res.z = PREVIEW_Z;

				Vector3[] vecs2 = { tar, res };
				lineRenderer[PREVIEW_INCOMING].SetPositions(vecs1);
				lineRenderer[PREVIEW_TARGETGO].SetPositions(vecs2);
				lineRenderer[PREVIEW_INCOMING].gameObject.SetActive(true);
				lineRenderer[PREVIEW_TARGETGO].gameObject.SetActive(true);
				lineRenderer[PREVIEW_REFLECT].gameObject.SetActive(false);
			}
			else
			{
				Vector3 r = Vector3.Reflect(-i, n).normalized;
				Vector3 res = c + -r * 1f;
				res.z = PREVIEW_Z;

				Vector3[] vecs2 = { c, res };
				lineRenderer[PREVIEW_INCOMING].SetPositions(vecs1);
				lineRenderer[PREVIEW_REFLECT].SetPositions(vecs2);
				lineRenderer[PREVIEW_INCOMING].gameObject.SetActive(true);
				lineRenderer[PREVIEW_REFLECT].gameObject.SetActive(true);
				lineRenderer[PREVIEW_TARGETGO].gameObject.SetActive(false);
			}

			DrawPreviewCircle(c, col.radius);
		}
		else
		{
			Vector3 spos = selectedDoll.transform.position;
			Vector3 dpos = spos + selectedDoll.transform.up * power;
			spos.z = PREVIEW_Z;
			dpos.z = PREVIEW_Z;

			Vector3[] vecs = { spos, dpos };
			lineRenderer[PREVIEW_INCOMING].SetPositions(vecs);
			lineRenderer[PREVIEW_INCOMING].gameObject.SetActive(true);
			lineRenderer[PREVIEW_REFLECT].gameObject.SetActive(false);
			lineRenderer[PREVIEW_TARGETGO].gameObject.SetActive(false);

			DrawPreviewCircle(dpos, col.radius);
		}
		Vector2 offset = lineRenderer[PREVIEW_INCOMING].material.mainTextureOffset;
		offset.x -= (64f * Time.deltaTime * previewMaterialSpeed) % 64f;
		lineRenderer[PREVIEW_INCOMING].material.mainTextureOffset = offset;
		lineRenderer[PREVIEW_REFLECT].material.mainTextureOffset = offset;
		attacking = true;
    }

	private void DisableAllPreview()
	{
		lineRenderer[PREVIEW_INCOMING].gameObject.SetActive(false);
		lineRenderer[PREVIEW_REFLECT].gameObject.SetActive(false);
		lineRenderer[PREVIEW_TARGETGO].gameObject.SetActive(false);
		lineRenderer[PREVIEW_CIRCLE].gameObject.SetActive(false);
		attacking = false;
    }

	private void ShotDoll()
	{
		touchStarted = false;
		attacking = false;
		// CancleControl
		// u.CloseCancelControl();
		u.SetDollControlAimActive(false);
		if (cancelAct)
		{
			g.DeselectDoll();
			return;
		}
		CharacterDoll selectedDoll = g.selectedDoll;
		Rigidbody2D rb = selectedDoll.GetComponent<Rigidbody2D>();
		rb.AddForce(selectedDoll.transform.up * (powerScale * ONE_GRID_FORCE));
		g.attacker = selectedDoll;
		g.NextTurn();
	}

	private void DrawPreviewCircle(Vector2 _pos, float _radius)
	{
		float x = 0f;
		float y = 0f;
		float z = PREVIEW_Z;
		float a = 0f;
		int segments = 20;
		float rad = _radius;
		lineRenderer[PREVIEW_CIRCLE].positionCount = segments + 1;
		for (int j = 0; j < (segments + 1); j++)
		{
			x = Mathf.Sin(Mathf.Deg2Rad * a) * rad;
			y = Mathf.Cos(Mathf.Deg2Rad * a) * rad;
			lineRenderer[PREVIEW_CIRCLE].SetPosition(j, new Vector3(x, y, z));
			a += (360f / segments);
		}
		lineRenderer[PREVIEW_CIRCLE].gameObject.SetActive(true);
		lineRenderer[PREVIEW_CIRCLE].gameObject.transform.position = _pos;
	}
}
