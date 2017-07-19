using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{
	void Start ()
	{
		SceneManager.Instance.LoadMainScene();
	}
}
