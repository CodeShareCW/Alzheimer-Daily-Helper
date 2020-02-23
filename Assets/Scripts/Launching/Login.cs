using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Threading.Tasks;

[BsonIgnoreExtraElements]
public class Login : MonoBehaviour
{
    UserAccount modelUser;
    public InputField phoneNo_input;
    public InputField password_input;

    public GameObject sendingDataPanel;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;


    public delegate void LoginUserSuccess();
    public static event LoginUserSuccess loginSuccessInfo;


    private bool isLoginSuccess = false;




    #region Interactable Input
    public void EnableInput()
    {
        InputField[] infields = GameObject.FindObjectsOfType<InputField>();
        foreach (var infield in infields)
        {
            infield.interactable = true;
        }
        Button[] btns = GameObject.FindObjectsOfType<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = true;
        }
    }

    public void DisableInput()
    {
        InputField[] infields = GameObject.FindObjectsOfType<InputField>();
        foreach (var infield in infields)
        {
            infield.interactable = false;
        }
        Button[] btns = GameObject.FindObjectsOfType<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = false;
        }
    }

    #endregion

    #region Login Callback
    public void Login_Callback()
    {
        Debug.Log("Login Callback");

        if (phoneNo_input.text == "" || password_input.text == "")
        {
            DisableInput();
            //show msg panel
            msg_panel.SetActive(true);
            EnableInput();
            msg_title.text = "Login failed";
            msg_description.text = "Please fill in all the field...";
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
            return;
        }
        //LoginUser(phoneNo_input.text, password_input.text);
        LoginUser(phoneNo_input.text, password_input.text);

        //  StartCoroutine(UserLogin(phoneNo_input.text, password_input.text));
    }
    #endregion

    #region Login User Async
    bool isLogin = false;

    private async void LoginUser(string phNo, string pwd)
    {
        AppManager.instance.updatingMongoDB = true;
        AppManager.instance.ConfigMongoDB();
        sendingDataPanel.SetActive(true);
        DisableInput();
        try
        {

            await Task.Run(() => (MongoCheckUserExist(phNo, pwd)));
        }
        catch(Exception e)
        {

            msg_panel.SetActive(true);
            msg_title.text = "Login Failed";
            msg_description.text = "Error: " + e.ToString();
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
            EnableInput();
            sendingDataPanel.SetActive(false);
            AppManager.instance.updatingMongoDB = false;
            return;
        }


        if (modelUser == null)
        {
            msg_panel.SetActive(true);
            msg_title.text = "Login Failed";
            msg_description.text = "Phone number do not exist...";

            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
        }
        else
        {
            Debug.Log("Checking Password");
            pwd = HelperScript.Sha256FromString(pwd);


            Debug.Log(pwd);
            Debug.Log(modelUser.hashPassword);

            if (pwd == modelUser.hashPassword)
            {
                try
                {
                    AppManager.instance.updatingMongoDB = true;

                    AppManager.instance.SetCurrentUser(modelUser);

                    TimeSpan timediff = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc).Subtract(AppManager.instance.currentUser.lastLogin);


                    if (timediff.Days == 0)
                    {
                        AppManager.instance.currentUser.isLoginDayCountDone = false;
                    }

                    else if (timediff.Days == 1&&!AppManager.instance.currentUser.isLoginDayCountDone)
                    {
                        AppManager.instance.currentUser.loginDayCount++;         //update the login count
                        AppManager.instance.currentUser.isLoginDayCountDone = true;
                    }
                    else if (timediff.Days>=2)
                    {
                        //we need to reset the continual login day count if they exceed 2 days not login
                        AppManager.instance.currentUser.loginDayCount = 0;
                    }


                    AppManager.instance.currentUser.lastLogin = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc);
                    AppManager.instance.currentUser.status = 1;


                    await Task.Run(() => (MongoUpdateStatusAndLastLogin(phNo, pwd)));
                   
                    await Task.Run(() => MongoUpdateLoginDayCount());

                    msg_panel.SetActive(true);
                    msg_title.text = "Login Success";
                    msg_description.text = "Welcome To Alzheimer Daily Helper. Press Ok to continue...";

                    


                    isLogin = true;

                }
                catch (Exception e)
                {
                    AppManager.instance.SetCurrentUser(null);

                    msg_panel.SetActive(true);
                    msg_title.text = "Login Failed";
                    msg_description.text = "Error: " + e.ToString();
                    msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
                }
            }
            else
            {
                msg_panel.SetActive(true);
                msg_title.text = "Login Failed";
                msg_description.text = "Wrong Password";
                msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
            }
        }
        EnableInput();
        sendingDataPanel.SetActive(false);
        AppManager.instance.updatingMongoDB = false;
    }
    #endregion



    #region Mongo DB Task 
    private async Task MongoCheckUserExist(string phNo, string pwd)
    {

        var FindUser= await AppManager.instance.userCollection.FindAsync(user => user.phoneNumber.Equals(phNo));
        modelUser = FindUser.SingleOrDefault();

    }

    private async Task MongoUpdateStatusAndLastLogin(string phNo, string pwd)
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == modelUser._id, Builders<UserAccount>.Update.Set(user => user.lastLogin, DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc)));
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == modelUser._id, Builders<UserAccount>.Update.Set(user => user.status, 1));
    }

    private async Task MongoUpdateLoginDayCount()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == modelUser._id, Builders<UserAccount>.Update.Set(user => user.isLoginDayCountDone, AppManager.instance.currentUser.isLoginDayCountDone));
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == modelUser._id, Builders<UserAccount>.Update.Set(user => user.loginDayCount, AppManager.instance.currentUser.loginDayCount));
    }

    

    #endregion
    private void Update()
    {
        if (msg_panel.GetComponent<MsgPanel>().isButtonPressed && isLogin)
            UnityEngine.SceneManagement.SceneManager.LoadScene(ActivityScene.HomeScene);

    }

}