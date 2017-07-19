using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_0009 : Skill
{
	public Animator shield;
	public AudioClip fx;



	public override void Use()
	{
		base.Use();

		this.transform.position = owner.transform.position;
		owner.defMode = true;
		shield.Play("Shield");
		SoundManager.Instance.PlayFX(fx);
	}

	public override void Tick()
	{
		base.Tick();

		if (timer >= 1.7f)
			End();
	}
}
