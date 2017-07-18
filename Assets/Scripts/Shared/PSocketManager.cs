using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using socket.io;
using SimpleJSON;

public class PSocketManager : MonoBehaviour
{

    private const string SERVER_URL = "http://localhost:3144";

    [HideInInspector]
    public Socket socket;

    [HideInInspector]
    public User user;
    [HideInInspector]
    public User targetUser;
    [HideInInspector]
    public bool isStartTurn;



    private static PSocketManager _instance;
    public static PSocketManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("SocketManager : Instance is null.");
                return null;
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
        ConnectServer();
    }

    [HideInInspector]
    public bool isConnected = false;

    public void ConnectServer()
    {
        socket = Socket.Connect(SERVER_URL);

        #region Connect Listener
        socket.On("connect", () =>
        {
            Debug.Log("connect, 접속 완료!");
            isConnected = true;
        });

        socket.On("connectTimeOut", () =>
        {
            Debug.Log("connectTimeOut, 시간 초과 접속 실패!");
        });

        socket.On("reconnectAttempt", () =>
        {
            Debug.Log("reconnectAttempt, 재접속 시도!");
        });

        socket.On("reconnectFailed", () =>
        {
            Debug.Log("reconnectFailed, 재접속 실패!");
        });

        socket.On("reconnect", (int n) =>
        {

            Debug.LogFormat("reconnect, {0}회, 재접속 완료!", n);
        });

        socket.On("reconnecting", (int n) =>
        {
            Debug.LogFormat("reconnecting, {0}회, 재접속 중!", n);
        });

        socket.On("connectError", (Exception e) =>
        {
            Debug.Log("connectError, 접속 실패!");
            Debug.LogException(e);
        });

        socket.On("reconnectError", (Exception e) =>
        {
            Debug.Log("reconnectError, 재접속 실패!");
            Debug.LogException(e);
        });
        #endregion
    }

    public void Register(string nickname)
    {
        socket.Emit("register", nickname, str =>
        {
            var result = PSocketManager.parseArrayJson<WrapData<User>>(str);
            if (result.code == 200)
            {
                PlayerPrefs.SetString("id", result.data._id);
                user = result.data;
                SceneManager.Instance.LoadMainScene();
            }
            else
            {
                Debug.LogError(result.message);
            }
        });
    }

    public void Login(string id)
    {
        socket.Emit("login", id, str =>
        {
            var result = PSocketManager.parseArrayJson<WrapData<User>>(str);

            if (result.code == 200)
            {
                user = result.data;
                socket.Emit("match", "{}", (ackStr) =>
                {
                    var ackResult = PSocketManager.parseArrayJson<WrapData<GameInitData>>(ackStr);
                    if (ackResult.code == 200)
                    {
                        targetUser = ackResult.data.user;
                        isStartTurn = ackResult.data.startTurn == 1;
                        SceneManager.Instance.LoadGameScene();
                    }
                });
            }
            else
            {
                Debug.LogError(result.message);
            }
        });
    }


    public static T parseArrayJson<T>(string str)
    {
        return JsonUtility.FromJson<T>(JSON.Parse(str)[0].ToString());
    }
}

[Serializable]
[SerializeField]
public class User
{
    public string _id;
    public string nickname;
    public int rank;
}

[Serializable]
[SerializeField]
public class WrapData<T>
{
    public int code;
    public T data;
    public string message;
}

[Serializable]
[SerializeField]
public class GameInitData
{
    public User user;
    public int startTurn;
}