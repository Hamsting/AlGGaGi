using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActSelect : MonoBehaviour
{
	public static readonly float MARGIN_X = 150f;

	public Button attack;
	public Button skill;
	public bool opened = false;

	private RectTransform rtAttack;
	private RectTransform rtSkill;
	private IEnumerator currentAnimate;
	private int attackType = 0;



	public void Initialize()
	{
		rtAttack = attack.GetComponent<RectTransform>();
		rtSkill = skill.GetComponent<RectTransform>();
	}

	public void Tick()
	{
		
	}

	public void Open()
	{
		opened = true;
		if (currentAnimate != null)
			StopCoroutine(currentAnimate);
		currentAnimate = OpenAnimate();
		StartCoroutine(currentAnimate);
	}

	public void Close()
	{
		opened = false;
        if (currentAnimate != null)
			StopCoroutine(currentAnimate);
		currentAnimate = CloseAnimation();
		StartCoroutine(currentAnimate);
	}

	private IEnumerator OpenAnimate()
	{
		rtAttack.localPosition = Vector2.zero;
		rtSkill.localPosition = Vector2.zero;
		bool animate = true;
		Vector2 aDist = new Vector2(-MARGIN_X, 0f);
		Vector2 sDist = new Vector2(MARGIN_X, 0f);
		yield return null;

		while (animate)
		{
			rtAttack.localPosition = Vector2.Lerp(rtAttack.localPosition, aDist, 0.5f);
			rtSkill.localPosition = Vector2.Lerp(rtSkill.localPosition, sDist, 0.5f);
			if (Vector2.Distance(rtAttack.localPosition, aDist) < 2f)
				animate = false;
			yield return null;
		}
		rtAttack.localPosition = aDist;
		rtSkill.localPosition = sDist;
		yield return null;
	}

	private IEnumerator CloseAnimation()
	{
		Vector2 aDist = new Vector2(-MARGIN_X, 0f);
		Vector2 sDist = new Vector2(MARGIN_X, 0f);
		rtAttack.localPosition = aDist;
		rtSkill.localPosition = sDist;
		bool animate = true;
		yield return null;

		DollController.Instance.attackType = attackType;
		attackType = 0;
		while (animate)
		{
			rtAttack.localPosition = Vector2.Lerp(rtAttack.localPosition, Vector2.zero, 0.5f);
			rtSkill.localPosition = Vector2.Lerp(rtSkill.localPosition, Vector2.zero, 0.5f);
			if (Vector2.Distance(rtAttack.localPosition, Vector2.zero) < 2f)
				animate = false;
			yield return null;
		}
		rtAttack.localPosition = Vector2.zero;
		rtSkill.localPosition = Vector2.zero;
		this.gameObject.SetActive(false);
		yield return null;
	}

	public void OnSelectAttackType(int _type)
	{
		attackType = _type;
		Close();
	}
}
