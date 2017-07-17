using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
	public AudioClip mainBgm;

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
	}

	public void GoGameScene()
	{
		SceneManager.Instance.LoadGameScene();
	}
}
