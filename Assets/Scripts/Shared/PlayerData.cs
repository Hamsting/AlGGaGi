using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
	private static PlayerData _instance;
	public static PlayerData Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("PlayerData : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	public List<Character> characters;
	public int gold = 0;
	public int rankScore = 300;



	void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(this);
		DebugInitialize();
	}

	void Start()
	{
	}

	private void DebugInitialize()
	{
		characters = new List<Character>();
		characters.Add(CharacterDB.Instance.GetCharacter(0001));
		characters.Add(CharacterDB.Instance.GetCharacter(0002));
		characters.Add(CharacterDB.Instance.GetCharacter(0003));
		characters.Add(CharacterDB.Instance.GetCharacter(0004));
		characters.Add(CharacterDB.Instance.GetCharacter(0005));
	}

	public void SaveData()
	{
		PlayerPrefs.SetInt("Gold", gold);
		PlayerPrefs.SetInt("RankScore", rankScore);

		PlayerPrefs.Save();
	}

	public void LoadData()
	{
		gold = PlayerPrefs.GetInt("Gold", 0);
		rankScore = PlayerPrefs.GetInt("RankScore", 300);
	}
}
