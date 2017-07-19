using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_0015 : Skill
{
	public Animator[] heal;
	public SpriteRenderer resSpr;
	public AnimationCurve curve;
	public AudioClip fx;

	private int resurrect = -1;



	public override void Use()
	{
		base.Use();
	
		for (int i = 0; i < 5; ++i)
		{
			CharacterDoll doll = null;
			if (owner.faction.isPlayer)
				doll = GameManager.Instance.playerDoll[i];
			else
				doll = GameManager.Instance.enemyDoll[i];

			if (doll.dead)
			{
				if (resurrect < 0)
					resurrect = i;
				else if (Random.Range(0, 2) == 1)
					resurrect = i;
			}
			else
			{
				doll.Heal((int)(doll.maxHp * 0.4f));
				heal[i].gameObject.SetActive(true);
				heal[i].transform.position = doll.transform.position + new Vector3(0f, 0f, -1f);
				heal[i].Play("Heal");
			}

			SoundManager.Instance.PlayFX(fx);
		}

		if (resurrect != -1)
		{
			CharacterDoll doll = null;
			if (owner.faction.isPlayer)
				doll = GameManager.Instance.playerDoll[resurrect];
			else
				doll = GameManager.Instance.enemyDoll[resurrect];

			doll.curHp = (int)(doll.maxHp * 0.2f);
			doll.portrait.SetNormalPortrait();
			doll.portrait.UpdateHpBar((float)doll.curHp / doll.maxHp);
			doll.dead = false;
			doll.gameObject.SetActive(true);
			resSpr.gameObject.SetActive(true);
			resSpr.transform.position = doll.transform.position + new Vector3(0f, 0f, -1f);
		}
	}

	public override void Tick()
	{
		base.Tick();

		if (resurrect != -1)
		{
			float t = curve.Evaluate(timer / 1f);
			Color c = new Color(1f, 1f, 1f, t);
			resSpr.color = c;
		}

		if (timer >= 1f)
			End();
	}
}
