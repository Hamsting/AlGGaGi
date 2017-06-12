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

	}

	public void OnMouseDown()
	{
		GameManager.Instance.SelectDoll(this);
	}
}
