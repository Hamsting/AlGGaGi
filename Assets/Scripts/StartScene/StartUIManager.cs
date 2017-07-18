using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUIManager : MonoBehaviour {


    public string nickname;
    public void SetNickname(string nickname)
    {
        this.nickname = nickname;
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Register()
    {
        PSocketManager.Instance.Register(nickname);
    }

    public void Login()
    {
        PSocketManager.Instance.Login(PlayerPrefs.GetString("id"));
    }
}
