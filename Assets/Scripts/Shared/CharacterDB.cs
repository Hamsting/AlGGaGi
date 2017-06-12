using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDB : MonoBehaviour
{
	private static CharacterDB _instance;
	public static CharacterDB Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("CharacterDB : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	public List<Character> characters;



	void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(this);
	}

	public Character GetCharacter(int _id)
	{
		for (int i = 0; i < characters.Count; ++i)
		{
			Character cha = characters[i];
			if (cha.id == _id)
				return cha;
		}
		return null;
	}

	public GameObject GetCharacterDollPrefab(int _id)
	{
		Character cha = GetCharacter(_id);
		return cha.dollPrefab.gameObject;
	}
}
