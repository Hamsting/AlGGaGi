using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *	! 주의 : 소스가 매우메우 많이 더러움.
 */

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

	/// <summary>
	/// 출전한 캐릭터들.
	/// </summary>
	public List<Character> characters;
	public int gold = 0;
	public int rankScore = 300;
	public int[] clearedStage;
	public int[] chaLevel;



	void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(this);
	}

	void Start()
	{
		clearedStage = new int[MapManager.MAX_DIVIDE];
		InitializeChaLevel();
		LoadData();
}

	public void SaveData()
	{
		PlayerPrefs.SetInt("Gold", gold);
		PlayerPrefs.SetInt("RankScore", rankScore);

		for (int i = 0; i < MapManager.MAX_DIVIDE; ++i)
			PlayerPrefs.SetInt("Stage" + (i + 1).ToString(), clearedStage[i]);

		string str = "";
		for (int i = 0; i < 5; ++i)
		{
			str += characters[i].id.ToString();
			if (i < 4)
				str += "|";
		}
		PlayerPrefs.SetString("JoinCharacter", str);

		str = "";
		int len = chaLevel.Length;
        for (int i = 0; i < len; ++i)
		{
			str += chaLevel[i].ToString();
			if (i < len - 1)
				str += "|";
		}
		PlayerPrefs.SetString("CharacterLevel", str);

		PlayerPrefs.Save();
	}

	public void LoadData()
	{
		gold = PlayerPrefs.GetInt("Gold", 0);
		rankScore = PlayerPrefs.GetInt("RankScore", 300);

		for (int i = 0; i < MapManager.MAX_DIVIDE; ++i)
			clearedStage[i] = PlayerPrefs.GetInt("Stage" + (i + 1).ToString(), 0);

		characters = new List<Character>();
		string str = PlayerPrefs.GetString("JoinCharacter", "1|6|9|14|15");
		string[] strs = str.Split('|');
		for (int i = 0; i < strs.Length; ++i)
			characters.Add(CharacterDB.Instance.GetCharacter(int.Parse(strs[i])));

		str = PlayerPrefs.GetString("CharacterLevel", "null");
		if (str.Equals("null"))
			InitializeChaLevel();
		else
		{
			strs = str.Split('|');
			for (int i = 0; i < strs.Length; ++i)
				chaLevel[i] = int.Parse(strs[i]);
		}
	}

	private void InitializeCharacters()
	{
		characters = new List<Character>();
		characters.Add(CharacterDB.Instance.GetCharacter(0001));
		characters.Add(CharacterDB.Instance.GetCharacter(0006));
		characters.Add(CharacterDB.Instance.GetCharacter(0009));
		characters.Add(CharacterDB.Instance.GetCharacter(0014));
		characters.Add(CharacterDB.Instance.GetCharacter(0015));
	}

	private void InitializeChaLevel()
	{
		chaLevel = new int[CharacterDB.Instance.characters.Count];
		chaLevel[0] = 1;
		chaLevel[5] = 1;
		chaLevel[8] = 1;
		chaLevel[13] = 1;
		chaLevel[14] = 1;
	}
}
