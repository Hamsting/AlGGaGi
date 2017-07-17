using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
	public AudioClip mainBgm;



	void Start ()
	{
		SoundManager.Instance.PlayBGM(mainBgm);
	}

	void Update ()
	{
		
	}

	public void GoGameScene()
	{
		SceneManager.Instance.LoadGameScene();
	}
}
