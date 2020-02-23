using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class Register : MonoBehaviour
{
    UserAccount modelUser;

    public GameObject processingPanel;


    public InputField username;
    public InputField phoneNo;
    public InputField password;
    public InputField confirmPassword;

    public GameObject sendingDataPanel;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public RawImage texturedisp;


    #region RegisterValidation
    private bool isAllFieldFilled(string un, string phNo, string pwd, string conpwd)
    {
        if (un == "" || phNo == "" || pwd == "" || conpwd == "")
            return false;

        return true;
    }
    private bool isUsernameValid(string un)
    {
        return (!un.Contains(" ") && Regex.IsMatch(un, StringPattern.USERNAME_PATTERN));
    }

    private bool isPasswordValid(string pwd)
    {
        return Regex.IsMatch(pwd, StringPattern.PASSWORD_PATTERN);
    }
    private bool isPhoneValid(string hp)
    {
        return Regex.IsMatch(hp, StringPattern.PHONE_PATTERN);
    }
    private bool isConfirmPasswordMatched(string x1, string x2)
    {
        if (x1 != x2)
            return false;
        return true;
    }

    #endregion

    #region Submit Callback
    public void Submit_Callback()
    {
        if (!isAllFieldFilled(username.text, phoneNo.text, password.text, confirmPassword.text))
        {
            msg_panel.SetActive(true);
            msg_title.text = "Register Failed";
            msg_description.text = "Please fill in all the field...";
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
        }
        else if (!isUsernameValid(username.text))
        {
            msg_panel.SetActive(true);
            msg_title.text = "Register Failed";
            msg_description.text = "Username is invalid. Please do not put \"SPACE\" in your username and your username should be between 4 to 20 letters...";
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
        }
        else if (!isPhoneValid(phoneNo.text))
        {
            msg_panel.SetActive(true);
            msg_title.text = "Register Failed";
            msg_description.text = "Phone number is invalid. This application only accept Malaysia number. For example, 60185554444 or +60185554444 or +6018-5554444 is the valid input...";
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
        }
        else if (!isPasswordValid(password.text))
        {
            msg_panel.SetActive(true);
            msg_title.text = "Register Failed";
            msg_description.text = "Password is invalid. Please make sure your password contain 8 characters long containing at least 1 uppercase, 1 lowercase letter, and 1 special symbol (*&^%). E.g: MyName*123";
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
        }

        else if (!isConfirmPasswordMatched(password.text, confirmPassword.text))
        {
            msg_panel.SetActive(true);
            msg_title.text = "Register Failed";
            msg_description.text = "Confirm password is not matched...";
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
        }
        else
        {
            RegisterUser(username.text, phoneNo.text, password.text);
        }
    }
    #endregion

    bool isRegisterSuccess = false;

    #region Register Async
    private async void RegisterUser(string un, string phone, string pwd)
    {
        sendingDataPanel.SetActive(true);
        AppManager.instance.ConfigMongoDB();
        DisableInput();
        try
        {
            AppManager.instance.updatingMongoDB = true;
            await Task.Run(() => (MongoCheckUserExist(un, phone, pwd)));
        }
        catch (Exception e)
        {
            msg_panel.SetActive(true);
            msg_title.text = "Register Failed";
            msg_description.text = "Error: " + e.ToString();
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
            sendingDataPanel.SetActive(false);
            AppManager.instance.updatingMongoDB = false;
            return;
        }

        if (modelUser == null)
        { 
            try
            {
                AppManager.instance.updatingMongoDB = true;
                await Task.Run(() => (MongoAddUser(un, phone, pwd)));
                msg_panel.SetActive(true);
                msg_title.text = "Register Success";
                msg_description.text = "Please log in the account...";
                isRegisterSuccess = true;
            }
            catch (Exception e)
            {
                msg_panel.SetActive(true);
                msg_title.text = "Register Failed";
                msg_description.text = "Error: "+e.ToString();
                msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
                EnableInput();
            }
        }
        else
        {
            msg_panel.SetActive(true);
            msg_title.text = "Register Failed";
            msg_description.text = "Phone number exist...";
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
        }
        EnableInput();
        sendingDataPanel.SetActive(false);

        AppManager.instance.updatingMongoDB = false;
    }

    #endregion

    #region Asynchronous Mongo Task
    private async Task MongoCheckUserExist(string un, string phone, string pwd)
    {
        var MongoFindUser = await AppManager.instance.userCollection.FindAsync(user => user.phoneNumber.Equals(phone));
        modelUser=MongoFindUser.SingleOrDefault();
    }

    private async Task MongoAddUser(string un, string phone, string pwd)
    {
        //register the user
        pwd = HelperScript.Sha256FromString(pwd);
        UserAccount user = new UserAccount(un, phone, pwd);

        //Update create on
        user.CreateOn = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc);
        await AppManager.instance.userCollection.InsertOneAsync(user);
    }
    #endregion

    #region input Disable Or Enable
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

    private void Update()
    {
        if (msg_panel.GetComponent<MsgPanel>().isButtonPressed && isRegisterSuccess)
            UnityEngine.SceneManagement.SceneManager.LoadScene(LauchingInterfaceScene.EntryScene);
    }

}
