using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using UnityEngine.UI;
using MongoDB.Bson;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;

public class FBScript : MonoBehaviour
{
    public GameObject FBLoginPanel;

    public Text currFBUsernameText, currFBEmailText;

    public RawImage currFBProfilePic;
    public Text currFBProfilePic_Placeholder;
    private string FBID, FBUsername, FBEmail;
    private byte[] FBProfilePicture;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;


    private void Awake()
    {
        FacebookManager.instance.InitFB();
    }


    public void FBLogin()
    {
        List<string> permissions=new List<string>();
        permissions.Add("public_profile");
        permissions.Add("email");
        FB.LogInWithReadPermissions(permissions, AuthCallback);
        //FB.LogInWithPublishPermissions(permissions, AuthCallback);

        /*
#if UNITY_EDITOR
        Facebook.Unity.Editor.Dialogs.MockLoginDialog mock = FindObjectsOfType<Facebook.Unity.Editor.Dialogs.MockLoginDialog>()[0];
        var type = typeof(Facebook.Unity.Editor.Dialogs.MockLoginDialog);
        var accessTokenField = type.GetField("accessToken", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        accessTokenField.SetValue(mock, "EAAjvvEekg4EBAKnDdxtisLmuFt7obZAD8yXO8RxovaFILDBVlWZBz42OgzgMMYcqZCNVcdQV5Tw2MeBSs5my26SJimoPSkRD7Yl3G8zMfJyZCcBoiwqrCFnx46ccu2csZCG1SIFjSkQrNvyIglbn19yuVtnmkgvBUIXZB2buYIprElAzCUnHv7yjDFgHqN02sZD");
        var sendSuccessResultMethod = type.GetMethod("SendSuccessResult", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        sendSuccessResultMethod.Invoke(mock, null);
        MonoBehaviour.Destroy(mock);
#endif
        */

    }

    void AuthCallback(IResult result)
    {
        if (result.Error!=null)
        {
            Debug.Log(result.Error);
            msg_panel.SetActive(true);
            msg_title.text = "Facebook Error";
            msg_description.text = "Error: " + result.Error;
        }
        else
        {
            if (FB.IsLoggedIn)
            {
                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;

                foreach(string perm in aToken.Permissions)
                {
                    Debug.Log(perm);
                }

                FacebookManager.instance.IsLoggedIn = true;
                Debug.Log("FB is logged in");
                DisplayFBLoginScene(FacebookManager.instance.IsLoggedIn);
            }
            else
            {
                FBLoginPanel.SetActive(false);
                Debug.Log("FB is not logged in");
                Debug.Log("User cancel login");
            }
            
        }



    }


    void DisplayFBLoginScene(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            FBLoginPanel.SetActive(true);
            FB.API("/me?fields=id, name, email", HttpMethod.GET, DisplayUserBasicInfo);
            FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayProfilePicture);
            
        }
        else
        {
            FBLoginPanel.SetActive(false);
        }
    }

    void DisplayUserBasicInfo(IResult result)
    {
        if (result.Error==null)
        {

            FBID= result.ResultDictionary["id"].ToString();
            FBUsername= result.ResultDictionary["name"].ToString();
            FBEmail= result.ResultDictionary["email"].ToString();

            currFBUsernameText.text = "Name: " + FBUsername;
            currFBEmailText.text = "Email: " + FBEmail;


            FacebookManager.instance.currFBUser = new UserAccount();
            FacebookManager.instance.currFBUser.username = result.ResultDictionary["name"].ToString();
            FacebookManager.instance.currFBUser.phoneNumber = result.ResultDictionary["email"].ToString();


           
        }
        else
        {
            msg_panel.SetActive(true);
            msg_title.text = "Facebook Error";
            msg_description.text = "Error: "+result.Error;
        }
    }
    void DisplayProfilePicture(IGraphResult result)
    {
        if (result.Texture != null)
        {
            currFBProfilePic_Placeholder.gameObject.SetActive(false);

            currFBProfilePic.texture = result.Texture as Texture2D;
            Texture2D tex = (Texture2D)currFBProfilePic.texture;

            FBProfilePicture = tex.EncodeToPNG();

            FacebookManager.instance.currFBUser.profile_picture = FBProfilePicture;
        }

        else
        {
            currFBProfilePic_Placeholder.gameObject.SetActive(true);
        }

    }



    public async void Continuebtn_Callback()
    {
        if (FacebookManager.instance.currFBUser != null)
        {

            AppManager.instance.updatingMongoDB = true;
            AppManager.instance.sendingDataPanel.SetActive(true);
            InteractableInput(false);
            try
            {
                await Task.Run(() => AppManager.instance.checkFBUserExist(FBID));
                await Task.Run(() => AppManager.instance.setupFBUser(FBID, FBUsername, FBEmail, FBProfilePicture));
                SceneManager.LoadScene(ActivityScene.HomeScene);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            AppManager.instance.sendingDataPanel.SetActive(false);
            InteractableInput(true);
            AppManager.instance.updatingMongoDB = false;
        }
    }

    private void InteractableInput(bool inter)
    {
        foreach(var b in FindObjectsOfType<Button>())
        {
            b.interactable = inter;
        }
    }

    public void Backbtn_Callback()
    {
        FBLoginPanel.SetActive(false);
        FacebookManager.instance.currFBUser = null;
        FB.LogOut();
    }

    

}
