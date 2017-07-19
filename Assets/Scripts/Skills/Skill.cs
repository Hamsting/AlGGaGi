using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
	public int id = 0000;
	public Sprite icon;
	public bool targeting = false;
	public int coolDown = 1;

	[HideInInspector]
	public CharacterDoll owner;
	[HideInInspector]
	public bool active = true;
	[HideInInspector]
	public float receivedForce;
	[HideInInspector]
	public Vector3 receivedPos;

	protected int state = 0;
	protected float timer = 0f;



	public virtual void Use()
	{
		active = true;
	}

	public virtual void Tick()
	{
		timer += Time.deltaTime;
	}

	public virtual void End()
	{
		active = false;
		GameManager.Instance.skills.Remove(this);
		Destroy(this.gameObject);
	}

	public virtual void OnTurnEnd()
	{
	}
}
