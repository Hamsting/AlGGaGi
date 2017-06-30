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
	private static readonly float ONE_GRID_FORCE	= 1280f;
	private static readonly float MAX_DISTANCE = Board.BOARD_WIDTH * 5f;
	private static readonly float PREVIEW_Z			= -2f;
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

	public bool attacking = false;

	private LineRenderer[] lineRenderer;
	private float powerScale = 0.0f;
	private Vector2 firstTouched;
	private float previewMaterialSpeed = 0.01f;



	/*
	 * DollController
	 * 
	 */

	void Awake()
	{
		_instance = this;
	}

	public void Initialize()
	{
		lineRenderer = this.GetComponentsInChildren<LineRenderer>();
		lineRenderer[PREVIEW_INCOMING].gameObject.SetActive(false);
		lineRenderer[PREVIEW_REFLECT].gameObject.SetActive(false);
		lineRenderer[PREVIEW_TARGETGO].gameObject.SetActive(false);
		lineRenderer[PREVIEW_CIRCLE].gameObject.SetActive(false);
		lineRenderer[PREVIEW_CIRCLE].useWorldSpace = false;
	}
	
	public void Tick()
	{
		if (GameManager.Instance.IsSelectingDoll())
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			if (Input.touchCount > 0)
			{
				Touch t = Input.GetTouch(0);
				if (t.phase == TouchPhase.Ended)
					ShotDoll();
				else
				{
					if (t.phase == TouchPhase.Began)
						firstTouched = Camera.main.ScreenToWorldPoint(t.position);

					Vector2 touchPos = Camera.main.ScreenToWorldPoint(t.position);
					float angle = Mathf.Atan2(touchPos.y - firstTouched.y, touchPos.x - firstTouched.x) * Mathf.Rad2Deg + 90.0f;
					float dis = Mathf.Clamp(Vector2.Distance(touchPos, firstTouched), 0f, MAX_DISTANCE);
					Quaternion q = Quaternion.Euler(0f, 0f, angle);
					GameManager.Instance.selectedDoll.transform.rotation = q;

					UpdatePreview(dis);
				}
			}
			else
				DisableAllPreview();
#else
			if (Input.GetMouseButton(0))
			{
				if (Input.GetMouseButtonDown(0))
					firstTouched = Camera.main.ScreenToWorldPoint(Input.mousePosition);

				Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				float angle = Mathf.Atan2(touchPos.y - firstTouched.y, touchPos.x - firstTouched.x) * Mathf.Rad2Deg + 90.0f;
				float dis = Mathf.Clamp(Vector2.Distance(touchPos, firstTouched), 0f, MAX_DISTANCE);
				Quaternion q = Quaternion.Euler(0f, 0f, angle);
				GameManager.Instance.selectedDoll.transform.rotation = q;

				UpdatePreview(dis);
			}
			else if (Input.GetMouseButtonUp(0))
				ShotDoll();
			else
				DisableAllPreview();
#endif
		}
		else
			DisableAllPreview();
	}

	private void UpdatePreview(float _touchDis)
	{
		CharacterDoll selectedDoll = GameManager.Instance.selectedDoll;

		float bw = Board.BOARD_WIDTH;
		float power = _touchDis * 2f;
		powerScale = power / bw;

		Transform trans = selectedDoll.transform;
		int layerMask = -1 - (1 << LayerMask.NameToLayer("Background")) - (1 << LayerMask.NameToLayer("SelectedDoll"));
		CircleCollider2D col = selectedDoll.GetComponent<CircleCollider2D>();
		RaycastHit2D hit = Physics2D.CircleCast(trans.position, col.radius, trans.up, power, layerMask);

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
		CharacterDoll selectedDoll = GameManager.Instance.selectedDoll;
		Rigidbody2D rb = selectedDoll.GetComponent<Rigidbody2D>();
		rb.AddForce(selectedDoll.transform.up * (powerScale * ONE_GRID_FORCE));
		attacking = false;
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
