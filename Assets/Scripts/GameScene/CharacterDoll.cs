﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDoll : MonoBehaviour
{
	public Vector2 center;
	public Faction faction;
	public Character chaInfo;

	private Rigidbody2D rb;
	private CircleCollider2D col;
	private int curHp = 100;
	private int maxHp = 100;
	private int attackPower = 30;
	private float pushPower = 5.0f;
	private float defensePower = 3.0f;



	public void Initialize(Character _chaInfo)
	{
		chaInfo = _chaInfo;
		Initialize();
	}

	public void Initialize()
	{
		rb = this.GetComponent<Rigidbody2D>();
		if (rb == null)
		{
			Debug.LogError(this.ToString() + " : Rigidbody2D is null.");
			return;
		}

		col = this.GetComponent<CircleCollider2D>();
		if (col == null)
		{
			Debug.LogError(this.ToString() + " : CircleCollider2D is null.");
			return;
		}
		center = this.transform.TransformPoint(col.offset);

		if (chaInfo == null)
		{
			Debug.LogError(this.ToString() + " : CharacterInfo is null.");
			return;
		}
		maxHp = chaInfo.hp;
		curHp = maxHp;
		attackPower = chaInfo.attackPower;
		pushPower = chaInfo.pushPower;
		defensePower = chaInfo.defensePower;
	}

	public void Tick()
	{
		center = this.transform.TransformPoint(col.offset);
	}

	public void OnMouseUp()
	{
		GameManager g = GameManager.Instance;
		if (g.myTurn && !g.placeMode && !g.gameStopped && 
			faction.isPlayer && !DollController.Instance.attacking)
		{
			if (g.IsSelectingDoll())
				g.DeselectDoll();
			g.SelectDoll(this);
		}
	}

	public void OnCollisionEnter2D(Collision2D col)
	{
		Vector3 posStart = this.transform.position;
		Vector3 n = rb.velocity.normalized;
		Vector3 posEnd = posStart + n;
		this.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(posEnd.x - posStart.x, posEnd.y - posStart.y) * Mathf.Rad2Deg);

		if (col.gameObject.layer == LayerMask.NameToLayer("Doll") &&
			GameManager.Instance.attacker == this)
		{
			Vector2 minVelocity = col.contacts[0].point.normalized * DollController.ONE_GRID_FORCE * 2f;
			col.rigidbody.AddForce(minVelocity);
		}
	}

	public bool IsStopped()
	{
		if (rb.velocity == Vector2.zero)
			return true;
		return false;
	}
}
