using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_0006 : Skill
{
	public AudioClip fx;

	private CharacterDoll target;
	private Rigidbody2D rb;



	public override void Use()
	{
		base.Use();

		this.transform.position = owner.transform.position;

		rb = this.GetComponent<Rigidbody2D>();
		rb.AddForce(owner.transform.up * receivedForce);

		SoundManager.Instance.PlayFX(fx);
	}

	public override void Tick()
	{
		base.Tick();

		if (active && state == 0 && rb.velocity == Vector2.zero)
		{
			active = false;
			++state;
		}
	}

	public void OnTriggerEnter2D(Collider2D _col)
	{
		if (_col.gameObject == owner.gameObject)
			return;

		if (_col.gameObject.layer != LayerMask.NameToLayer("Doll"))
			return;

		CharacterDoll tar = _col.gameObject.GetComponent<CharacterDoll>();
		if (tar.faction.isPlayer == owner.faction.isPlayer)
			return;

		float damage = owner.chaInfo.attackPower * 0.75f;
		if (state > 0)
			damage = owner.chaInfo.attackPower * 2f;
		else
			tar.moveLock += 1;
        tar.TakeDamage((int)damage);

		tar.rb.velocity = Vector2.zero;
		End();
	}
}
