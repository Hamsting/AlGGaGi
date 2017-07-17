using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	private static SoundManager _instance;
	public static SoundManager Instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.LogError("SoundManager : Instance is null.");
				return null;
			}
			return _instance;
		}
	}

	private AudioSource[] audios;



	void Awake()
	{
		_instance = this;
		audios = this.GetComponents<AudioSource>();
		audios[0].loop = true;
		DontDestroyOnLoad(this);
	}

	void Start()
	{
		
	}

	void Update()
	{
		
	}

	public void PlayBGM(AudioClip _bgm)
	{
		audios[0].Stop();
		audios[0].clip = _bgm;
		audios[0].Play();
	}

	public void PlayFX(AudioClip _fx)
	{
		audios[1].PlayOneShot(_fx);
	}
}
