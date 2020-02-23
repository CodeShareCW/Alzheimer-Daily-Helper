using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CaregiverMainScript : MonoBehaviour
{
    public InputField caregiver_Name;
    public InputField caregiver_PhoneNumber;
    public Button EditBtn;
    public Button okBtn;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;


    private void Awake()
    {
        if (AppManager.instance!=null)
            if (AppManager.instance.currentUser.caregiver!=null)
                if(AppManager.instance.currentUser.caregiver.caregiverName!=""&& AppManager.instance.currentUser.caregiver.caregiverPhoneNo != "")
                {
                    caregiver_Name.text = AppManager.instance.currentUser.caregiver.caregiverName;
                    caregiver_PhoneNumber.text = AppManager.instance.currentUser.caregiver.caregiverPhoneNo;
                }
        EditBtn.onClick.AddListener(Edit_Callback);
    }
    public void Edit_Callback()
    {
        EditBtn.gameObject.SetActive(false);
        okBtn.gameObject.SetActive(true);

        caregiver_Name.interactable = true;
        caregiver_PhoneNumber.interactable = true;

    }

    public async void OkBtn_Callback()
    {
        AppManager.instance.updatingMongoDB = true;
        if (!Regex.IsMatch(caregiver_PhoneNumber.text, StringPattern.PHONE_PATTERN))
        {
            msg_panel.SetActive(true);
            msg_title.text = "Edit Failed";
            msg_description.text = "Phone number should be in the format +60185554444 or +6018-5554444 or 60185554444";
        }
        else if (caregiver_Name.text != "" && caregiver_PhoneNumber.text != "")
        {
            UserAccount temp = AppManager.instance.currentUser;
            try
            {
                if (AppManager.instance.currentUser.caregiver == null)
                    AppManager.instance.currentUser.caregiver = new CaregiverProfile(caregiver_Name.text, caregiver_PhoneNumber.text);
                else
                {
                    AppManager.instance.currentUser.caregiver.caregiverName = caregiver_Name.text;
                    AppManager.instance.currentUser.caregiver.caregiverPhoneNo = caregiver_PhoneNumber.text;
                }
                await Task.Run(() => MongoUpdateCaregiver());
                SSTools.ShowMessage("Update Success", SSTools.Position.bottom, SSTools.Time.twoSecond);


            }
            catch (Exception e)
            {
                AppManager.instance.currentUser.caregiver = temp.caregiver;

                SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);

            }

            //caregiverCollection.InsertOne(caregiver); 
            caregiver_Name.interactable = false;
            caregiver_PhoneNumber.interactable = false;
            EditBtn.gameObject.SetActive(true);
            okBtn.gameObject.SetActive(false);

            



        }


        else
        {
            msg_panel.SetActive(true);
            msg_title.text = "Edit Failed";
            msg_description.text = "Please fill in all the field...";

            caregiver_PhoneNumber.text = "";
            caregiver_Name.text = "";

            caregiver_Name.interactable = false;
            caregiver_PhoneNumber.interactable = false;
            EditBtn.gameObject.SetActive(true);
            okBtn.gameObject.SetActive(false);
        }
        AppManager.instance.updatingMongoDB = false;
    }

    private async Task MongoUpdateCaregiver()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.caregiver, AppManager.instance.currentUser.caregiver));
    }
}
