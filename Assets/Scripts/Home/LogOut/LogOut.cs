using Facebook.Unity;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LogOut : MonoBehaviour
{
    public GameObject sendingDataPanel;
    public GameObject logOutPanel;
    public Button OkBtn;
    public Button CancelBtn;

    private void Awake()
    {


        GetComponent<Button>().onClick.AddListener(LoggingOut);
        OkBtn.onClick.AddListener(ConfirmLogOut);
        CancelBtn.onClick.AddListener(CancelLogOut);
    }
    private void LoggingOut()
    {
        logOutPanel.SetActive(true);
    }

    private async void ConfirmLogOut()
    {
        UserAccount temp = AppManager.instance.currentUser;
        try
        {
            sendingDataPanel.SetActive(true);
            await Task.Run(() => MongoUpdateStatus());
            AppManager.instance.currentUser = null;
            FacebookManager.instance.currFBUser = null;
            FB.LogOut();
            UnityEngine.SceneManagement.SceneManager.LoadScene(LauchingInterfaceScene.EntryScene);
        }

        catch (Exception e)
        {
            AppManager.instance.currentUser=temp;
            Debug.Log("App Manager current user is null");
        }
        sendingDataPanel.SetActive(false);

    }

    private async Task MongoUpdateStatus()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.status, 0));
    }

    private void CancelLogOut()
    {
        SetEveryButtonInteratable(true);
        logOutPanel.SetActive(false);
    }

    void SetEveryButtonInteratable(bool ans)
    {
        Button[] btns = FindObjectsOfType<Button>();
        foreach (var b in btns)
        {
            if (b!=OkBtn&&b!=CancelBtn)
                b.interactable = ans;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoggingOut();
        }
    }
}
