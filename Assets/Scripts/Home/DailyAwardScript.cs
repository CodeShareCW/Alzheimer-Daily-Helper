using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DailyAwardScript : MonoBehaviour
{
    private float floatAmplitude = 0.03f;
    private float floatFrequency = 0.5f;


    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;

    public Text dailyLoginAwardCountDownText;
    public GameObject dailyLoginAwardPanel;
    public GameObject particleEffectObject;
    public GameObject ImageObject;
    public Button OkBtn;

    private Vector3 startPosImage;


    private void Start()
    {
        startPosImage = ImageObject.transform.position;


    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public async void ReceiveAward()
    {
        AppManager.instance.updatingMongoDB = true;
        try
        {
            UserAccount temp = AppManager.instance.currentUser;

            try
            {
                sendingDataPanel.SetActive(true);
                AllButtonsInteractable(false);
                await Task.Run(() => MongoReceiveAwardUpdate());
                AllButtonsInteractable(true);
                AppManager.instance.currentUser.DailyAwardReceivedTime = DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc);
                AppManager.instance.currentUser.coins++;

                particleEffectObject.SetActive(false);
                dailyLoginAwardPanel.SetActive(true);
                ImageObject.SetActive(false);
                dailyLoginAwardCountDownText.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                particleEffectObject.SetActive(true);
                ImageObject.SetActive(true);
                dailyLoginAwardCountDownText.gameObject.SetActive(false);

                AppManager.instance.currentUser.DailyAwardReceivedTime = temp.DailyAwardReceivedTime;


                msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
                msg_panel.SetActive(true);
                msg_title.text = "Received Award Failed";
                msg_description.text = "Some connection problems...";
            }
            
        }
        catch(Exception e)
        {
            msg_panel.GetComponent<MsgPanel>().isButtonPressed = false;
            msg_panel.SetActive(true);
            msg_title.text = "App Manager faulty";
            msg_description.text = "Current User is empty";
        }
        AllButtonsInteractable(true);
        sendingDataPanel.SetActive(false);
        AppManager.instance.updatingMongoDB = false;
    }

    private async Task MongoReceiveAwardUpdate()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.DailyAwardReceivedTime, DateTime.SpecifyKind(System.DateTime.Now, DateTimeKind.Utc)));
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.coins, AppManager.instance.currentUser.coins+1));

    }

    public void AllButtonsInteractable(bool inter)
    {
        Button[] btns = FindObjectsOfType<Button>();
        foreach(var b in btns)
        {
            if (b!=OkBtn)
                b.interactable = inter;
        }
    }
    public void OkBtn_Callback()
    {
        AllButtonsInteractable(true);
        dailyLoginAwardPanel.SetActive(false);
    }
    private void Update()
    {
        if (AppManager.instance.currentUser != null)
        {
            if (AppManager.instance.currentUser.DailyAwardReceivedTime == DateTime.MinValue)
            {
                ImageObject.SetActive(true);
                particleEffectObject.SetActive(true);
                dailyLoginAwardCountDownText.gameObject.SetActive(false);

                particleEffectObject.SetActive(true);
                Vector3 temp = ImageObject.transform.position;

                temp.y += Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitude;

                ImageObject.transform.position = temp;
                dailyLoginAwardCountDownText.gameObject.SetActive(false);


            }
            else
            {
                var nextReceivedTime = AppManager.instance.currentUser.DailyAwardReceivedTime.AddDays(1);
                TimeSpan diffTime = nextReceivedTime.Subtract(System.DateTime.Now);
                Debug.Log(diffTime.Days);
                //check that next received time should be greater than 1, then spawn the award
                if (diffTime.Seconds < 0 || diffTime.Minutes < 0 || diffTime.Hours < 0)
                {
                    ImageObject.SetActive(true);
                    particleEffectObject.SetActive(true);
                    dailyLoginAwardCountDownText.gameObject.SetActive(false);

                    particleEffectObject.SetActive(true);
                    Vector3 temp = ImageObject.transform.position;

                    temp.y += Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitude;

                    ImageObject.transform.position = temp;
                    dailyLoginAwardCountDownText.gameObject.SetActive(false);

                }
                else
                {
                    ImageObject.SetActive(false);
                    particleEffectObject.SetActive(false);
                    dailyLoginAwardCountDownText.gameObject.SetActive(true);

                    string timeDiff_string = string.Format("{0:D2}:{1:D2}:{2:D2}", diffTime.Hours, diffTime.Minutes, diffTime.Seconds);

                    dailyLoginAwardCountDownText.text = "Your Next Award:\n" + timeDiff_string;
                }
            }
        }
    }
}
