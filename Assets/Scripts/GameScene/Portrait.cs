using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Portrait : MonoBehaviour
{
	public Image image;
	public Bar hpBar;
	public RectTransform rectTransform;
	public CharacterDoll doll;



	public void UpdateHpBar(float _per)
	{
		hpBar.Tick(_per);
	}

	public void SetDeadPortrait()
	{
		image.color = new Color32(165, 108, 108, 255);
	}
}
