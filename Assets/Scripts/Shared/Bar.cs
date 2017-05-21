using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
	private Image img;



	public void Initialize()
	{
		img = this.GetComponent<Image>();
	}

	public void Tick(float _percent01)
	{
		img.fillAmount = _percent01;
	}
}
