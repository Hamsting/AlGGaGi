using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDoll : MonoBehaviour
{
	public Vector2 center;
	public Faction faction;
	public Character chaInfo;
	public Portrait portrait;
	public bool dead = false;

	private Rigidbody2D rb;
	private CircleCollider2D col;
	private int curHp = 100;			// 현재 체력
	private int maxHp = 100;			// 최대 체력
	private float attackPower = 30.0f;	// 공격력
	private float pushPower = 5.0f;		// 파워
	private float defensePower = 3.0f;  // 맷집



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
		GameManager g = GameManager.Instance;

		Vector3 posStart = this.transform.position;
		Vector3 n = rb.velocity.normalized;
		Vector3 posEnd = posStart + n;
		this.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(posEnd.x - posStart.x, posEnd.y - posStart.y) * Mathf.Rad2Deg);
		
		if (col.gameObject.layer == LayerMask.NameToLayer("Doll") &&
			g.attacker == this)
		{
			CharacterDoll hit = col.gameObject.GetComponent<CharacterDoll>();
			if (hit.faction.isPlayer != this.faction.isPlayer)
			{
				col.rigidbody.velocity = CalculateVelocity(this, hit);
				hit.TakeDamage(this);
				g.victim = hit;
				g.attacker = null;
			}
		}
	}

	public bool IsStopped()
	{
		if (rb.velocity == Vector2.zero)
			return true;
		return false;
	}

	private Vector2 FixVelocityToMinimum(Vector2 _vel)
	{
		if (_vel.magnitude < DollController.ONE_GRID_VELOCITY)
		{
			Vector2 v = _vel.normalized * DollController.ONE_GRID_VELOCITY * 1.5f;
			return v;
		}
		return _vel;
	}

	private Vector2 CalculateVelocity(CharacterDoll _atk, CharacterDoll _hit)
	{
		float push = Mathf.Clamp(_atk.pushPower - _hit.defensePower, 1.5f, 10f);
		Vector2 hitVel = _hit.rb.velocity;
		Vector2 v = hitVel.normalized * DollController.ONE_GRID_VELOCITY * push;
		return v;
	}

	private int TakeDamage(CharacterDoll _atk)
	{
		int damage = (int)(_atk.attackPower - this.defensePower);
		curHp -= damage;

		if (curHp <= 0)
		{
			Die();
			return damage;
		}

		portrait.UpdateHpBar((float)curHp / maxHp);
		return damage;
	}

	public void TakeDamage(int _fixedDamage)
	{
		curHp -= _fixedDamage;

		if (curHp <= 0)
		{
			Die();
			return;
		}

		portrait.UpdateHpBar((float)curHp / maxHp);
	}

	private void Die()
	{
		portrait.SetDeadPortrait();
		portrait.UpdateHpBar(0f);
		dead = true;
        this.gameObject.SetActive(false);
	}
}
