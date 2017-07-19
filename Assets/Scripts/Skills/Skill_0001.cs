using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_0001 : Skill
{
	public Animator slash;
	public SpriteRenderer shadow;
	public AnimationCurve curve;
	public AudioClip fx;



	public override void Use()
	{
		base.Use();

		this.transform.position = owner.transform.position;
		owner.rb.AddForce(owner.transform.up * receivedForce);
		owner.col.isTrigger = true;
	}

	public override void Tick()
	{
		base.Tick();

		if (state == 0)
		{
			this.transform.position = owner.transform.position;
			if (owner.rb.velocity == Vector2.zero)
				End();
		}
		else
		{
			float t = Mathf.Clamp01(timer / 0.75f);
			Color c = new Color(1f, 1f, 1f, curve.Evaluate(t));
			shadow.color = c;

			if (timer >= 1.5f)
				End();
		}
	}

	public void OnTriggerEnter2D(Collider2D _col)
	{
		if (state != 0)
			return;

		if (_col.gameObject == owner.gameObject)
			return;

		if (_col.gameObject.layer != LayerMask.NameToLayer("Doll"))
			return;

		CharacterDoll tar = _col.gameObject.GetComponent<CharacterDoll>();
		if (tar.faction.isPlayer == owner.faction.isPlayer)
			return;

		shadow.transform.position = owner.transform.position;
		slash.transform.position = tar.transform.position;
		shadow.gameObject.SetActive(true);
		slash.gameObject.SetActive(true);
		slash.Play("Slash");
		++state;
		timer = 0f;

		float damage = owner.chaInfo.attackPower * 2f;
		tar.TakeDamage((int)damage);

		float rad = tar.col.radius + owner.col.radius + 0.1f;
		Vector3 pos = tar.transform.position + (tar.transform.up * -rad);
		owner.transform.position = pos;
		owner.rb.velocity = Vector2.zero;
		owner.transform.rotation = GameManager.CalculateLookRotation(owner, tar);

		SoundManager.Instance.PlayFX(fx);
	}

	public override void End()
	{
		owner.col.isTrigger = false;
		base.End();
	}
}
