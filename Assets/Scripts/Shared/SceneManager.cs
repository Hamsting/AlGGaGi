using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
	private static SceneManager _instance;
	public static SceneManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("SceneManager : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	void Awake()
	{
		_instance = this;
		DontDestroyOnLoad(this);
	}

	public void LoadMainScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
	}

	public void LoadGameScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
	}
}
