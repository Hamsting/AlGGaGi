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
	public int receivedDamage = 0;

	private Rigidbody2D rb;
	private CircleCollider2D col;
	private int curHp = 100;			// 현재 체력
	private int maxHp = 100;			// 최대 체력
	private float attackPower = 30.0f;	// 공격력
	private float pushPower = 5.0f;		// 파워
	private float defensePower = 3.0f;  // 맷집
	private SpriteRenderer spr;



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

		spr = this.GetComponentInChildren<SpriteRenderer>();
		if (spr == null)
		{
			Debug.LogError(this.ToString() + " : SpriteRenderer is null.");
			return;
		}

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
				hit.receivedDamage = hit.TakeDamage(this);
				g.victim = hit;
				g.attacker = null;
			}
		}
		else if (col.gameObject.layer == LayerMask.NameToLayer("Wall") && receivedDamage > 0)
		{
			int wallDmg = (int)(receivedDamage * 0.2f);
			if (wallDmg < 1)
				wallDmg = 1;
            TakeDamage(wallDmg);
			receivedDamage = 0;
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
        if (damage < 1)
			damage = 1;
		curHp -= damage;
		float power = damage / (maxHp * 0.5f) * 0.25f;
		GameUIManager.Instance.ShakeCamera(power);
		if (!this.faction.isPlayer)
			GameManager.Instance.gainGold += damage;
		GameUIManager.Instance.ShowDamage(damage, Camera.main.WorldToScreenPoint(this.transform.position));

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
		if (!this.faction.isPlayer)
			GameManager.Instance.gainGold += _fixedDamage;
		GameUIManager.Instance.ShowCriticalDamage(_fixedDamage, Camera.main.WorldToScreenPoint(this.transform.position));

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

	public void MakeDarkSprite()
	{
		spr.color = new Color(0.8f, 0.3f, 0.3f, 1f);
	}

	public void MakeRedSprite()
	{
		spr.color = new Color(1f, 0.65f, 0.65f, 1f);
	}
}
