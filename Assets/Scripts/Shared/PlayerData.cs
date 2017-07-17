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
		clearedStage = new int[MapManager.MAX_DIVIDE];
		chaLevel = new int[CharacterDB.Instance.characters.Count];
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
		for (int i = 0; i < MapManager.MAX_DIVIDE; ++i)
			PlayerPrefs.SetInt("Stage" + (i + 1).ToString(), clearedStage[i]);

		PlayerPrefs.Save();
	}

	public void LoadData()
	{
		gold = PlayerPrefs.GetInt("Gold", 0);
		rankScore = PlayerPrefs.GetInt("RankScore", 300);
		for (int i = 0; i < MapManager.MAX_DIVIDE; ++i)
			clearedStage[i] = PlayerPrefs.GetInt("Stage" + (i + 1).ToString(), 0);
	}
}
