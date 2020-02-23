using Assets.SimpleAndroidNotifications;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class HelloTodayController : MonoBehaviour
{
    public Text reminderContent;
    public Text time;

    public GameObject pointerMinutes;
    public GameObject pointerHours;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;

    private int currPageIndex;

    private void Awake()
    {
        if (AppManager.instance != null)
        {
            if (AppManager.instance.currentUser.reminderRecord!=null && AppManager.instance.currentUser.reminderRecord.Count != 0)
            {
                currPageIndex = getClosestWithCurrentTimeReminderIndex();
            }
        }

    }


        // Update is called once per frame
        void Update()
    {
        if (AppManager.instance != null)
        {
            if (AppManager.instance.currentUser.reminderRecord != null && AppManager.instance.currentUser.reminderRecord.Count != 0)
            {
                reminderContent.fontStyle = FontStyle.Normal;
                time.gameObject.SetActive(true);
                pointerHours.gameObject.SetActive(true);
                pointerMinutes.gameObject.SetActive(true);


                
                reminderContent.text = AppManager.instance.currentUser.reminderRecord[currPageIndex].reminder_content;

                time.text = AppManager.instance.currentUser.reminderRecord[currPageIndex].reminderTime.ToString("hh:mm tt");

                Debug.Log(AppManager.instance.currentUser.reminderRecord[currPageIndex].reminderTime.ToShortTimeString());
                Debug.Log(AppManager.instance.currentUser.reminderRecord[currPageIndex].reminderTime);
                float rotationMinutes = (360.0f / 60.0f) * AppManager.instance.currentUser.reminderRecord[currPageIndex].reminder_time_minute;
                float rotationHours = ((360.0f / 12.0f) * AppManager.instance.currentUser.reminderRecord[currPageIndex].reminder_time_hour) + ((360.0f / (60.0f * 12.0f)) * AppManager.instance.currentUser.reminderRecord[0].reminder_time_minute);


                pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
                pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
            }
            else
            {
                reminderContent.text = "No Event";
                reminderContent.fontStyle = FontStyle.Italic;
                time.gameObject.SetActive(false);
                pointerHours.gameObject.SetActive(false);
                pointerMinutes.gameObject.SetActive(false);
            }
        }
    }

    public void NextReminder()
    {
        currPageIndex=(currPageIndex + 1) % AppManager.instance.currentUser.reminderRecord.Count;
    }
    public void PreviousReminder()
    {
        currPageIndex--;
        if (currPageIndex==-1)
        {
            currPageIndex = AppManager.instance.currentUser.reminderRecord.Count-1;
        }
    }


    private async Task MongoUpdateReminderRecord()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.reminderRecord, AppManager.instance.currentUser.reminderRecord));
    }

    private int getClosestWithCurrentTimeReminderIndex()
    {

        for (int i=0; i<AppManager.instance.currentUser.reminderRecord.Count; i++)
        {
            if (AppManager.instance.currentUser.reminderRecord[i].reminderTime.Hour > System.DateTime.Now.Hour)
            {
                Debug.Log(AppManager.instance.currentUser.reminderRecord[i].reminderTime.Hour);
                return i;
            }
            else if (AppManager.instance.currentUser.reminderRecord[i].reminderTime.Hour==System.DateTime.Now.Hour && 
                AppManager.instance.currentUser.reminderRecord[i].reminderTime.Minute>=System.DateTime.Now.Minute)
            {

                Debug.Log(AppManager.instance.currentUser.reminderRecord[i].reminderTime.Hour);
                return i;
            }

        }
        return 0;
    }


}
