using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseWindow : MonoBehaviour
{
	public Image portrait;
	public Text chaName;
	public Text desc;
	public Text attackPower;
	public Text hp;
	public Text pushPower;
	public Text level;
	public Text defensePower;
	public Text price;
	public Button purchaseButton;
	public Text gold;


	[HideInInspector]
	public CharacterSlot slot;

	private Character targetCha;
	private Animator anim;



	void Awake()
	{
		anim = this.GetComponent<Animator>();
	}

	public void Open()
	{
		anim.Play("Open");
    }

	public void Close()
	{
		anim.Play("Close");
	}

	public void ActiveTrue()
	{
		this.gameObject.SetActive(true);
	}

	public void ActiveFalse()
	{
		this.gameObject.SetActive(false);
	}

	public void SetContents(Character _c)
	{
		targetCha = _c;
        int lv = PlayerData.Instance.chaLevel[_c.id - 1];

		portrait.sprite = _c.portrait;
		chaName.text = _c.name;
		desc.text = "";
		attackPower.text = ((int)(_c.attackPower)).ToString();
		hp.text = _c.hp.ToString();
		pushPower.text = _c.pushPower.ToString();
		level.text = lv.ToString();
		defensePower.text = _c.defensePower.ToString();
		price.text = lv.ToString();
		gold.text = PlayerData.Instance.gold.ToString();

		if (PlayerData.Instance.gold < lv)
		{
			purchaseButton.image.color = Color.gray;
			purchaseButton.interactable = false;
		}
		else
		{
			purchaseButton.image.color = Color.white;
			purchaseButton.interactable = true;
		}
	}

	public void OnPurchase()
	{
		int lv = PlayerData.Instance.chaLevel[targetCha.id - 1]++;
		PlayerData.Instance.gold -= lv++;
		PlayerData.Instance.SaveData();

		slot.level.text = lv.ToString();
		SetContents(targetCha.CalculateLevel(lv));
	}
}
