using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ShowReminderController : MonoBehaviour
{
    private int currentPageIndex, totalPage;

    public GameObject ContentPanel, PageIndicatorPanel;
    public Text pageIndicatorText;

    public Text ContentPanel_Placeholder;
    public InputField reminderContent;
    public Button editBtn, delBtn, confirmEditBtn, cancelEditBtn;

    public GameObject confirmDeletePanel;
    public GameObject confirmClearAllRecordPanel;
    public Button confirmDeleteBtn, cancelDeleteBtn;
    public Button confirmClearAllBtn, cancelClearAllBtn;


    public Dropdown hour_DP, minute_DP, am_pm_DP;
    List<string> hour_options, minute_options, am_pm_options;

    public Button nextPageBtn, previousPagebtn;

    public GameObject msg_panel;
    public Text msg_title;
    public Text msg_description;

    public GameObject sendingDataPanel;

    private void Awake()
    {
        if (AppManager.instance.currentUser!=null)
        {
            
            if (AppManager.instance.currentUser.reminderRecord!=null&& AppManager.instance.currentUser.reminderRecord.Count!=0)
            {
                hour_options = new List<string>();
                minute_options = new List<string>();

                //add option
                for (int i = 1 % 12; i <= 12; i++)
                {
                    hour_options.Add(i.ToString("D2"));
                }
                for (int i = 0; i < 60; i++)
                {
                   minute_options.Add(i.ToString("D2"));

                }
                am_pm_options = new List<string>() { "AM", "PM" };

                hour_DP.AddOptions(hour_options);
                minute_DP.AddOptions(minute_options);
                am_pm_DP.AddOptions(am_pm_options);

                if (AppManager.instance.currentUser.reminderRecord.Count == 1)
                {
                    nextPageBtn.gameObject.SetActive(false);
                    previousPagebtn.gameObject.SetActive(false);
                }
                else
                {
                    nextPageBtn.gameObject.SetActive(true);
                    previousPagebtn.gameObject.SetActive(true);
                }
                currentPageIndex = 0;
                totalPage = AppManager.instance.currentUser.reminderRecord.Count;
                pageIndicatorText.text = (currentPageIndex + 1).ToString() + "/" + totalPage.ToString();


                AppManager.instance.currentUser.reminderRecord.Sort(new ReminderRecordNameComparer());

                reminderContent.text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_content;

                
                hour_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_hour-1;    //-1 because there is no 0 in hour, mean 1:00 to be 0, 2:00 to be 1 for the value 
                

                minute_DP.value= AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_minute;
                am_pm_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm=="AM"? 0 : 1;
                Debug.Log(AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm);

                reminderContent.interactable = false;
                hour_DP.interactable = false;
                minute_DP.interactable = false;
                am_pm_DP.interactable = false;

                confirmEditBtn.gameObject.SetActive(false);
                cancelEditBtn.gameObject.SetActive(false);
                editBtn.gameObject.SetActive(true);
                delBtn.gameObject.SetActive(true);

            }
            else
            {
                ContentPanel.SetActive(false);
                PageIndicatorPanel.SetActive(false);
                ContentPanel_Placeholder.gameObject.SetActive(true);
            }
        }
    }

    #region Callback
    public void EditBtn_Callback()
    {
        reminderContent.interactable = true;
        hour_DP.interactable = true;
        minute_DP.interactable = true;
        am_pm_DP.interactable = true;


        confirmEditBtn.gameObject.SetActive(true);
        cancelEditBtn.gameObject.SetActive(true);
        editBtn.gameObject.SetActive(false);
        delBtn.gameObject.SetActive(false);

    }

    public void DelBtn_Callback()
    {
        //pop out confirm message
        confirmDeletePanel.SetActive(true);
        InterableButtonInput(false);

    }

    public async void ConfirmDeleteBtn_Callback()
    {
        AppManager.instance.updatingMongoDB = true;
        UserAccount temp = AppManager.instance.currentUser;
        AppManager.instance.currentUser.reminderRecord.RemoveAt(currentPageIndex);
        if (AppManager.instance.currentUser.reminderRecord.Count==0)
        {
            ContentPanel.SetActive(false);
            ContentPanel_Placeholder.gameObject.SetActive(true);
            PageIndicatorPanel.SetActive(false);
        }
        else
        {
            if(AppManager.instance.currentUser.reminderRecord.Count==1)
            {
                nextPageBtn.gameObject.SetActive(false);
                previousPagebtn.gameObject.SetActive(false);
            }

            PreviousPageBtn_Callback();
        }
        try
        {
            sendingDataPanel.SetActive(true);
            reminderContent.interactable = false;
            await Task.Run(() => MongoUpdateReminderRecord());
            SSTools.ShowMessage("Delete Success", SSTools.Position.bottom, SSTools.Time.twoSecond);

        }
        catch (Exception e)
        {
            AppManager.instance.currentUser.reminderRecord = temp.reminderRecord;
            //error
            SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);


        }
        confirmDeletePanel.SetActive(false);
        confirmEditBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);


        InterableButtonInput(true);
        sendingDataPanel.SetActive(false);
        AppManager.instance.updatingMongoDB = false;
    }

    public void CancelDeleteBtn_Callback()
    {
        InterableButtonInput(true);
        confirmDeletePanel.SetActive(false);
    }

    public async void ConfirmEditBtn_Callback()
    {
        AppManager.instance.updatingMongoDB = true;
        UserAccount temp = AppManager.instance.currentUser;
        if (reminderContent.text!="")
        {
            if(reminderContent.text[0] != ' ')
            {
                Debug.Log(am_pm_DP.options[am_pm_DP.value].text.ToUpper());
                Debug.Log(AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm.ToUpper());
                if (reminderContent.text.ToUpper()==AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_content.ToUpper()
                    && hour_DP.options[hour_DP.value].text.ToUpper() == AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_hour.ToString("D2").ToUpper()
                    && minute_DP.options[minute_DP.value].text.ToUpper() == AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_minute.ToString("D2").ToUpper()
                    && am_pm_DP.options[am_pm_DP.value].text.ToUpper() == AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm.ToUpper())
                {
                    //unchange
                    SSTools.ShowMessage("No change", SSTools.Position.bottom, SSTools.Time.twoSecond);

                }
                else
                {
                    //edit reminder
                    AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_content = reminderContent.text;
                    AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_hour = int.Parse(hour_DP.options[hour_DP.value].text);
                    AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_minute = int.Parse(minute_DP.options[minute_DP.value].text);
                    AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm = am_pm_DP.options[am_pm_DP.value].text;

                    AppManager.instance.currentUser.reminderRecord.Sort(new ReminderRecordNameComparer());
                    try
                    {
                        sendingDataPanel.SetActive(true);
                        ButtonInteractable(false);
                        foreach (var i in FindObjectsOfType<InputField>())
                        {
                            i.interactable = false;
                        }
                        await Task.Run(() => MongoUpdateReminderRecord());
                        SSTools.ShowMessage("Edit Success", SSTools.Position.bottom, SSTools.Time.twoSecond);

                    }
                    catch (Exception e)
                    {
                        AppManager.instance.currentUser = temp;
                        //error
                        SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);



                        reminderContent.text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_content;
                        hour_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_hour - 1;    //-1 because there is no 0 in hour, mean 1:00 to be 0, 2:00 to be 1 for the value 
                        minute_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_minute;
                        am_pm_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm == "am" ? 0 : 1;
                    }
                }



            }
            else
            {
                //no allow space at first letter
                //pop out msg to warn
                msg_panel.SetActive(true);
                msg_title.text = "Edit Reminder Failed";
                msg_description.text = "Please don't put\"SPACE\" at first letter";
            }
        }
        else
        {
            //pop out msg to warn
            msg_panel.SetActive(true);
            msg_title.text = "Add Reminder Failed";
            msg_description.text = "Please fill in the reminder...";
        }

        reminderContent.interactable = false;
        hour_DP.interactable = false;
        minute_DP.interactable = false;
        am_pm_DP.interactable = false;


        confirmEditBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);


        ButtonInteractable(true);
        sendingDataPanel.SetActive(false);
        AppManager.instance.updatingMongoDB = false;
    }

    public void CancelEditBtn_Callback()
    {
        reminderContent.text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_content;
        hour_DP.options[hour_DP.value].text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_hour.ToString();
        minute_DP.options[minute_DP.value].text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_minute.ToString();
        am_pm_DP.options[am_pm_DP.value].text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm;

        reminderContent.interactable = false;
        hour_DP.interactable = false;
        minute_DP.interactable = false;
        am_pm_DP.interactable = false;


        confirmEditBtn.gameObject.SetActive(false);
        cancelEditBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        delBtn.gameObject.SetActive(true);
    }
    #endregion

    private async Task MongoUpdateReminderRecord()
    {
        await AppManager.instance.userCollection.UpdateOneAsync(user => user._id == AppManager.instance.currentUser._id, Builders<UserAccount>.Update.Set(user => user.reminderRecord, AppManager.instance.currentUser.reminderRecord));
    }


    #region PageSwitching
    public void PreviousPageBtn_Callback()
    {
        currentPageIndex--;
        if (currentPageIndex == -1)
            currentPageIndex = AppManager.instance.currentUser.reminderRecord.Count - 1;

        reminderContent.text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_content;
        hour_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_hour - 1;    //-1 because there is no 0 in hour, mean 1:00 to be 0, 2:00 to be 1 for the value 
        minute_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_minute;
        am_pm_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm == "AM" ? 0 : 1; //am is 0 index while pm is index 1

        totalPage = AppManager.instance.currentUser.reminderRecord.Count;
        pageIndicatorText.text = (currentPageIndex + 1).ToString() + "/" + totalPage.ToString();
    }

    public void NextPageBtn_Callback()
    {
        currentPageIndex = (currentPageIndex + 1) % AppManager.instance.currentUser.reminderRecord.Count;

        reminderContent.text = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_content;
        hour_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_hour - 1;    //-1 because there is no 0 in hour, mean 1:00 to be 0, 2:00 to be 1 for the value 
        minute_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_time_minute;
        am_pm_DP.value = AppManager.instance.currentUser.reminderRecord[currentPageIndex].reminder_am_pm == "AM" ? 0 : 1; //am is 0 index while pm is index 1


        Debug.Log(am_pm_DP.value);
        totalPage = AppManager.instance.currentUser.reminderRecord.Count;
        pageIndicatorText.text = (currentPageIndex + 1).ToString() + "/" + totalPage.ToString();

    }

    #endregion

    private void InterableButtonInput(bool inter)
    {
        foreach (var b in FindObjectsOfType<Button>())
        {
            if (b != confirmDeleteBtn && b != cancelDeleteBtn)
                b.interactable = inter;
        }
    }

    private void InterableButtonInput2(bool inter)
    {
        foreach (var b in FindObjectsOfType<Button>())
        {
            if (b != confirmClearAllBtn && b != cancelClearAllBtn)
                b.interactable = inter;
        }
    }

    private void ButtonInteractable(bool inter)
    {
        foreach (var b in FindObjectsOfType<Button>())
        {
             b.interactable = inter;
        }
    }


    public void ClearAllRecords_Callback()
    {
        confirmClearAllRecordPanel.SetActive(true);
        InterableButtonInput2(false);
    }

    public void CancelClearAllRecords_Callback()
    {
        confirmClearAllRecordPanel.SetActive(false);
        InterableButtonInput2(true);
    }

    public async void ConfirmClearAllRecords_Callback()
    {
        if (AppManager.instance.currentUser.reminderRecord != null && AppManager.instance.currentUser.reminderRecord.Count != 0)
        {

            AppManager.instance.currentUser.reminderRecord.Clear();
            AppManager.instance.currentUser.reminderRecord = null;
            try
            {
                AppManager.instance.updatingMongoDB = true;
                sendingDataPanel.SetActive(true);
                confirmClearAllRecordPanel.SetActive(false);
                ButtonInteractable(false);
                await Task.Run(() => MongoUpdateReminderRecord());
                SSTools.ShowMessage("Clear Up Success", SSTools.Position.bottom, SSTools.Time.twoSecond);

                ContentPanel.SetActive(false);
                PageIndicatorPanel.SetActive(false);
                ContentPanel_Placeholder.gameObject.SetActive(true);
                

            }
            catch (Exception e)
            {
                SSTools.ShowMessage("Some errors occurred", SSTools.Position.bottom, SSTools.Time.twoSecond);

            }
        }
        ButtonInteractable(true);
        AppManager.instance.updatingMongoDB = false;
        sendingDataPanel.SetActive(false);
    }
}
