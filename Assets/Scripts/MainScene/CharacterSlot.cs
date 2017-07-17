using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
	public Character cha;
	public Image portrait;
	public Button levelButton;
	public Button slotButton;
	public Text level;



	public void OnSlotSelected()
	{
		CorpsManager.Instance.OnSlotSelected(this);
	}
}
