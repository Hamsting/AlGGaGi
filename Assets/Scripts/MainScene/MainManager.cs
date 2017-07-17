using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
	public AudioClip mainBgm;
	public Text goldText;

	private MapManager map;



	void Start ()
	{
		map = MapManager.Instance;
		map.Initialize();
		SoundManager.Instance.PlayBGM(mainBgm);
	}

	void Update ()
	{
		map.Tick();
		goldText.text = PlayerData.Instance.gold.ToString();
	}

	public void GoGameScene()
	{
		SceneManager.Instance.LoadGameScene();
	}
}
