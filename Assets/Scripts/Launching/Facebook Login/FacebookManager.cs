using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacebookManager : MonoBehaviour
{
    public static FacebookManager instance;
    public UserAccount currFBUser;
    void MakeSingleton()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public bool IsLoggedIn { get; set; }
    private void Awake()
    {
        MakeSingleton();
        IsLoggedIn = true;
    }

    public void InitFB()
    {
        if (!FB.IsInitialized)
            FB.Init(SetInit, OnHideUnity);
        else
        {
            IsLoggedIn = FB.IsLoggedIn;
        }
    }

    void SetInit()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            Debug.Log("FB is logged in");
        }
        else
        {
            Debug.Log("FB is not logged in");
        }
    }

    void OnHideUnity(bool isGameShown)
    {
        if (isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }



}
