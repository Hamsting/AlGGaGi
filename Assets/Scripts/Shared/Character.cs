using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
	public int id = 0000;
	public string name = "";
	public int hp = 100;
	public float attackPower = 30;
	public int hpUpgrade = 1;
	public float attackUpgrade = 1;
	public float pushPower = 5.0f;
	public float defensePower = 3.0f;
	public CharacterDoll dollPrefab;
	public Sprite portrait;
	public Sprite ingame;
}
